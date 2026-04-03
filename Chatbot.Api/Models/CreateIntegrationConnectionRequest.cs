using Chatbot.Api.Domain.Enums;

namespace Chatbot.Api.Models;

public sealed record CreateIntegrationConnectionRequest(
    ExternalSystemType ExternalSystemType,
    string Name,
    string BaseUrl,
    string? AuthConfig = null,
    bool IsEnabled = true);
