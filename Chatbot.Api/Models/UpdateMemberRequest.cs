using Chatbot.Api.Domain.Enums;

namespace Chatbot.Api.Models;

public sealed record UpdateMemberRequest(
    MemberRole Role,
    string? Title = null);
