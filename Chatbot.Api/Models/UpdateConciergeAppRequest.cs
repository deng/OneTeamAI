using Chatbot.Api.Domain.Enums;

namespace Chatbot.Api.Models;

public sealed record UpdateConciergeAppRequest(
    string Name,
    string? Description = null,
    string? ServiceScope = null,
    string? WelcomeMessage = null,
    string? FaqScope = null,
    string? BusinessHours = null,
    string? ChannelLabel = null,
    string? IntakeGuidance = null,
    string? SuggestedPrompts = null,
    bool RequireEmail = false,
    bool RequirePhoneNumber = false,
    ConciergeAppStatus Status = ConciergeAppStatus.Draft,
    Guid? PrimaryAiMemberId = null,
    string? TicketCreationPolicy = null,
    string? HumanHandoffPolicy = null);
