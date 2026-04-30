using System.ComponentModel.DataAnnotations;
using Chatbot.Api.Domain.Common;

namespace Chatbot.Api.Domain.Entities;

public class AiMemberTemplate : EntityBase
{
    public Guid? TeamId { get; set; }

    public Team? Team { get; set; }

    [MaxLength(64)]
    public string Key { get; set; } = string.Empty;

    [MaxLength(128)]
    public string Label { get; set; } = string.Empty;

    [MaxLength(128)]
    public string DisplayName { get; set; } = string.Empty;

    [MaxLength(128)]
    public string JobTitle { get; set; } = string.Empty;

    [MaxLength(2048)]
    public string ResponsibilitySummary { get; set; } = string.Empty;

    [MaxLength(128)]
    public string? Title { get; set; }

    [MaxLength(1024)]
    public string? PermissionBoundary { get; set; }

    public string? SystemPrompt { get; set; }

    public string? AllowedTools { get; set; }

    public string? ExecutableActions { get; set; }

    public string? KnowledgeScope { get; set; }

    public bool IsAutonomous { get; set; }

    public bool IsBuiltIn { get; set; }

    public bool IsEnabled { get; set; } = true;

    public int SortOrder { get; set; }
}
