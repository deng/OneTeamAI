using Chatbot.Api.Domain.Entities;
using Chatbot.Api.Domain.Enums;
using Chatbot.Api.Infrastructure.Persistence;

namespace Chatbot.Api.Services;

public sealed class AgentWorkflowWritebackService(AppDbContext dbContext)
{
    public Task ApplyAsync(
        AgentWorkflowRun workflow,
        Ticket ticket,
        UserSession? session,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        ticket.LastActivityAt = now;
        ticket.UpdatedAt = now;
        dbContext.TicketActivities.Add(new TicketActivity
        {
            TeamId = ticket.TeamId,
            Ticket = ticket,
            ActorUserId = session?.UserId,
            ActivityType = TicketActivityType.Note,
            Summary = "AI 协作结果已回写",
            Detail = TrimToLimit(workflow.Summary, 2048),
        });

        if (ticket.Customer is not null)
        {
            ticket.Customer.LastContactedAt = now;
            ticket.Customer.UpdatedAt = now;
            if (ticket.Customer.FollowUpStatus == CustomerFollowUpStatus.New)
            {
                ticket.Customer.FollowUpStatus = CustomerFollowUpStatus.Contacting;
            }

            ticket.Customer.Notes = MergeText(
                ticket.Customer.Notes,
                $"[AI 协作 {now:yyyy-MM-dd HH:mm}] {workflow.Summary}",
                2048);
        }

        return Task.CompletedTask;
    }

    public Task ApplyAsync(
        AgentWorkflowRun workflow,
        Conversation conversation,
        UserSession? session,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        conversation.UpdatedAt = now;
        dbContext.ConversationMessages.Add(new ConversationMessage
        {
            ConversationId = conversation.Id,
            ParticipantType = ConversationParticipantType.AiMember,
            MemberId = workflow.StartedByMemberId ?? workflow.Steps.OrderBy(x => x.Sequence).FirstOrDefault()?.MemberId,
            SenderName = "AI 协作摘要",
            Content = TrimToLimit(workflow.Summary, 4000) ?? "已完成协作摘要。",
        });

        if (conversation.Customer is not null)
        {
            conversation.Customer.LastContactedAt = now;
            conversation.Customer.UpdatedAt = now;
            if (conversation.Customer.FollowUpStatus == CustomerFollowUpStatus.New)
            {
                conversation.Customer.FollowUpStatus = CustomerFollowUpStatus.Contacting;
            }

            if (!conversation.Customer.ProjectId.HasValue && conversation.ConciergeApp?.ProjectId is not null)
            {
                conversation.Customer.ProjectId = conversation.ConciergeApp.ProjectId;
            }

            conversation.Customer.Notes = MergeText(
                conversation.Customer.Notes,
                $"[会话协作 {now:yyyy-MM-dd HH:mm}] {workflow.Summary}",
                2048);
        }

        return Task.CompletedTask;
    }

    public Task ApplyAsync(
        AgentWorkflowRun workflow,
        Project project,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        project.UpdatedAt = now;
        project.Summary = MergeText(project.Summary, $"[AI 协作 {now:yyyy-MM-dd HH:mm}] {workflow.Summary}", 4096);

        var riskHint = workflow.Steps
            .OrderByDescending(step => step.Sequence)
            .Select(step => step.OutputSummary)
            .FirstOrDefault(text => !string.IsNullOrWhiteSpace(text));
        project.RiskSummary = MergeText(project.RiskSummary, riskHint, 4096);

        var nextStepHint = workflow.Steps
            .OrderBy(step => step.Sequence)
            .Select(step => step.HandoffSummary)
            .FirstOrDefault(text => !string.IsNullOrWhiteSpace(text));
        project.NextSteps = MergeText(project.NextSteps, nextStepHint, 4096);

        return Task.CompletedTask;
    }

    private static string? MergeText(string? existing, string? incoming, int maxLength)
    {
        var incomingText = TrimToLimit(incoming, maxLength);
        if (string.IsNullOrWhiteSpace(incomingText))
        {
            return TrimToLimit(existing, maxLength);
        }

        var baseText = string.IsNullOrWhiteSpace(existing) ? string.Empty : existing.Trim();
        if (string.IsNullOrWhiteSpace(baseText))
        {
            return incomingText;
        }

        var merged = $"{baseText}\n\n{incomingText}";
        return TrimToLimit(merged, maxLength);
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
}
