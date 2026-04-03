using System.ComponentModel.DataAnnotations;
using Chatbot.Api.Domain.Common;
using Chatbot.Api.Domain.Enums;

namespace Chatbot.Api.Domain.Entities;

public class AgentWorkflowStep : EntityBase
{
    public Guid WorkflowRunId { get; set; }

    public AgentWorkflowRun? WorkflowRun { get; set; }

    public Guid? MemberId { get; set; }

    public Member? Member { get; set; }

    public Guid? HandoffToMemberId { get; set; }

    public Member? HandoffToMember { get; set; }

    public int Sequence { get; set; }

    [MaxLength(128)]
    public string ActionType { get; set; } = string.Empty;

    [MaxLength(1024)]
    public string InputSummary { get; set; } = string.Empty;

    [MaxLength(2048)]
    public string OutputSummary { get; set; } = string.Empty;

    [MaxLength(1024)]
    public string HandoffSummary { get; set; } = string.Empty;

    public AgentWorkflowStepStatus Status { get; set; } = AgentWorkflowStepStatus.Pending;

    public DateTimeOffset? ExecutedAt { get; set; }

    public ICollection<AgentExecutionLog> ExecutionLogs { get; set; } = [];
}
