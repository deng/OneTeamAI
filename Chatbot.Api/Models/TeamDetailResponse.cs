namespace Chatbot.Api.Models;

public sealed record TeamDetailResponse(
    Guid Id,
    string Name,
    string? Description,
    string? BrandName,
    TeamOwnerResponse? Owner,
    int MemberCount,
    int ProjectCount);

public sealed record TeamOwnerResponse(
    Guid Id,
    string DisplayName,
    string Email);
