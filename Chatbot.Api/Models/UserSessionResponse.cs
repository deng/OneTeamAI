namespace Chatbot.Api.Models;

public sealed record UserSessionResponse(
    Guid Id,
    DateTimeOffset CreatedAt,
    DateTimeOffset LastSeenAt,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? RevokedAt,
    string? RevokedReason,
    string? UserAgent,
    string? IpAddress,
    bool IsCurrent);
