namespace Chatbot.Api.Models;

public sealed record UpdateAiMemberTemplateRequest(
    string Label,
    string DisplayName,
    string JobTitle,
    string ResponsibilitySummary,
    string? Title = null,
    string? PermissionBoundary = null,
    string? SystemPrompt = null,
    string? AllowedTools = null,
    string? ExecutableActions = null,
    string? KnowledgeScope = null,
    bool IsAutonomous = false,
    bool IsEnabled = true,
    int? SortOrder = null);
