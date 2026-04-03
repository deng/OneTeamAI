using Chatbot.Api.Domain.Enums;

namespace Chatbot.Api.Models;

public sealed record ConversationResponse(
    Guid Id,
    Guid TeamId,
    Guid ConciergeAppId,
    Guid? CustomerId,
    ConversationStatus Status,
    string FirstMessage,
    Guid? AutoCreatedTicketId);

public sealed record ConversationSummaryResponse(
    Guid Id,
    Guid TeamId,
    Guid ConciergeAppId,
    Guid? CustomerId,
    string? CustomerName,
    ConversationStatus Status,
    int MessageCount,
    string? LatestMessage,
    DateTimeOffset CreatedAt);

public sealed record ConversationDetailResponse(
    Guid Id,
    Guid TeamId,
    Guid ConciergeAppId,
    Guid? CustomerId,
    ConversationCustomerResponse? Customer,
    ConversationStatus Status,
    IReadOnlyList<ConversationMessageResponse> Messages,
    IReadOnlyList<Guid> TicketIds);

public sealed record ConversationCustomerResponse(
    string DisplayName,
    string? Email);

public sealed record ConversationMessageResponse(
    Guid Id,
    ConversationParticipantType ParticipantType,
    Guid? MemberId,
    string? SenderName,
    string Content,
    DateTimeOffset CreatedAt);
