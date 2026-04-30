namespace Chatbot.Api.Models;

public sealed record CreateAiMemberTemplateRequest(
    string Label,
    string DisplayName,
    string JobTitle,
    string ResponsibilitySummary,
    string? Key = null,
    string? Title = null,
    string? PermissionBoundary = null,
    string? SystemPrompt = null,
    string? AllowedTools = null,
    string? ExecutableActions = null,
    string? KnowledgeScope = null,
    bool IsAutonomous = false,
    int? SortOrder = null);
