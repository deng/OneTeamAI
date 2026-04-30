using Chatbot.Api.Domain.Enums;

namespace Chatbot.Api.Models;

public sealed record PublicConciergeAppResponse(
    Guid Id,
    string Name,
    string? Description,
    string? ServiceScope,
    string? WelcomeMessage,
    string? FaqScope,
    string? BusinessHours,
    string? ChannelLabel,
    string? IntakeGuidance,
    string? SuggestedPrompts,
    bool RequireEmail,
    bool RequirePhoneNumber,
    ConciergeAppStatus Status,
    string? TeamBrandName,
    string? ProjectName);
