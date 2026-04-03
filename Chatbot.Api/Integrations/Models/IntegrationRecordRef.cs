using Chatbot.Api.Domain.Enums;

namespace Chatbot.Api.Integrations.Models;

public sealed record IntegrationRecordRef(
    string Id,
    string DisplayName,
    ExternalSystemType ExternalSystemType,
    string? Summary = null);
