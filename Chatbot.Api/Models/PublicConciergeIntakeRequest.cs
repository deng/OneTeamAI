using Chatbot.Api.Domain.Enums;

namespace Chatbot.Api.Models;

public sealed record PublicConciergeIntakeRequest(
    string DisplayName,
    string? Email,
    string? PhoneNumber,
    string? CompanyName,
    string Message,
    bool AutoCreateTicket = true,
    TicketPriority AutoTicketPriority = TicketPriority.Medium);

public sealed record PublicConciergeIntakeResponse(
    Guid CustomerId,
    Guid ConversationId,
    Guid? TicketId,
    string Message);
