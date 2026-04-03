using Chatbot.Api.Domain.Enums;

namespace Chatbot.Api.Models;

public sealed record TicketDetailResponse(
    Guid Id,
    Guid TeamId,
    Guid ProjectId,
    Guid? ConciergeAppId,
    Guid? CustomerId,
    string? CustomerName,
    Guid? ConversationId,
    string Title,
    string Summary,
    string? Category,
    TicketStatus Status,
    TicketPriority Priority,
    DateTimeOffset? DueAt,
    DateTimeOffset? LastActivityAt,
    Guid? AssignedMemberId,
    string? AssignedMemberName,
    IReadOnlyList<TicketActivityResponse> Activities);
