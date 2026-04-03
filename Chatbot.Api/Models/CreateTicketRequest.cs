using Chatbot.Api.Domain.Enums;

namespace Chatbot.Api.Models;

public sealed record CreateTicketRequest(
    string Title,
    string Summary,
    TicketPriority Priority = TicketPriority.Medium,
    Guid? AssignedMemberId = null);
