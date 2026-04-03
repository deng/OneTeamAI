namespace Chatbot.Api.Models;

public sealed record UpdateProjectRequest(
    string Name,
    string? Description = null,
    string? StageLabel = null,
    string? Summary = null,
    string? RiskSummary = null,
    string? NextSteps = null,
    Guid? LeadMemberId = null,
    List<Guid>? ParticipantMemberIds = null);
