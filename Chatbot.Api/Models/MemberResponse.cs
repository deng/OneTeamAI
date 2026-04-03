using Chatbot.Api.Domain.Enums;

namespace Chatbot.Api.Models;

public sealed record MemberResponse(
    Guid Id,
    Guid TeamId,
    MemberType MemberType,
    MemberRole Role,
    MemberStatus Status,
    string DisplayName,
    string? Title,
    AiMemberProfileResponse? AiProfile);

public sealed record AiMemberProfileResponse(
    Guid? Id,
    string? TemplateKey,
    string JobTitle,
    string ResponsibilitySummary,
    string? PermissionBoundary,
    string? SystemPrompt,
    string? AllowedTools,
    string? ExecutableActions,
    string? KnowledgeScope,
    bool IsAutonomous);
