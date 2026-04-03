using Chatbot.Api.Domain.Enums;

namespace Chatbot.Api.Models;

public sealed record CreateConversationRequest(
    Guid? CustomerId,
    string? CustomerDisplayName,
    string? CustomerEmail,
    string InitialMessage,
    bool AutoCreateTicket = false,
    TicketPriority AutoTicketPriority = TicketPriority.Medium);
