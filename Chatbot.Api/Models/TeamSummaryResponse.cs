using Chatbot.Api.Domain.Enums;

namespace Chatbot.Api.Models;

public sealed record TeamSummaryResponse(
    Guid Id,
    string Name,
    string? Description,
    string? BrandName,
    Guid OwnerUserId,
    Guid? CurrentMemberId,
    MemberRole? CurrentMemberRole);
