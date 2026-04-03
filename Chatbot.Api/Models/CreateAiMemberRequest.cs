namespace Chatbot.Api.Models;

public sealed record CreateAiMemberRequest(
    string DisplayName,
    string JobTitle,
    string ResponsibilitySummary,
    string? TemplateKey = null,
    string? PermissionBoundary = null,
    string? Title = null,
    string? SystemPrompt = null,
    string? AllowedTools = null,
    string? ExecutableActions = null,
    string? KnowledgeScope = null,
    bool IsAutonomous = false);
