using Chatbot.Api.Domain.Enums;

namespace Chatbot.Api.Models;

public sealed record CreateInvitationRequest(
    string Email,
    MemberRole Role = MemberRole.Operator,
    string? Title = null,
    int ExpiresInDays = 7);
