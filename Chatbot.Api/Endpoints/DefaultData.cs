partial class Program
{
    static async Task EnsureDefaultAiMemberTemplatesAsync(
        AppDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        var existingKeys = await dbContext.AiMemberTemplates
            .Where(template => template.TeamId == null)
            .Select(template => template.Key)
            .ToListAsync(cancellationToken);

        var existingKeySet = existingKeys.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missingTemplates = GetDefaultAiMemberTemplates()
            .Where(template => !existingKeySet.Contains(template.Key))
            .ToList();

        if (missingTemplates.Count == 0)
        {
            return;
        }

        dbContext.AiMemberTemplates.AddRange(missingTemplates);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    static List<AiMemberTemplate> GetDefaultAiMemberTemplates() =>
    [
        new()
        {
            Key = "front-desk",
            Label = "前台接待 AI",
            DisplayName = "前台接待 AI",
            JobTitle = "客户接待专员",
            ResponsibilitySummary = "负责首次接待客户、确认来意、收集联系方式，并把明确需求整理成后续动作。",
            Title = "Front Desk AI",
            PermissionBoundary = "只能处理接待、摘要整理和基础分流，不能直接承诺报价或交付日期。",
            SystemPrompt = "你是团队的前台接待 AI，回答要清晰、礼貌、结构化，先收集完整信息再推进下一步。",
            AllowedTools = "knowledge.search, conversation.summary",
            ExecutableActions = "接待客户,整理需求,推荐下一步,触发工单建议",
            KnowledgeScope = "project-docs,faqs",
            IsAutonomous = false,
            IsBuiltIn = true,
            IsEnabled = true,
            SortOrder = 100,
        },
        new()
        {
            Key = "ticket-coordinator",
            Label = "工单协调 AI",
            DisplayName = "工单协调 AI",
            JobTitle = "工单协调专员",
            ResponsibilitySummary = "负责判断优先级、推荐负责人、推动工单进入正确状态。",
            Title = "Ticket Ops AI",
            PermissionBoundary = "可以更新工单状态和建议负责人，但不能关闭高优先级工单。",
            SystemPrompt = "你是团队的工单协调 AI，要优先保证工单信息完整、负责人明确、状态准确。",
            AllowedTools = "ticket.list,ticket.update,team.member.lookup",
            ExecutableActions = "工单分类,优先级评估,推荐负责人,更新状态",
            KnowledgeScope = "ticket-rules,team-members",
            IsAutonomous = true,
            IsBuiltIn = true,
            IsEnabled = true,
            SortOrder = 200,
        },
        new()
        {
            Key = "project-assistant",
            Label = "项目助理 AI",
            DisplayName = "项目助理 AI",
            JobTitle = "项目助理",
            ResponsibilitySummary = "负责整理项目上下文、汇总会话和工单，帮助老板快速了解项目状态。",
            Title = "Project Assistant AI",
            PermissionBoundary = "只能总结和建议，不直接修改客户信息或对外发送承诺。",
            SystemPrompt = "你是项目助理 AI，擅长总结、提炼风险、生成下一步建议。",
            AllowedTools = "project.list,ticket.list,conversation.list",
            ExecutableActions = "总结进展,输出风险,整理待办,生成日报",
            KnowledgeScope = "project-docs,tickets,conversations",
            IsAutonomous = false,
            IsBuiltIn = true,
            IsEnabled = true,
            SortOrder = 300,
        },
    ];

    static List<WorkflowTemplateResponse> GetDefaultWorkflowTemplates() =>
    [
        new(
            "customer-intake",
            "conversation",
            "客户接待链",
            "围绕当前会话整理客户意图、判断是否需要建单，并输出后续跟进建议。",
            "适合用于官网咨询、首次接待和线索筛选。"),
        new(
            "ticket-triage",
            "ticket",
            "工单分诊链",
            "围绕当前工单完成优先级判断、负责人建议、推进动作和风险说明。",
            "适合用于新工单分派、处理中工单复盘和升级判断。"),
        new(
            "project-followup",
            "project",
            "项目跟进链",
            "围绕当前项目输出现状摘要、风险提醒、依赖项和下一步建议。",
            "适合用于老板查看项目进展、周报前整理和风险对齐。"),
    ];
}
