namespace Chatbot.Api.Models;

public sealed record AuditLogResponse(
    Guid Id,
    Guid? TeamId,
    Guid? UserId,
    string? UserDisplayName,
    string ActionType,
    string EntityType,
    Guid? EntityId,
    string Summary,
    string Result,
    string? IpAddress,
    DateTimeOffset CreatedAt);
