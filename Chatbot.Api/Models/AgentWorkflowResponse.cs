using Chatbot.Api.Domain.Enums;

namespace Chatbot.Api.Models;

public sealed record AgentExecutionLogResponse(
    Guid Id,
    Guid? MemberId,
    string? MemberName,
    string ToolName,
    string ToolCategory,
    string BoundarySummary,
    string InputSummary,
    string OutputSummary,
    AgentExecutionLogStatus Status,
    bool WasAllowed,
    DateTimeOffset? ExecutedAt);

public sealed record AgentWorkflowStepResponse(
    Guid Id,
    int Sequence,
    Guid? MemberId,
    string? MemberName,
    string? MemberTitle,
    Guid? HandoffToMemberId,
    string? HandoffToMemberName,
    string ActionType,
    string InputSummary,
    string OutputSummary,
    string HandoffSummary,
    AgentWorkflowStepStatus Status,
    DateTimeOffset? ExecutedAt,
    IReadOnlyList<AgentExecutionLogResponse> ExecutionLogs);

public sealed record AgentWorkflowResponse(
    Guid Id,
    Guid TeamId,
    Guid? ProjectId,
    Guid? ConversationId,
    Guid? TicketId,
    string WorkflowType,
    string Goal,
    string Summary,
    AgentWorkflowStatus Status,
    Guid? RequestedByUserId,
    Guid? StartedByMemberId,
    string? StartedByMemberName,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt,
    IReadOnlyList<AgentWorkflowStepResponse> Steps);
