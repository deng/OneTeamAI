namespace Chatbot.Api.Models;

public sealed record CreateProjectRequest(
    string Name,
    string? Description = null,
    string? StageLabel = null,
    string? Summary = null,
    string? RiskSummary = null,
    string? NextSteps = null,
    Guid? LeadMemberId = null);
