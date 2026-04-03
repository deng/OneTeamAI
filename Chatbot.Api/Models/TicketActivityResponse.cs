using Chatbot.Api.Domain.Enums;

namespace Chatbot.Api.Models;

public sealed record TicketActivityResponse(
    Guid Id,
    TicketActivityType ActivityType,
    string Summary,
    string? Detail,
    Guid? ActorMemberId,
    string? ActorMemberName,
    Guid? ActorUserId,
    string? ActorUserName,
    DateTimeOffset CreatedAt);
