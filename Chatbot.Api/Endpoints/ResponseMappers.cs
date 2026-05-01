partial class Program
{
    static UserResponse ToUserResponse(User user) =>
        new(user.Id, user.Email, user.DisplayName, user.CompanyName);

    static AuditLogResponse ToAuditLogResponse(AuditLog log) =>
        new(
            log.Id,
            log.TeamId,
            log.UserId,
            log.User?.DisplayName,
            log.ActionType,
            log.EntityType,
            log.EntityId,
            log.Summary,
            log.Result,
            log.IpAddress,
            log.CreatedAt);

    static UserSessionResponse ToUserSessionResponse(UserSession session, Guid currentSessionId) =>
        new(
            session.Id,
            session.CreatedAt,
            session.LastSeenAt,
            session.ExpiresAt,
            session.RevokedAt,
            session.RevokedReason,
            session.UserAgent,
            session.IpAddress,
            session.Id == currentSessionId);

    static InvitationResponse ToInvitationResponse(TeamInvitation invitation) =>
        new(
            invitation.Id,
            invitation.TeamId,
            invitation.Team?.Name ?? string.Empty,
            invitation.Email,
            invitation.Role,
            invitation.Title,
            invitation.Status,
            invitation.InvitedByUser?.DisplayName ?? string.Empty,
            invitation.ExpiresAt,
            invitation.CreatedAt,
            invitation.RespondedAt);

    static AiMemberTemplateResponse ToAiMemberTemplateResponse(AiMemberTemplate template) =>
        new(
            template.Id,
            template.Key,
            template.Label,
            template.DisplayName,
            template.JobTitle,
            template.ResponsibilitySummary,
            template.Title,
            template.PermissionBoundary,
            template.SystemPrompt,
            template.AllowedTools,
            template.ExecutableActions,
            template.KnowledgeScope,
            template.IsAutonomous,
            template.TeamId,
            template.IsBuiltIn,
            template.IsEnabled,
            template.SortOrder);

    static MemberResponse ToMemberResponse(Member member) =>
        new(
            member.Id,
            member.TeamId,
            member.MemberType,
            member.Role,
            member.Status,
            member.DisplayName,
            member.Title,
            member.AiProfile is null
                ? null
                : new AiMemberProfileResponse(
                    member.AiProfile.Id,
                    member.AiProfile.TemplateKey,
                    member.AiProfile.JobTitle,
                    member.AiProfile.ResponsibilitySummary,
                    member.AiProfile.PermissionBoundary,
                    member.AiProfile.SystemPrompt,
                    member.AiProfile.AllowedTools,
                    member.AiProfile.ExecutableActions,
                    member.AiProfile.KnowledgeScope,
                    member.AiProfile.IsAutonomous));

    static ProjectResponse ToProjectResponse(
        Project project,
        int ticketCount = 0,
        int customerCount = 0,
        IReadOnlyCollection<Guid>? participantMemberIds = null) =>
        new(
            project.Id,
            project.TeamId,
            project.Name,
            project.Description,
            project.StageLabel,
            project.Summary,
            project.RiskSummary,
            project.NextSteps,
            project.Status,
            project.LeadMemberId,
            participantMemberIds?.ToList() ?? [],
            participantMemberIds?.Count ?? 0,
            ticketCount,
            customerCount,
            project.SourceType,
            project.ExternalSystemType,
            project.ExternalId);

    static ConciergeAppResponse ToConciergeAppResponse(ConciergeApp conciergeApp) =>
        new(
            conciergeApp.Id,
            conciergeApp.TeamId,
            conciergeApp.ProjectId,
            conciergeApp.Name,
            conciergeApp.Description,
            conciergeApp.ServiceScope,
            conciergeApp.WelcomeMessage,
            conciergeApp.FaqScope,
            conciergeApp.BusinessHours,
            conciergeApp.ChannelLabel,
            conciergeApp.IntakeGuidance,
            conciergeApp.SuggestedPrompts,
            conciergeApp.RequireEmail,
            conciergeApp.RequirePhoneNumber,
            conciergeApp.Status,
            conciergeApp.PrimaryAiMemberId,
            conciergeApp.TicketCreationPolicy,
            conciergeApp.HumanHandoffPolicy);

    static CustomerResponse ToCustomerResponse(Customer customer) =>
        new(
            customer.Id,
            customer.TeamId,
            customer.DisplayName,
            customer.Email,
            customer.PhoneNumber,
            customer.CompanyName,
            customer.SourceLabel,
            customer.Tags,
            customer.FollowUpStatus,
            customer.LastContactedAt,
            customer.ProjectId,
            customer.Notes,
            customer.Status,
            customer.SourceType,
            customer.ExternalSystemType,
            customer.ExternalId);

    static TicketActivityResponse ToTicketActivityResponse(TicketActivity activity) =>
        new(
            activity.Id,
            activity.ActivityType,
            activity.Summary,
            activity.Detail,
            activity.ActorMemberId,
            activity.ActorMember?.DisplayName,
            activity.ActorUserId,
            activity.ActorUser?.DisplayName,
            activity.CreatedAt);

    static TicketDetailResponse ToTicketDetailResponse(Ticket ticket) =>
        new(
            ticket.Id,
            ticket.TeamId,
            ticket.ProjectId,
            ticket.ConciergeAppId,
            ticket.CustomerId,
            ticket.Customer?.DisplayName,
            ticket.ConversationId,
            ticket.Title,
            ticket.Summary,
            ticket.Category,
            ticket.Status,
            ticket.Priority,
            ticket.DueAt,
            ticket.ResolutionSummary,
            ticket.ResolvedAt,
            ticket.LastActivityAt,
            ticket.AssignedMemberId,
            ticket.AssignedMember?.DisplayName,
            ticket.Activities
                .OrderByDescending(x => x.CreatedAt)
                .Select(ToTicketActivityResponse)
                .ToList());

    static TicketResponse ToTicketResponse(Ticket ticket) =>
        new(
            ticket.Id,
            ticket.TeamId,
            ticket.ProjectId,
            ticket.ConciergeAppId,
            ticket.CustomerId,
            ticket.Customer?.DisplayName,
            ticket.ConversationId,
            ticket.Title,
            ticket.Summary,
            ticket.Category,
            ticket.Status,
            ticket.Priority,
            ticket.DueAt,
            ticket.ResolutionSummary,
            ticket.ResolvedAt,
            ticket.LastActivityAt,
            ticket.AssignedMemberId,
            ticket.AssignedMember?.DisplayName,
            ticket.SourceType,
            ticket.ExternalSystemType,
            ticket.ExternalId,
            ticket.CreatedAt);

    static TicketActivity CreateTicketActivity(
        Ticket ticket,
        UserSession? session,
        TicketActivityType activityType,
        string summary,
        string? detail) =>
        new()
        {
            TeamId = ticket.TeamId,
            Ticket = ticket,
            ActorUserId = session?.UserId,
            ActivityType = activityType,
            Summary = summary,
            Detail = string.IsNullOrWhiteSpace(detail) ? null : detail.Trim(),
        };

    static AgentWorkflowResponse ToAgentWorkflowResponse(AgentWorkflowRun workflow) =>
        new(
            workflow.Id,
            workflow.TeamId,
            workflow.ProjectId,
            workflow.ConversationId,
            workflow.TicketId,
            workflow.WorkflowType,
            workflow.TriggerMode,
            workflow.Goal,
            workflow.Summary,
            workflow.SummarySchemaVersion,
            workflow.SummaryRawResponse,
            ParseAiResponseAttempts(workflow.SummaryAttemptTrace),
            workflow.Status,
            workflow.RequestedByUserId,
            workflow.StartedByMemberId,
            workflow.StartedByMember?.DisplayName,
            workflow.CreatedAt,
            workflow.CompletedAt,
            workflow.Steps
                .OrderBy(step => step.Sequence)
                .Select(step => new AgentWorkflowStepResponse(
                    step.Id,
                    step.Sequence,
                    step.MemberId,
                    step.Member?.DisplayName,
                    step.Member?.AiProfile?.JobTitle ?? step.Member?.Title,
                    step.HandoffToMemberId,
                    step.HandoffToMember?.DisplayName,
                    step.ActionType,
                    step.InputSummary,
                    step.OutputSummary,
                    step.OutputSchemaVersion,
                    step.OutputRawResponse,
                    ParseAiResponseAttempts(step.OutputAttemptTrace),
                    step.HandoffSummary,
                    step.Status,
                    step.ExecutedAt,
                    step.ExecutionLogs
                        .OrderBy(log => log.CreatedAt)
                        .Select(log => new AgentExecutionLogResponse(
                            log.Id,
                            log.MemberId,
                            log.Member?.DisplayName,
                            log.ToolName,
                            log.ToolCategory,
                            log.BoundarySummary,
                            log.InputSummary,
                            log.OutputSummary,
                            log.Status,
                            log.WasAllowed,
                            log.ExecutedAt))
                        .ToList()))
                .ToList());

    static IReadOnlyList<AiResponseAttemptResponse> ParseAiResponseAttempts(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            var attempts = JsonSerializer.Deserialize<List<AiResponseAttemptResponse>>(json);
            return attempts ?? [];
        }
        catch
        {
            return [];
        }
    }

    static IntegrationConnectionResponse ToIntegrationConnectionResponse(IntegrationConnection connection) =>
        new(
            connection.Id,
            connection.TeamId,
            connection.ExternalSystemType,
            connection.Name,
            connection.BaseUrl,
            connection.IsEnabled,
            !string.IsNullOrWhiteSpace(connection.AuthConfig),
            connection.CreatedAt);

    static IntegrationPreviewItemResponse ToIntegrationPreviewItemResponse(IntegrationRecordRef record) =>
        new(
            record.Id,
            record.DisplayName,
            record.Summary);
}
