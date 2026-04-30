using Chatbot.Api.Domain.Enums;

namespace Chatbot.Api.Models;

public sealed record RunTicketWorkflowRequest(
    string? Goal = null,
    Guid? StartedByMemberId = null,
    AgentWorkflowTriggerMode TriggerMode = AgentWorkflowTriggerMode.Manual);
