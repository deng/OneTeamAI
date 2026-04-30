using Chatbot.Api.Domain.Enums;

namespace Chatbot.Api.Models;

public sealed record UpdateTicketRequest(
    TicketStatus Status,
    TicketPriority Priority,
    Guid? AssignedMemberId = null,
    string? Category = null,
    DateTimeOffset? DueAt = null,
    string? ResolutionSummary = null,
    string? ActivityNote = null);
