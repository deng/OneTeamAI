namespace Chatbot.Api.Integrations.Models;

public sealed record FileKnowledgeRecord(
    string Id,
    string Name,
    string Path,
    DateTimeOffset? UpdatedAt = null,
    string? MimeType = null,
    long? Size = null);
