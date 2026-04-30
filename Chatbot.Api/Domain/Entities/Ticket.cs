using System.ComponentModel.DataAnnotations;
using Chatbot.Api.Domain.Common;
using Chatbot.Api.Domain.Enums;

namespace Chatbot.Api.Domain.Entities;

public class Ticket : ExternalMappedEntityBase
{
    public Guid TeamId { get; set; }

    public Team? Team { get; set; }

    public Guid ProjectId { get; set; }

    public Project? Project { get; set; }

    public Guid? ConciergeAppId { get; set; }

    public ConciergeApp? ConciergeApp { get; set; }

    public Guid? CustomerId { get; set; }

    public Customer? Customer { get; set; }

    public Guid? ConversationId { get; set; }

    public Conversation? Conversation { get; set; }

    [MaxLength(256)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(4096)]
    public string Summary { get; set; } = string.Empty;

    [MaxLength(64)]
    public string? Category { get; set; }

    public TicketStatus Status { get; set; } = TicketStatus.Pending;

    public TicketPriority Priority { get; set; } = TicketPriority.Medium;

    public DateTimeOffset? DueAt { get; set; }

    [MaxLength(4096)]
    public string? ResolutionSummary { get; set; }

    public DateTimeOffset? ResolvedAt { get; set; }

    public DateTimeOffset? LastActivityAt { get; set; }

    public Guid? AssignedMemberId { get; set; }

    public Member? AssignedMember { get; set; }

    public ICollection<TicketActivity> Activities { get; set; } = [];
}
