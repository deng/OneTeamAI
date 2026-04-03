namespace Chatbot.Api.Models;

public sealed record RunTicketWorkflowRequest(
    string? Goal = null,
    Guid? StartedByMemberId = null);
