namespace Chatbot.Api.Models;

public sealed record ImportIntegrationProjectRequest(
    string ExternalRecordId,
    Guid? LeadMemberId = null,
    bool ForceUpdate = false);
