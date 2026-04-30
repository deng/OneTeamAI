using Chatbot.Api.Domain.Entities;
using Chatbot.Api.Domain.Enums;

namespace Chatbot.Api.Services;

public sealed class AgentWorkflowOrchestrator
{
    public AgentWorkflowRun CreateConversationWorkflow(
        Team team,
        Conversation conversation,
        IReadOnlyList<Member> aiMembers,
        Guid? requestedByUserId,
        Guid? startedByMemberId,
        string? goal,
        AgentWorkflowTriggerMode triggerMode)
    {
        var orderedMembers = SelectWorkflowMembers(aiMembers);
        var workflowGoal = string.IsNullOrWhiteSpace(goal)
            ? $"围绕会话“{conversation.Customer?.DisplayName ?? "匿名客户"}”完成接待、意图整理和后续建议。"
            : goal.Trim();

        var run = new AgentWorkflowRun
        {
            TeamId = team.Id,
            ProjectId = conversation.ConciergeApp?.ProjectId,
            ConversationId = conversation.Id,
            RequestedByUserId = requestedByUserId,
            StartedByMemberId = startedByMemberId,
            WorkflowType = "conversation-collaboration",
            TriggerMode = triggerMode,
            Goal = workflowGoal,
            Summary = BuildConversationWorkflowSummary(conversation, orderedMembers),
            Status = AgentWorkflowStatus.Completed,
            CompletedAt = DateTimeOffset.UtcNow,
        };

        var steps = BuildConversationSteps(conversation, orderedMembers);
        FinalizeRun(run, steps);
        return run;
    }

    public AgentWorkflowRun CreateProjectWorkflow(
        Team team,
        Project project,
        IReadOnlyList<Member> aiMembers,
        Guid? requestedByUserId,
        Guid? startedByMemberId,
        string? goal,
        AgentWorkflowTriggerMode triggerMode)
    {
        var orderedMembers = SelectWorkflowMembers(aiMembers);
        var workflowGoal = string.IsNullOrWhiteSpace(goal)
            ? $"围绕项目“{project.Name}”完成现状总结、风险评估和下一步建议。"
            : goal.Trim();

        var run = new AgentWorkflowRun
        {
            TeamId = team.Id,
            ProjectId = project.Id,
            RequestedByUserId = requestedByUserId,
            StartedByMemberId = startedByMemberId,
            WorkflowType = "project-collaboration",
            TriggerMode = triggerMode,
            Goal = workflowGoal,
            Summary = BuildProjectWorkflowSummary(project, orderedMembers),
            Status = AgentWorkflowStatus.Completed,
            CompletedAt = DateTimeOffset.UtcNow,
        };

        var steps = BuildProjectSteps(project, orderedMembers);
        FinalizeRun(run, steps);
        return run;
    }

    public AgentWorkflowRun CreateTicketWorkflow(
        Team team,
        Ticket ticket,
        IReadOnlyList<Member> aiMembers,
        Guid? requestedByUserId,
        Guid? startedByMemberId,
        string? goal,
        AgentWorkflowTriggerMode triggerMode)
    {
        var orderedMembers = SelectWorkflowMembers(aiMembers);
        var workflowGoal = string.IsNullOrWhiteSpace(goal)
            ? $"围绕工单“{ticket.Title}”完成多智能体分诊、协调和推进建议。"
            : goal.Trim();

        var run = new AgentWorkflowRun
        {
            TeamId = team.Id,
            ProjectId = ticket.ProjectId,
            ConversationId = ticket.ConversationId,
            TicketId = ticket.Id,
            RequestedByUserId = requestedByUserId,
            StartedByMemberId = startedByMemberId,
            WorkflowType = "ticket-collaboration",
            TriggerMode = triggerMode,
            Goal = workflowGoal,
            Summary = BuildWorkflowSummary(ticket, orderedMembers),
            Status = AgentWorkflowStatus.Completed,
            CompletedAt = DateTimeOffset.UtcNow,
        };

        var steps = BuildSteps(ticket, orderedMembers);
        FinalizeRun(run, steps);
        return run;
    }

