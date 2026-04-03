using Chatbot.Api.Domain.Enums;

namespace Chatbot.Api.Integrations.Models;

public sealed record IntegrationConnectionDescriptor(
    Guid ConnectionId,
    Guid TeamId,
    ExternalSystemType ExternalSystemType,
    string Name,
    string BaseUrl,
    string? AuthConfig,
    bool IsEnabled);
