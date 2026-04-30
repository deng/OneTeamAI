using Chatbot.Api.Domain.Entities;
using Chatbot.Api.Domain.Enums;
using Chatbot.Api.Integrations.Providers;
using System.Text.Json;

namespace Chatbot.Api.Services;

public sealed class AgentWorkflowExecutionService(
    IEnumerable<IFileKnowledgeProvider> fileKnowledgeProviders,
    IEnumerable<IProjectProvider> projectProviders,
    IEnumerable<ITicketProvider> ticketProviders,
    IWorkflowTextCompletionService workflowTextCompletionService,
    ILogger<AgentWorkflowExecutionService> logger)
{
    private const string StepResponseSchemaVersion = "workflow_step_v1";
    private const string SummaryResponseSchemaVersion = "workflow_summary_v1";

    public async Task EnrichAsync(
        AgentWorkflowRun workflow,
        Conversation conversation,
        IReadOnlyList<IntegrationConnection> integrationConnections,
        CancellationToken cancellationToken)
    {
        foreach (var step in workflow.Steps.OrderBy(x => x.Sequence))
        {
            step.InputSummary = EnrichConversationStepInputSummary(step, conversation);
            step.OutputSummary = EnrichConversationStepOutputSummary(step, conversation);

            foreach (var log in step.ExecutionLogs)
            {
                await EnrichConversationExecutionLogAsync(log, conversation, integrationConnections, cancellationToken);
            }
        }

        await GenerateAiOutputsAsync(
            workflow,
            BuildConversationContext(conversation),
            BuildConversationWorkflowFallbackSummary(workflow, conversation),
            cancellationToken);
    }

    public async Task EnrichAsync(
        AgentWorkflowRun workflow,
        Project project,
        IReadOnlyList<IntegrationConnection> integrationConnections,
        CancellationToken cancellationToken)
    {
        foreach (var step in workflow.Steps.OrderBy(x => x.Sequence))
        {
            step.InputSummary = EnrichProjectStepInputSummary(step, project);
            step.OutputSummary = EnrichProjectStepOutputSummary(step, project);

            foreach (var log in step.ExecutionLogs)
            {
                await EnrichProjectExecutionLogAsync(log, project, integrationConnections, cancellationToken);
            }
        }

        await GenerateAiOutputsAsync(
            workflow,
            BuildProjectContext(project),
            BuildProjectWorkflowFallbackSummary(workflow, project),
            cancellationToken);
    }

    public async Task EnrichAsync(
        AgentWorkflowRun workflow,
        Ticket ticket,
        IReadOnlyList<IntegrationConnection> integrationConnections,
        CancellationToken cancellationToken)
    {
        foreach (var step in workflow.Steps.OrderBy(x => x.Sequence))
        {
            step.InputSummary = EnrichStepInputSummary(step, ticket);
            step.OutputSummary = EnrichStepOutputSummary(step, ticket);

            foreach (var log in step.ExecutionLogs)
            {
                await EnrichExecutionLogAsync(log, ticket, integrationConnections, cancellationToken);
            }
        }

        await GenerateAiOutputsAsync(
            workflow,
            BuildTicketContext(ticket),
            BuildTicketWorkflowFallbackSummary(workflow, ticket),
            cancellationToken);
    }

    private async Task GenerateAiOutputsAsync(
        AgentWorkflowRun workflow,
        string businessContext,
        string fallbackSummary,
        CancellationToken cancellationToken)
    {
        var completedOutputs = new List<string>();

        foreach (var step in workflow.Steps.OrderBy(x => x.Sequence))
        {
            if (step.Member?.AiProfile is null || step.Status == AgentWorkflowStepStatus.Failed)
            {
                completedOutputs.Add($"{step.Sequence}. {step.Member?.DisplayName ?? "系统"}: {step.OutputSummary}");
                continue;
            }

            try
            {
                var response = await GenerateAiStepResponseAsync(
                    workflow,
                    step,
                    businessContext,
                    completedOutputs,
                    cancellationToken);
                ApplyAiStepResponse(step, response);
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    ex,
                    "Workflow step AI response failed. WorkflowId={WorkflowId} Sequence={Sequence} Member={MemberName}",
                    workflow.Id,
                    step.Sequence,
                    step.Member?.DisplayName);
                step.OutputSummary = TrimToLimit(step.OutputSummary, 2048) ?? step.OutputSummary;
                step.HandoffSummary = TrimToLimit(step.HandoffSummary, 1024) ?? step.HandoffSummary;
            }

            completedOutputs.Add($"{step.Sequence}. {step.Member?.DisplayName ?? "系统"}: {step.OutputSummary}");
        }

        workflow.Summary = await GenerateWorkflowSummaryAsync(
            workflow,
            businessContext,
            fallbackSummary,
            cancellationToken);
    }

    private async Task EnrichConversationExecutionLogAsync(
        AgentExecutionLog log,
        Conversation conversation,
        IReadOnlyList<IntegrationConnection> integrationConnections,
        CancellationToken cancellationToken)
    {
        if (!log.WasAllowed)
        {
            return;
        }

        switch (log.ToolName)
        {
            case "knowledge.search":
                await EnrichKnowledgeLogAsync(log, integrationConnections, cancellationToken);
                break;
            case "conversation.summary":
            case "conversation.list":
                var latestMessage = conversation.Messages.OrderByDescending(x => x.CreatedAt).FirstOrDefault();
                log.OutputSummary =
                    $"已读取会话，共 {conversation.Messages.Count} 条消息；最新消息：{latestMessage?.Content ?? "暂无内容"}。";
                break;
            case "project.list":
                if (conversation.ConciergeApp?.Project is not null)
                {
                    await EnrichProjectLogAsync(
                        log,
                        new Ticket
                        {
                            Project = conversation.ConciergeApp.Project,
                            Customer = conversation.Customer,
                        },
                        integrationConnections,
                        cancellationToken);
                }
                else
                {
                    log.OutputSummary = "当前会话未关联项目，项目建议仅能基于会话上下文给出。";
                }
                break;
            default:
                log.OutputSummary = $"{log.ToolName} 已按当前会话上下文执行，输出已汇总到步骤中。";
                break;
        }
    }

    private async Task EnrichProjectExecutionLogAsync(
        AgentExecutionLog log,
        Project project,
        IReadOnlyList<IntegrationConnection> integrationConnections,
        CancellationToken cancellationToken)
    {
        if (!log.WasAllowed)
        {
            return;
        }

        switch (log.ToolName)
        {
            case "knowledge.search":
                await EnrichKnowledgeLogAsync(log, integrationConnections, cancellationToken);
                break;
            case "project.list":
                {
                    var localSummary = $"当前项目阶段为 {project.StageLabel ?? "未设置"}，状态为 {project.Status}。";
                    var connection = integrationConnections.FirstOrDefault(x =>
                        x.IsEnabled && x.ExternalSystemType == ExternalSystemType.ErpNext);
                    if (connection is null)
                    {
                        log.OutputSummary = $"{localSummary} 当前未连接 ERPNext，因此项目建议基于本地上下文给出。";
                        return;
                    }

                    var provider = projectProviders.FirstOrDefault(x => x.CanHandle(connection.ExternalSystemType));
                    if (provider is null)
                    {
                        log.OutputSummary = $"{localSummary} 但系统未找到 ERPNext Project Provider。";
                        return;
                    }

                    var previews = await provider.ListProjectsAsync(provider.BuildDescriptor(connection), cancellationToken);
                    log.OutputSummary = $"{localSummary} ERPNext 当前可预览 {previews.Count} 个项目样本。";
                    break;
                }
            case "ticket.list":
                {
                    var connection = integrationConnections.FirstOrDefault(x =>
                        x.IsEnabled && x.ExternalSystemType == ExternalSystemType.ErpNext);
                    if (connection is null)
                    {
                        log.OutputSummary = "当前未连接 ERPNext，因此工单建议仅基于本地项目上下文给出。";
                        return;
                    }

                    var provider = ticketProviders.FirstOrDefault(x => x.CanHandle(connection.ExternalSystemType));
                    if (provider is null)
                    {
                        log.OutputSummary = "系统未找到 ERPNext Ticket Provider。";
                        return;
                    }

                    var previews = await provider.ListTicketsAsync(provider.BuildDescriptor(connection), cancellationToken);
                    log.OutputSummary = $"已结合项目视角预览 {previews.Count} 条外部工单样本。";
                    break;
                }
            default:
                log.OutputSummary = $"{log.ToolName} 已按当前项目上下文执行，输出已汇总到步骤中。";
                break;
        }
    }

    private async Task EnrichExecutionLogAsync(
        AgentExecutionLog log,
        Ticket ticket,
        IReadOnlyList<IntegrationConnection> integrationConnections,
        CancellationToken cancellationToken)
    {
        if (!log.WasAllowed)
        {
            return;
        }

        switch (log.ToolName)
        {
            case "knowledge.search":
                await EnrichKnowledgeLogAsync(log, integrationConnections, cancellationToken);
                break;
            case "conversation.summary":
            case "conversation.list":
                EnrichConversationLog(log, ticket);
                break;
            case "team.member.lookup":
                EnrichMemberLookupLog(log, ticket);
                break;
            case "ticket.list":
            case "ticket.update":
                await EnrichTicketLogAsync(log, ticket, integrationConnections, cancellationToken);
                break;
            case "project.list":
                await EnrichProjectLogAsync(log, ticket, integrationConnections, cancellationToken);
                break;
            default:
                log.OutputSummary = $"{log.ToolName} 已按当前上下文执行，输出已汇总到步骤中。";
                break;
        }
    }

    private async Task EnrichKnowledgeLogAsync(
        AgentExecutionLog log,
        IReadOnlyList<IntegrationConnection> integrationConnections,
        CancellationToken cancellationToken)
    {
        var connection = integrationConnections.FirstOrDefault(x =>
            x.IsEnabled && x.ExternalSystemType == ExternalSystemType.Nextcloud);
        if (connection is null)
        {
            log.Status = AgentExecutionLogStatus.Blocked;
            log.WasAllowed = false;
            log.OutputSummary = "团队未配置可用的 Nextcloud 连接，当前仅保留本地知识检索意图。";
            return;
        }

        var provider = fileKnowledgeProviders.FirstOrDefault(x => x.CanHandle(connection.ExternalSystemType));
        if (provider is null)
        {
            log.Status = AgentExecutionLogStatus.Blocked;
            log.WasAllowed = false;
            log.OutputSummary = "系统未找到 Nextcloud 文件知识 Provider。";
            return;
        }

        var files = await provider.ListFilesAsync(provider.BuildDescriptor(connection), "/", cancellationToken);
        var sample = string.Join(" / ", files.Take(3).Select(x => x.Name));
        log.OutputSummary =
            $"已从 {connection.Name} 检索到 {files.Count} 条文件预览，样本：{(string.IsNullOrWhiteSpace(sample) ? "暂无样本" : sample)}。";
    }

    private void EnrichConversationLog(AgentExecutionLog log, Ticket ticket)
    {
        var conversation = ticket.Conversation;
        if (conversation is null)
        {
            log.OutputSummary = "当前工单未关联会话，因此只基于工单摘要继续推进。";
            return;
        }

        var latestMessage = conversation.Messages
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefault();
        log.OutputSummary =
            $"已读取会话上下文，共 {conversation.Messages.Count} 条消息；最新消息：{latestMessage?.Content ?? "暂无内容"}。";
    }

    private void EnrichMemberLookupLog(AgentExecutionLog log, Ticket ticket)
    {
        var teamMembers = ticket.Team?.Members ?? [];
        var activeHumans = teamMembers.Count(x => x.MemberType == MemberType.Human && x.Status == MemberStatus.Active);
        var activeAi = teamMembers.Count(x => x.MemberType == MemberType.Ai && x.Status == MemberStatus.Active);
        log.OutputSummary = $"当前团队活跃成员 {teamMembers.Count} 人，其中真人 {activeHumans}、AI {activeAi}。";
    }

    private async Task EnrichTicketLogAsync(
        AgentExecutionLog log,
        Ticket ticket,
        IReadOnlyList<IntegrationConnection> integrationConnections,
        CancellationToken cancellationToken)
    {
        var localSummary =
            $"本地工单状态为 {ticket.Status}，优先级为 {ticket.Priority}，负责人为 {ticket.AssignedMember?.DisplayName ?? "未分配"}。";
        var connection = integrationConnections.FirstOrDefault(x =>
            x.IsEnabled && x.ExternalSystemType == ExternalSystemType.ErpNext);
        if (connection is null)
        {
            log.OutputSummary = $"{localSummary} 当前未连接 ERPNext，因此不做外部工单视图比对。";
            return;
        }

        var provider = ticketProviders.FirstOrDefault(x => x.CanHandle(connection.ExternalSystemType));
        if (provider is null)
        {
            log.OutputSummary = $"{localSummary} 但系统未找到 ERPNext Ticket Provider。";
            return;
        }

        var previews = await provider.ListTicketsAsync(provider.BuildDescriptor(connection), cancellationToken);
        log.OutputSummary =
            $"{localSummary} ERPNext 预览中还有 {previews.Count} 条工单样本，首条为 {previews.FirstOrDefault()?.DisplayName ?? "暂无"}。";
    }

    private async Task EnrichProjectLogAsync(
        AgentExecutionLog log,
        Ticket ticket,
        IReadOnlyList<IntegrationConnection> integrationConnections,
        CancellationToken cancellationToken)
    {
        var localSummary =
            $"当前工单关联项目 {ticket.Project?.Name ?? "未绑定项目"}，项目状态 {ticket.Project?.Status.ToString() ?? "未知"}。";
        var connection = integrationConnections.FirstOrDefault(x =>
            x.IsEnabled && x.ExternalSystemType == ExternalSystemType.ErpNext);
        if (connection is null)
        {
            log.OutputSummary = $"{localSummary} 当前未连接 ERPNext，因此项目建议基于本地上下文给出。";
            return;
        }

        var provider = projectProviders.FirstOrDefault(x => x.CanHandle(connection.ExternalSystemType));
        if (provider is null)
        {
            log.OutputSummary = $"{localSummary} 但系统未找到 ERPNext Project Provider。";
            return;
        }

        var previews = await provider.ListProjectsAsync(provider.BuildDescriptor(connection), cancellationToken);
        log.OutputSummary =
            $"{localSummary} ERPNext 当前可预览 {previews.Count} 个项目样本，便于后续做外部项目映射。";
    }

    private static string EnrichStepInputSummary(AgentWorkflowStep step, Ticket ticket)
    {
        var customer = ticket.Customer?.DisplayName ?? "匿名客户";
        var project = ticket.Project?.Name ?? "未绑定项目";
        return $"{step.InputSummary} 当前客户：{customer}；关联项目：{project}。";
    }

    private static string EnrichStepOutputSummary(AgentWorkflowStep step, Ticket ticket)
    {
        var assigned = ticket.AssignedMember?.DisplayName ?? "未分配";
        return $"{step.OutputSummary} 当前建议负责人：{assigned}。";
    }

    private async Task<string> GenerateWorkflowSummaryAsync(
        AgentWorkflowRun workflow,
        string businessContext,
        string fallbackSummary,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await GenerateAiSummaryResponseAsync(workflow, businessContext, cancellationToken);
            workflow.SummarySchemaVersion = response.SchemaVersion;
            workflow.SummaryRawResponse = TrimToLimit(response.RawResponse, 8192);
            workflow.SummaryAttemptTrace = SerializeAttemptTrace(response.Attempts);
            return TrimToLimit(response.Summary, 2048) ?? fallbackSummary;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Workflow summary AI response failed. WorkflowId={WorkflowId}", workflow.Id);
            return fallbackSummary;
        }
    }

    private static string BuildTicketWorkflowFallbackSummary(AgentWorkflowRun workflow, Ticket ticket)
    {
        var names = string.Join(
            " -> ",
            workflow.Steps
                .OrderBy(x => x.Sequence)
                .Select(x => x.Member?.DisplayName ?? "系统"));
        return $"已围绕工单“{ticket.Title}”执行协作链：{names}，并写入真实上下文与工具执行日志。";
    }

    private static string EnrichConversationStepInputSummary(AgentWorkflowStep step, Conversation conversation)
    {
        var customer = conversation.Customer?.DisplayName ?? "匿名客户";
        var appName = conversation.ConciergeApp?.Name ?? "未绑定坐台程序";
        return $"{step.InputSummary} 当前客户：{customer}；坐台程序：{appName}。";
    }

    private static string EnrichConversationStepOutputSummary(AgentWorkflowStep step, Conversation conversation)
    {
        var latestMessage = conversation.Messages.OrderByDescending(x => x.CreatedAt).FirstOrDefault()?.Content ?? "暂无消息";
        return $"{step.OutputSummary} 当前会话最新消息：{latestMessage}。";
    }

    private static string BuildConversationWorkflowFallbackSummary(AgentWorkflowRun workflow, Conversation conversation)
    {
        var names = string.Join(
            " -> ",
            workflow.Steps.OrderBy(x => x.Sequence).Select(x => x.Member?.DisplayName ?? "系统"));
        var target = conversation.Customer?.DisplayName ?? conversation.ConciergeApp?.Name ?? "匿名会话";
        return $"已围绕会话“{target}”执行协作链：{names}，并写入真实上下文与工具执行日志。";
    }

    private static string EnrichProjectStepInputSummary(AgentWorkflowStep step, Project project)
    {
        return $"{step.InputSummary} 当前阶段：{project.StageLabel ?? "未设置"}；项目状态：{project.Status}。";
    }

    private static string EnrichProjectStepOutputSummary(AgentWorkflowStep step, Project project)
    {
        return $"{step.OutputSummary} 当前风险：{project.RiskSummary ?? "暂无风险摘要"}。";
    }

    private static string BuildProjectWorkflowFallbackSummary(AgentWorkflowRun workflow, Project project)
    {
        var names = string.Join(
            " -> ",
            workflow.Steps.OrderBy(x => x.Sequence).Select(x => x.Member?.DisplayName ?? "系统"));
        return $"已围绕项目“{project.Name}”执行协作链：{names}，并写入真实上下文与工具执行日志。";
    }

    private static string BuildTicketContext(Ticket ticket)
    {
        var latestConversationMessage = ticket.Conversation?.Messages
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => x.Content)
            .FirstOrDefault();
        return string.Join(
            "\n",
            [
                $"对象类型：工单",
                $"标题：{ticket.Title}",
                $"摘要：{ticket.Summary ?? "暂无"}",
                $"状态：{ticket.Status}",
                $"优先级：{ticket.Priority}",
                $"客户：{ticket.Customer?.DisplayName ?? "匿名客户"}",
                $"项目：{ticket.Project?.Name ?? "未绑定项目"}",
                $"负责人：{ticket.AssignedMember?.DisplayName ?? "未分配"}",
                $"最近会话消息：{latestConversationMessage ?? "暂无"}"
            ]);
    }

    private static string BuildConversationContext(Conversation conversation)
    {
        var latestMessage = conversation.Messages
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => x.Content)
            .FirstOrDefault();
        return string.Join(
            "\n",
            [
                "对象类型：会话",
                $"客户：{conversation.Customer?.DisplayName ?? "匿名客户"}",
                $"坐台程序：{conversation.ConciergeApp?.Name ?? "未绑定"}",
                $"关联项目：{conversation.ConciergeApp?.Project?.Name ?? "未绑定项目"}",
                $"消息数：{conversation.Messages.Count}",
                $"最新消息：{latestMessage ?? "暂无"}"
            ]);
    }

    private static string BuildProjectContext(Project project)
    {
        return string.Join(
            "\n",
            [
                "对象类型：项目",
                $"名称：{project.Name}",
                $"阶段：{project.StageLabel ?? "未设置"}",
                $"状态：{project.Status}",
                $"摘要：{project.Summary ?? "暂无"}",
                $"风险：{project.RiskSummary ?? "暂无"}",
                $"下一步：{project.NextSteps ?? "暂无"}"
            ]);
    }

    private static string BuildStepPrompt(
        AgentWorkflowRun workflow,
        AgentWorkflowStep step,
        string businessContext,
        IReadOnlyList<string> previousOutputs,
        string? validationFeedback = null)
    {
        var profile = step.Member?.AiProfile;
        var schemaExample =
            "{\n"
            + $"  \"schemaVersion\": \"{StepResponseSchemaVersion}\",\n"
            + "  \"status\": \"completed\",\n"
            + "  \"output\": \"这一岗位本轮产出的核心结论\",\n"
            + "  \"handoff\": \"给下一岗位或老板的交接内容\"\n"
            + "}";
        var tools = step.ExecutionLogs.Count == 0
            ? "无"
            : string.Join(
                "\n",
                step.ExecutionLogs.Select(log =>
                    $"- {log.ToolName} [{log.Status}] 输入: {TrimToLimit(log.InputSummary, 180) ?? "无"} 输出: {TrimToLimit(log.OutputSummary, 240) ?? "无"}"));
        var previous = previousOutputs.Count == 0
            ? "无前序结论"
            : string.Join("\n", previousOutputs.Select(text => $"- {TrimToLimit(text, 240)}"));
        var handoffTarget = step.HandoffToMember?.DisplayName ?? "真人老板";

        return
            $"""
            你正在扮演团队里的 AI 员工，请基于岗位身份输出当前步骤的真实工作结果。

            工作流目标：
            {workflow.Goal}

            业务对象上下文：
            {businessContext}

            当前岗位：
            - 成员：{step.Member?.DisplayName ?? "系统"}
            - 职位：{profile?.JobTitle ?? step.Member?.Title ?? "未设置"}
            - 责任：{profile?.ResponsibilitySummary ?? "未设置"}
            - 权限边界：{profile?.PermissionBoundary ?? "未设置"}
            - 系统提示词：{profile?.SystemPrompt ?? "未设置"}

            当前步骤输入：
            {step.InputSummary}

            前序结论：
            {previous}

            可用工具及结果：
            {tools}

            交接对象：
            {handoffTarget}

            输出规则：
            - 只能输出单个 JSON 对象。
            - 不要 Markdown，不要代码块，不要额外解释，不要前后缀文字。
            - schemaVersion 必须固定为 "{StepResponseSchemaVersion}"。
            - status 只能是 "completed" 或 "blocked"。
            - output 和 handoff 必须是非空中文字符串。

            目标 JSON Schema：
            {schemaExample}
            {(string.IsNullOrWhiteSpace(validationFeedback) ? string.Empty : $"\n上一次输出未通过校验：{validationFeedback}\n请严格按上述 JSON Schema 重新生成。")}
            """;
    }

    private static string BuildWorkflowSummaryPrompt(
        AgentWorkflowRun workflow,
        string businessContext,
        string? validationFeedback = null)
    {
        var schemaExample =
            "{\n"
            + $"  \"schemaVersion\": \"{SummaryResponseSchemaVersion}\",\n"
            + "  \"summary\": \"中文业务摘要\"\n"
            + "}";
        var orderedSteps = workflow.Steps
            .OrderBy(x => x.Sequence)
            .Select(step =>
                $"- {step.Sequence}. {step.Member?.DisplayName ?? "系统"}: {TrimToLimit(step.OutputSummary, 240) ?? "无输出"}");

        return
            $"""
            请基于以下工作流执行结果，生成一条适合写回业务对象的协作摘要。

            工作流类型：{workflow.WorkflowType}
            工作流目标：{workflow.Goal}

            业务对象上下文：
            {businessContext}

            步骤输出：
            {string.Join("\n", orderedSteps)}

            输出规则：
            - 只能输出单个 JSON 对象。
            - 不要 Markdown，不要代码块，不要额外解释，不要前后缀文字。
            - schemaVersion 必须固定为 "{SummaryResponseSchemaVersion}"。
            - summary 必须是 80 到 160 字中文摘要，强调结论、风险和下一步。

            目标 JSON Schema：
            {schemaExample}
            {(string.IsNullOrWhiteSpace(validationFeedback) ? string.Empty : $"\n上一次输出未通过校验：{validationFeedback}\n请严格按上述 JSON Schema 重新生成。")}
            """;
    }

    private async Task<WorkflowStepAiResponse> GenerateAiStepResponseAsync(
        AgentWorkflowRun workflow,
        AgentWorkflowStep step,
        string businessContext,
        IReadOnlyList<string> previousOutputs,
        CancellationToken cancellationToken)
    {
        string? validationFeedback = null;
        string? latestRawResponse = null;
        var attempts = new List<AiResponseAttemptTrace>();

        for (var attempt = 1; attempt <= 2; attempt++)
        {
            latestRawResponse = await workflowTextCompletionService.CompleteTextAsync(
                "你是企业工作流中的结构化执行助手。你必须严格按用户指定的 JSON 格式输出，不要输出 Markdown、解释或额外文字。",
                BuildStepPrompt(workflow, step, businessContext, previousOutputs, validationFeedback),
                cancellationToken);
            logger.LogInformation(
                "Workflow step AI response received. WorkflowId={WorkflowId} Sequence={Sequence} Member={MemberName} Attempt={Attempt} Raw={RawResponse}",
                workflow.Id,
                step.Sequence,
                step.Member?.DisplayName,
                attempt,
                latestRawResponse);

            if (TryParseAiStepResponse(latestRawResponse, out var response, out validationFeedback))
            {
                attempts.Add(new AiResponseAttemptTrace(
                    attempt,
                    "accepted",
                    response.SchemaVersion,
                    null,
                    TrimToLimit(latestRawResponse, 8192),
                    DateTimeOffset.UtcNow));
                return response with { Attempts = attempts };
            }

            attempts.Add(new AiResponseAttemptTrace(
                attempt,
                "rejected",
                ExtractJsonField(latestRawResponse, "schemaVersion"),
                TrimToLimit(validationFeedback, 1024),
                TrimToLimit(latestRawResponse, 8192),
                DateTimeOffset.UtcNow));

            logger.LogWarning(
                "Workflow step AI response invalid. WorkflowId={WorkflowId} Sequence={Sequence} Member={MemberName} Attempt={Attempt} Reason={Reason}",
                workflow.Id,
                step.Sequence,
                step.Member?.DisplayName,
                attempt,
                validationFeedback);
        }

        var fallbackLines = latestRawResponse?
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            ?? [];
        var output = ExtractJsonField(latestRawResponse, "output") ?? ExtractTaggedValue(latestRawResponse, "OUTPUT");
        var handoff = ExtractJsonField(latestRawResponse, "handoff") ?? ExtractTaggedValue(latestRawResponse, "HANDOFF");
        var status = ExtractJsonField(latestRawResponse, "status") ?? ExtractTaggedValue(latestRawResponse, "STATUS");

        return new WorkflowStepAiResponse(
            TrimToLimit(output, 2048) ?? TrimToLimit(fallbackLines.FirstOrDefault(), 2048) ?? step.OutputSummary,
            TrimToLimit(handoff, 1024) ?? TrimToLimit(fallbackLines.Skip(1).FirstOrDefault(), 1024) ?? step.HandoffSummary,
            ExtractJsonField(latestRawResponse, "schemaVersion"),
            latestRawResponse ?? string.Empty,
            attempts,
            string.Equals(status, "blocked", StringComparison.OrdinalIgnoreCase)
                ? AgentWorkflowStepStatus.Failed
                : AgentWorkflowStepStatus.Completed);
    }

    private async Task<WorkflowSummaryAiResponse> GenerateAiSummaryResponseAsync(
        AgentWorkflowRun workflow,
        string businessContext,
        CancellationToken cancellationToken)
    {
        string? validationFeedback = null;
        string? latestRawResponse = null;
        var attempts = new List<AiResponseAttemptTrace>();

        for (var attempt = 1; attempt <= 2; attempt++)
        {
            latestRawResponse = await workflowTextCompletionService.CompleteTextAsync(
                "你是企业工作流摘要助手。你必须严格按用户指定的 JSON 格式输出，不要输出 Markdown、解释或额外文字。",
                BuildWorkflowSummaryPrompt(workflow, businessContext, validationFeedback),
                cancellationToken);
            logger.LogInformation(
                "Workflow summary AI response received. WorkflowId={WorkflowId} Attempt={Attempt} Raw={RawResponse}",
                workflow.Id,
                attempt,
                latestRawResponse);

            if (TryParseAiSummaryResponse(latestRawResponse, out var response, out validationFeedback))
            {
                attempts.Add(new AiResponseAttemptTrace(
                    attempt,
                    "accepted",
                    response.SchemaVersion,
                    null,
                    TrimToLimit(latestRawResponse, 8192),
                    DateTimeOffset.UtcNow));
                return response with { Attempts = attempts };
            }

            attempts.Add(new AiResponseAttemptTrace(
                attempt,
                "rejected",
                ExtractJsonField(latestRawResponse, "schemaVersion"),
                TrimToLimit(validationFeedback, 1024),
                TrimToLimit(latestRawResponse, 8192),
                DateTimeOffset.UtcNow));

            logger.LogWarning(
                "Workflow summary AI response invalid. WorkflowId={WorkflowId} Attempt={Attempt} Reason={Reason}",
                workflow.Id,
                attempt,
                validationFeedback);
        }

        var summary = ExtractJsonField(latestRawResponse, "summary")
            ?? ExtractTaggedValue(latestRawResponse, "SUMMARY")
            ?? latestRawResponse?
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .FirstOrDefault()
            ?? string.Empty;
        return new WorkflowSummaryAiResponse(
            summary,
            ExtractJsonField(latestRawResponse, "schemaVersion"),
            latestRawResponse ?? string.Empty,
            attempts);
    }

    private static void ApplyAiStepResponse(AgentWorkflowStep step, WorkflowStepAiResponse response)
    {
        step.OutputSchemaVersion = response.SchemaVersion;
        step.OutputRawResponse = TrimToLimit(response.RawResponse, 8192);
        step.OutputAttemptTrace = SerializeAttemptTrace(response.Attempts);
        step.OutputSummary = TrimToLimit(response.Output, 2048) ?? step.OutputSummary;
        step.HandoffSummary = TrimToLimit(response.Handoff, 1024) ?? step.HandoffSummary;
        step.Status = response.Status;
    }

    private static string? ExtractTaggedValue(string? rawResponse, string tag)
    {
        if (string.IsNullOrWhiteSpace(rawResponse))
        {
            return null;
        }

        var prefix = $"{tag}:";
        var line = rawResponse
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault(item => item.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
        if (line is null)
        {
            return null;
        }

        return line[prefix.Length..].Trim();
    }

    private static string? ExtractJsonField(string? rawResponse, string fieldName)
    {
        var jsonText = ExtractJsonObject(rawResponse);
        if (jsonText is null)
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(jsonText);
            if (!document.RootElement.TryGetProperty(fieldName, out var element))
            {
                return null;
            }

            return element.ValueKind == JsonValueKind.String
                ? element.GetString()
                : element.ToString();
        }
        catch
        {
            return null;
        }
    }

    private static string? ExtractJsonObject(string? rawResponse)
    {
        if (string.IsNullOrWhiteSpace(rawResponse))
        {
            return null;
        }

        var trimmed = rawResponse.Trim();
        if (trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            var lines = trimmed
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Where(line => !line.TrimStart().StartsWith("```", StringComparison.Ordinal))
                .ToArray();
            trimmed = string.Join("\n", lines).Trim();
        }

        var start = trimmed.IndexOf('{');
        var end = trimmed.LastIndexOf('}');
        if (start < 0 || end <= start)
        {
            return null;
        }

        return trimmed[start..(end + 1)];
    }

    private static bool TryParseAiStepResponse(
        string? rawResponse,
        out WorkflowStepAiResponse response,
        out string? validationError)
    {
        response = default!;
        validationError = null;

        if (!TryParseJsonRoot(rawResponse, out var root, out validationError))
        {
            return false;
        }

        var schemaVersion = GetRequiredString(root, "schemaVersion");
        if (!string.Equals(schemaVersion, StepResponseSchemaVersion, StringComparison.Ordinal))
        {
            validationError = $"schemaVersion 必须是 {StepResponseSchemaVersion}";
            return false;
        }

        var output = GetRequiredString(root, "output");
        if (output is null)
        {
            validationError = "output 必须是非空字符串";
            return false;
        }

        var handoff = GetRequiredString(root, "handoff");
        if (handoff is null)
        {
            validationError = "handoff 必须是非空字符串";
            return false;
        }

        var statusText = GetRequiredString(root, "status");
        if (!string.Equals(statusText, "completed", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(statusText, "blocked", StringComparison.OrdinalIgnoreCase))
        {
            validationError = "status 只能是 completed 或 blocked";
            return false;
        }

        response = new WorkflowStepAiResponse(
            output,
            handoff,
            StepResponseSchemaVersion,
            rawResponse ?? string.Empty,
            [],
            string.Equals(statusText, "blocked", StringComparison.OrdinalIgnoreCase)
                ? AgentWorkflowStepStatus.Failed
                : AgentWorkflowStepStatus.Completed);
        return true;
    }

    private static bool TryParseAiSummaryResponse(
        string? rawResponse,
        out WorkflowSummaryAiResponse response,
        out string? validationError)
    {
        response = default!;
        validationError = null;

        if (!TryParseJsonRoot(rawResponse, out var root, out validationError))
        {
            return false;
        }

        var schemaVersion = GetRequiredString(root, "schemaVersion");
        if (!string.Equals(schemaVersion, SummaryResponseSchemaVersion, StringComparison.Ordinal))
        {
            validationError = $"schemaVersion 必须是 {SummaryResponseSchemaVersion}";
            return false;
        }

        var summary = GetRequiredString(root, "summary");
        if (summary is null)
        {
            validationError = "summary 必须是非空字符串";
            return false;
        }

        response = new WorkflowSummaryAiResponse(summary, SummaryResponseSchemaVersion, rawResponse ?? string.Empty, []);
        return true;
    }

    private static bool TryParseJsonRoot(
        string? rawResponse,
        out JsonElement root,
        out string? validationError)
    {
        root = default;
        validationError = null;
        var jsonText = ExtractJsonObject(rawResponse);
        if (jsonText is null)
        {
            validationError = "未找到 JSON 对象";
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(jsonText);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                validationError = "根节点必须是 JSON 对象";
                return false;
            }

            root = document.RootElement.Clone();
            return true;
        }
        catch (Exception ex)
        {
            validationError = $"JSON 解析失败: {ex.Message}";
            return false;
        }
    }

    private static string? GetRequiredString(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var element) || element.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        return TrimToLimit(element.GetString(), 4096);
    }

    private static string? TrimToLimit(string? value, int maxLength)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return null;
        }

        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }

    private static string? SerializeAttemptTrace(IReadOnlyList<AiResponseAttemptTrace> attempts)
    {
        if (attempts.Count == 0)
        {
            return null;
        }

        return JsonSerializer.Serialize(attempts);
    }

    private sealed record WorkflowStepAiResponse(
        string Output,
        string Handoff,
        string? SchemaVersion,
        string RawResponse,
        IReadOnlyList<AiResponseAttemptTrace> Attempts,
        AgentWorkflowStepStatus Status);

    private sealed record WorkflowSummaryAiResponse(
        string Summary,
        string? SchemaVersion,
        string RawResponse,
        IReadOnlyList<AiResponseAttemptTrace> Attempts);

    private sealed record AiResponseAttemptTrace(
        int Attempt,
        string Outcome,
        string? SchemaVersion,
        string? ValidationError,
        string? RawResponse,
        DateTimeOffset CreatedAt);
}