    private static void FinalizeRun(AgentWorkflowRun run, List<AgentWorkflowStep> steps)
    {
        for (var index = 0; index < steps.Count; index++)
        {
            steps[index].WorkflowRun = run;
            steps[index].Sequence = index + 1;
        }

        foreach (var step in steps)
        {
            foreach (var log in BuildExecutionLogs(step, step.Member))
            {
                step.ExecutionLogs.Add(log);
            }

            run.Steps.Add(step);
        }
    }

    private static List<Member> SelectWorkflowMembers(IReadOnlyList<Member> aiMembers)
    {
        var ordered = new List<Member>();

        void TryAddByTemplate(string templateKey)
        {
            var member = aiMembers.FirstOrDefault(x => x.AiProfile?.TemplateKey == templateKey);
            if (member is not null && ordered.All(existing => existing.Id != member.Id))
            {
                ordered.Add(member);
            }
        }

        TryAddByTemplate("front-desk");
        TryAddByTemplate("ticket-coordinator");
        TryAddByTemplate("project-assistant");

        foreach (var member in aiMembers)
        {
            if (ordered.Count >= 3)
            {
                break;
            }

            if (ordered.All(existing => existing.Id != member.Id))
            {
                ordered.Add(member);
            }
        }

        return ordered;
    }

    private static string BuildWorkflowSummary(Ticket ticket, IReadOnlyList<Member> orderedMembers)
    {
        var names = orderedMembers.Count > 0
            ? string.Join(" -> ", orderedMembers.Select(member => member.DisplayName))
            : "未匹配到 AI 员工";
        return $"已围绕工单“{ticket.Title}”执行多智能体协作链：{names}。";
    }

    private static string BuildConversationWorkflowSummary(Conversation conversation, IReadOnlyList<Member> orderedMembers)
    {
        var names = orderedMembers.Count > 0
            ? string.Join(" -> ", orderedMembers.Select(member => member.DisplayName))
            : "未匹配到 AI 员工";
        var target = conversation.Customer?.DisplayName ?? conversation.ConciergeApp?.Name ?? "匿名会话";
        return $"已围绕会话“{target}”执行多智能体协作链：{names}。";
    }

    private static string BuildProjectWorkflowSummary(Project project, IReadOnlyList<Member> orderedMembers)
    {
        var names = orderedMembers.Count > 0
            ? string.Join(" -> ", orderedMembers.Select(member => member.DisplayName))
            : "未匹配到 AI 员工";
        return $"已围绕项目“{project.Name}”执行多智能体协作链：{names}。";
    }

    private static List<AgentWorkflowStep> BuildSteps(Ticket ticket, IReadOnlyList<Member> orderedMembers)
    {
        var steps = new List<AgentWorkflowStep>();
        var now = DateTimeOffset.UtcNow;

        if (orderedMembers.Count == 0)
        {
            steps.Add(new AgentWorkflowStep
            {
                ActionType = "workflow-unavailable",
                InputSummary = $"工单“{ticket.Title}”没有可用的 AI 员工参与。",
                OutputSummary = "请先为团队配置前台接待、工单协调或项目助理等 AI 员工后再重试。",
                HandoffSummary = "未发生交接。",
                Status = AgentWorkflowStepStatus.Failed,
                ExecutedAt = now,
            });
            return steps;
        }

        for (var index = 0; index < orderedMembers.Count; index++)
        {
            var member = orderedMembers[index];
            var nextMember = index + 1 < orderedMembers.Count ? orderedMembers[index + 1] : null;
            var templateKey = member.AiProfile?.TemplateKey ?? "general-ai";

            steps.Add(new AgentWorkflowStep
            {
                MemberId = member.Id,
                Member = member,
                HandoffToMemberId = nextMember?.Id,
                HandoffToMember = nextMember,
                ActionType = templateKey,
                InputSummary = BuildInputSummary(ticket, member, index),
                OutputSummary = BuildOutputSummary(ticket, member, index),
                HandoffSummary = nextMember is null
                    ? "协作链在该步骤收口，等待真人老板或执行成员确认。"
                    : $"将当前判断与下一步建议交接给 {nextMember.DisplayName}。",
                Status = AgentWorkflowStepStatus.Completed,
                ExecutedAt = now.AddSeconds(index),
            });
        }

        return steps;
    }

