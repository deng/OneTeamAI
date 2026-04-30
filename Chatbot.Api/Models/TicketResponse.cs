using Chatbot.Api.Domain.Enums;

namespace Chatbot.Api.Models;

public sealed record TicketResponse(
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
    string? ResolutionSummary,
    DateTimeOffset? ResolvedAt,
    DateTimeOffset? LastActivityAt,
    Guid? AssignedMemberId,
    string? AssignedMemberName,
    RecordSourceType SourceType,
    ExternalSystemType? ExternalSystemType,
    string? ExternalId,
    DateTimeOffset? CreatedAt);
