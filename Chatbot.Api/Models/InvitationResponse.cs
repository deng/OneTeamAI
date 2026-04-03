using Chatbot.Api.Domain.Enums;

namespace Chatbot.Api.Models;

public sealed record InvitationResponse(
    Guid Id,
    Guid TeamId,
    string TeamName,
    string Email,
    MemberRole Role,
    string? Title,
    InvitationStatus Status,
    string InvitedByDisplayName,
    DateTimeOffset ExpiresAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset? RespondedAt);