    private static List<AgentWorkflowStep> BuildConversationSteps(Conversation conversation, IReadOnlyList<Member> orderedMembers)
    {
        var steps = new List<AgentWorkflowStep>();
        var now = DateTimeOffset.UtcNow;

        if (orderedMembers.Count == 0)
        {
            steps.Add(new AgentWorkflowStep
            {
                ActionType = "workflow-unavailable",
                InputSummary = "当前会话没有可用的 AI 员工参与。",
                OutputSummary = "请先配置可用 AI 员工后再发起会话协作。",
                HandoffSummary = "未发生交接。",
                Status = AgentWorkflowStepStatus.Failed,
                ExecutedAt = now,
            });
            return steps;
        }

        for (var index = 0; index < orderedMembers.Count; index++)
        {
            var member = orderedMembers[index];
            var nextMember = index + 1 < orderedMembers.Count ? orderedMembers[index + 1] : null;
            var templateKey = member.AiProfile?.TemplateKey ?? "general-ai";
            steps.Add(new AgentWorkflowStep
            {
                MemberId = member.Id,
                Member = member,
                HandoffToMemberId = nextMember?.Id,
                HandoffToMember = nextMember,
                ActionType = templateKey,
                InputSummary = index switch
                {
                    0 => $"读取会话消息和客户资料，先确认客户意图与沟通阶段。由 {member.DisplayName} 负责首轮接待分析。",
                    1 => $"承接前一位 AI 的判断，识别是否需要转工单、转人工或补充追问。",
                    _ => "结合前序结论，给出项目或客户经营层面的下一步建议。"
                },
                OutputSummary = index switch
                {
                    0 => $"{member.DisplayName} 已完成会话首轮摘要，整理了客户问题、意图和缺失信息。",
                    1 => $"{member.DisplayName} 输出了继续跟进、转工单或转人工的建议。",
                    _ => $"{member.DisplayName} 给出了后续客户经营与项目推进建议。"
                },
                HandoffSummary = nextMember is null
                    ? "协作链在该步骤收口，等待真人老板确认。"
                    : $"将会话理解和建议交接给 {nextMember.DisplayName}。",
                Status = AgentWorkflowStepStatus.Completed,
                ExecutedAt = now.AddSeconds(index),
            });
        }

        return steps;
    }

    private static List<AgentWorkflowStep> BuildProjectSteps(Project project, IReadOnlyList<Member> orderedMembers)
    {
        var steps = new List<AgentWorkflowStep>();
        var now = DateTimeOffset.UtcNow;

        if (orderedMembers.Count == 0)
        {
            steps.Add(new AgentWorkflowStep
            {
                ActionType = "workflow-unavailable",
                InputSummary = $"当前项目“{project.Name}”没有可用的 AI 员工参与。",
                OutputSummary = "请先配置可用 AI 员工后再发起项目协作。",
                HandoffSummary = "未发生交接。",
                Status = AgentWorkflowStepStatus.Failed,
                ExecutedAt = now,
            });
            return steps;
        }

        for (var index = 0; index < orderedMembers.Count; index++)
        {
            var member = orderedMembers[index];
            var nextMember = index + 1 < orderedMembers.Count ? orderedMembers[index + 1] : null;
            var templateKey = member.AiProfile?.TemplateKey ?? "general-ai";
            steps.Add(new AgentWorkflowStep
            {
                MemberId = member.Id,
                Member = member,
                HandoffToMemberId = nextMember?.Id,
                HandoffToMember = nextMember,
                ActionType = templateKey,
                InputSummary = index switch
                {
                    0 => $"读取项目阶段、摘要、风险和下一步信息，先做项目现状梳理。由 {member.DisplayName} 负责首轮汇总。",
                    1 => $"承接上一位 AI 的结论，继续评估工单与资源推进节奏，识别阻塞点。",
                    _ => "输出面向老板的项目节奏建议、风险提醒和下一步行动。"
                },
                OutputSummary = index switch
                {
                    0 => $"{member.DisplayName} 已完成项目背景和当前阶段摘要。",
                    1 => $"{member.DisplayName} 已整理项目中的阻塞点、待协同事项和优先级建议。",
                    _ => $"{member.DisplayName} 已输出下一阶段项目推进建议。"
                },
                HandoffSummary = nextMember is null
                    ? "协作链在该步骤收口，等待真人老板确认。"
                    : $"将项目总结和建议交接给 {nextMember.DisplayName}。",
                Status = AgentWorkflowStepStatus.Completed,
                ExecutedAt = now.AddSeconds(index),
            });
        }

        return steps;
    }

