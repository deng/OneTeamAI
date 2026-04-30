namespace Chatbot.Api.Models;

public sealed record AiMemberTemplateResponse(
    Guid Id,
    string Key,
    string Label,
    string DisplayName,
    string JobTitle,
    string ResponsibilitySummary,
    string? Title,
    string? PermissionBoundary,
    string? SystemPrompt,
    string? AllowedTools,
    string? ExecutableActions,
    string? KnowledgeScope,
    bool IsAutonomous,
    Guid? TeamId,
    bool IsBuiltIn,
    bool IsEnabled,
    int SortOrder);
