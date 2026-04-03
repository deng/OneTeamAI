using Chatbot.Api.Domain.Entities;
using Chatbot.Api.Domain.Enums;
using Chatbot.Api.Integrations.Providers;

namespace Chatbot.Api.Services;

public sealed class AgentWorkflowExecutionService(
    IEnumerable<IFileKnowledgeProvider> fileKnowledgeProviders,
    IEnumerable<IProjectProvider> projectProviders,
    IEnumerable<ITicketProvider> ticketProviders)
{
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

        workflow.Summary = BuildConversationWorkflowSummary(workflow, conversation);
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

        workflow.Summary = BuildProjectWorkflowSummary(workflow, project);
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

        workflow.Summary = BuildWorkflowSummary(workflow, ticket);
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

    private static string BuildWorkflowSummary(AgentWorkflowRun workflow, Ticket ticket)
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

    private static string BuildConversationWorkflowSummary(AgentWorkflowRun workflow, Conversation conversation)
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

    private static string BuildProjectWorkflowSummary(AgentWorkflowRun workflow, Project project)
    {
        var names = string.Join(
            " -> ",
            workflow.Steps.OrderBy(x => x.Sequence).Select(x => x.Member?.DisplayName ?? "系统"));
        return $"已围绕项目“{project.Name}”执行协作链：{names}，并写入真实上下文与工具执行日志。";
    }
}