    private static string BuildInputSummary(Ticket ticket, Member member, int index)
    {
        return index switch
        {
            0 => $"读取工单标题“{ticket.Title}”与摘要，先理解客户诉求、紧急度和当前上下文。由 {member.DisplayName} 负责首轮分诊。",
            1 => $"承接上一位 AI 的分诊结果，基于工单状态 {ticket.Status} 和优先级 {ticket.Priority} 进一步给出负责人建议与推进策略。",
            _ => $"结合前序 AI 的结论，输出项目层面的风险、依赖和下一步执行建议。"
        };
    }

    private static string BuildOutputSummary(Ticket ticket, Member member, int index)
    {
        return index switch
        {
            0 => $"{member.DisplayName} 判断该工单核心目标是“{ticket.Title}”，建议先确认客户预期、交付范围和时效要求。",
            1 => $"{member.DisplayName} 建议将工单维持在 {TicketStatus.InProgress}，并优先由具备对应岗位能力的成员跟进，同时保留人工复核。",
            _ => $"{member.DisplayName} 输出项目推进建议：拆成明确待办、同步风险点，并在客户下一次反馈后更新结论。"
        };
    }

    private static IReadOnlyList<AgentExecutionLog> BuildExecutionLogs(AgentWorkflowStep step, Member? member)
    {
        if (member?.AiProfile is null)
        {
            return
            [
                new AgentExecutionLog
                {
                    MemberId = member?.Id,
                    ToolName = "workflow.system",
                    ToolCategory = "system",
                    BoundarySummary = "没有 AI 岗位配置，无法推导工具边界。",
                    InputSummary = step.InputSummary,
                    OutputSummary = "该步骤由系统兜底生成，没有真实 AI 工具调用。",
                    Status = AgentExecutionLogStatus.Blocked,
                    WasAllowed = false,
                    ExecutedAt = step.ExecutedAt,
                }
            ];
        }

        var allowedTools = (member.AiProfile.AllowedTools ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (allowedTools.Length == 0)
        {
            return
            [
                new AgentExecutionLog
                {
                    MemberId = member.Id,
                    ToolName = "tool.unassigned",
                    ToolCategory = "policy",
                    BoundarySummary = member.AiProfile.PermissionBoundary ?? "未设置权限边界",
                    InputSummary = step.InputSummary,
                    OutputSummary = "该 AI 员工没有配置可调用工具，因此只输出文字建议。",
                    Status = AgentExecutionLogStatus.Blocked,
                    WasAllowed = false,
                    ExecutedAt = step.ExecutedAt,
                }
            ];
        }

        return allowedTools
            .Select((tool, index) => new AgentExecutionLog
            {
                MemberId = member.Id,
                ToolName = tool,
                ToolCategory = ResolveToolCategory(tool),
                BoundarySummary = member.AiProfile.PermissionBoundary ?? "未设置权限边界",
                InputSummary = index == 0
                    ? step.InputSummary
                    : $"基于前一个工具输出，继续执行 {tool}。",
                OutputSummary = $"工具 {tool} 已在当前权限边界内执行，结论被汇总到步骤输出中。",
                Status = AgentExecutionLogStatus.Succeeded,
                WasAllowed = true,
                ExecutedAt = step.ExecutedAt?.AddMilliseconds(index * 120),
            })
            .ToList();
    }

    private static string ResolveToolCategory(string toolName)
    {
        if (toolName.Contains("knowledge", StringComparison.OrdinalIgnoreCase) ||
            toolName.Contains("search", StringComparison.OrdinalIgnoreCase))
        {
            return "knowledge";
        }

        if (toolName.Contains("ticket", StringComparison.OrdinalIgnoreCase))
        {
            return "ticket";
        }

        if (toolName.Contains("project", StringComparison.OrdinalIgnoreCase))
        {
            return "project";
        }

        if (toolName.Contains("conversation", StringComparison.OrdinalIgnoreCase))
        {
            return "conversation";
        }

        if (toolName.Contains("team", StringComparison.OrdinalIgnoreCase) ||
            toolName.Contains("member", StringComparison.OrdinalIgnoreCase))
        {
            return "team";
        }

        return "general";
    }
}
