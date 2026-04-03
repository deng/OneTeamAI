namespace Chatbot.Api.Models;

public sealed record CreateConciergeAppRequest(
    Guid ProjectId,
    string Name,
    string? Description = null,
    string? ServiceScope = null,
    string? WelcomeMessage = null,
    string? FaqScope = null,
    string? BusinessHours = null,
    string? ChannelLabel = null,
    Guid? PrimaryAiMemberId = null,
    string? TicketCreationPolicy = null,
    string? HumanHandoffPolicy = null);
