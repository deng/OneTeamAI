using Chatbot.Api.Domain.Enums;

namespace Chatbot.Api.Models;

public sealed record RunAgentWorkflowRequest(
    string? Goal = null,
    Guid? StartedByMemberId = null,
    AgentWorkflowTriggerMode TriggerMode = AgentWorkflowTriggerMode.Manual);
