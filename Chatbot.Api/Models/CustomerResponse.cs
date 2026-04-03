using Chatbot.Api.Domain.Enums;

namespace Chatbot.Api.Models;

public sealed record CustomerResponse(
    Guid Id,
    Guid TeamId,
    string DisplayName,
    string? Email,
    string? PhoneNumber,
    string? CompanyName,
    string? SourceLabel,
    string? Tags,
    CustomerFollowUpStatus FollowUpStatus,
    DateTimeOffset? LastContactedAt,
    Guid? ProjectId,
    string? Notes,
    CustomerStatus Status,
    RecordSourceType SourceType,
    ExternalSystemType? ExternalSystemType,
    string? ExternalId);
