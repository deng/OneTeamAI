using Chatbot.Api.Domain.Enums;

namespace Chatbot.Api.Models;

public sealed record ConciergeAppResponse(
    Guid Id,
    Guid TeamId,
    Guid ProjectId,
    string Name,
    string? Description,
    string? ServiceScope,
    string? WelcomeMessage,
    string? FaqScope,
    string? BusinessHours,
    string? ChannelLabel,
    ConciergeAppStatus Status,
    Guid? PrimaryAiMemberId,
    string? TicketCreationPolicy,
    string? HumanHandoffPolicy);
