using Chatbot.Api.Domain.Enums;

namespace Chatbot.Api.Models;

public sealed record IntegrationConnectionResponse(
    Guid Id,
    Guid TeamId,
    ExternalSystemType ExternalSystemType,
    string Name,
    string BaseUrl,
    bool IsEnabled,
    bool HasAuthConfig,
    DateTimeOffset CreatedAt);

public sealed record IntegrationPreviewItemResponse(
    string Id,
    string DisplayName,
    string? Summary = null);

public sealed record FileKnowledgeItemResponse(
    string Id,
    string Name,
    string Path,
    DateTimeOffset? UpdatedAt,
    string? MimeType,
    long? Size);
