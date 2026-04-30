using System.ComponentModel.DataAnnotations;
using Chatbot.Api.Domain.Common;
using Chatbot.Api.Domain.Enums;

namespace Chatbot.Api.Domain.Entities;

public class AgentWorkflowRun : EntityBase
{
    public Guid TeamId { get; set; }

    public Team? Team { get; set; }

    public Guid? ProjectId { get; set; }

    public Project? Project { get; set; }

    public Guid? ConversationId { get; set; }

    public Conversation? Conversation { get; set; }

    public Guid? TicketId { get; set; }

    public Ticket? Ticket { get; set; }

    public Guid? RequestedByUserId { get; set; }

    public User? RequestedByUser { get; set; }

    public Guid? StartedByMemberId { get; set; }

    public Member? StartedByMember { get; set; }

    [MaxLength(128)]
    public string WorkflowType { get; set; } = string.Empty;

    [MaxLength(1024)]
    public string Goal { get; set; } = string.Empty;

    [MaxLength(2048)]
    public string Summary { get; set; } = string.Empty;

    [MaxLength(64)]
    public string? SummarySchemaVersion { get; set; }

    public string? SummaryRawResponse { get; set; }

    public string? SummaryAttemptTrace { get; set; }

    public AgentWorkflowTriggerMode TriggerMode { get; set; } = AgentWorkflowTriggerMode.Manual;

    public AgentWorkflowStatus Status { get; set; } = AgentWorkflowStatus.Planned;

    public DateTimeOffset? CompletedAt { get; set; }

    public ICollection<AgentWorkflowStep> Steps { get; set; } = [];
}
