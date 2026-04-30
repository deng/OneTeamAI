namespace Chatbot.Api.Models;

public sealed record ImportIntegrationCustomerRequest(
    string ExternalRecordId,
    Guid? ProjectId = null,
    string? Tags = null,
    bool ForceUpdate = false);
