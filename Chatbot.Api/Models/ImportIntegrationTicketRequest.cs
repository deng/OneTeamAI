namespace Chatbot.Api.Models;

public sealed record ImportIntegrationTicketRequest(
    string ExternalRecordId,
    Guid ProjectId,
    Guid? CustomerId = null,
    bool ForceUpdate = false);
