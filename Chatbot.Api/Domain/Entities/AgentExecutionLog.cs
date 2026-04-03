using System.ComponentModel.DataAnnotations;
using Chatbot.Api.Domain.Common;
using Chatbot.Api.Domain.Enums;

namespace Chatbot.Api.Domain.Entities;

public class AgentExecutionLog : EntityBase
{
    public Guid WorkflowStepId { get; set; }

    public AgentWorkflowStep? WorkflowStep { get; set; }

    public Guid? MemberId { get; set; }

    public Member? Member { get; set; }

    [MaxLength(128)]
    public string ToolName { get; set; } = string.Empty;

    [MaxLength(128)]
    public string ToolCategory { get; set; } = string.Empty;

    [MaxLength(1024)]
    public string BoundarySummary { get; set; } = string.Empty;

    [MaxLength(1024)]
    public string InputSummary { get; set; } = string.Empty;

    [MaxLength(2048)]
    public string OutputSummary { get; set; } = string.Empty;

    public AgentExecutionLogStatus Status { get; set; } = AgentExecutionLogStatus.Planned;

    public bool WasAllowed { get; set; }

    public DateTimeOffset? ExecutedAt { get; set; }
}
