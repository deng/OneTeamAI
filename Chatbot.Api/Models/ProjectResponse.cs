using Chatbot.Api.Domain.Enums;

namespace Chatbot.Api.Models;

public sealed record ProjectResponse(
    Guid Id,
    Guid TeamId,
    string Name,
    string? Description,
    string? StageLabel,
    string? Summary,
    string? RiskSummary,
    string? NextSteps,
    ProjectStatus Status,
    Guid? LeadMemberId,
    List<Guid> ParticipantMemberIds,
    int ParticipantCount,
    int TicketCount,
    int CustomerCount,
    RecordSourceType SourceType,
    ExternalSystemType? ExternalSystemType,
    string? ExternalId);
