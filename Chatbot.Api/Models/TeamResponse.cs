namespace Chatbot.Api.Models;

public sealed record TeamResponse(
    Guid Id,
    string Name,
    string? Description,
    string? BrandName,
    Guid OwnerUserId,
    Guid OwnerMemberId);
