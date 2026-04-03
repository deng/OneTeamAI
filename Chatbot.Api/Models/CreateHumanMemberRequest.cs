using Chatbot.Api.Domain.Enums;

namespace Chatbot.Api.Models;

public sealed record CreateHumanMemberRequest(
    string Email,
    MemberRole Role = MemberRole.Operator,
    string? Title = null);
