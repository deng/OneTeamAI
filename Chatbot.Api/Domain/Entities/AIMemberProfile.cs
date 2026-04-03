using System.ComponentModel.DataAnnotations;
using Chatbot.Api.Domain.Common;

namespace Chatbot.Api.Domain.Entities;

public class AIMemberProfile : EntityBase
{
    public Guid MemberId { get; set; }

    public Member? Member { get; set; }

    [MaxLength(128)]
    public string JobTitle { get; set; } = string.Empty;

    [MaxLength(2048)]
    public string ResponsibilitySummary { get; set; } = string.Empty;

    [MaxLength(64)]
    public string? TemplateKey { get; set; }

    [MaxLength(1024)]
    public string? PermissionBoundary { get; set; }

    public string? SystemPrompt { get; set; }

    public string? AllowedTools { get; set; }

    public string? ExecutableActions { get; set; }

    public string? KnowledgeScope { get; set; }

    public bool IsAutonomous { get; set; }
}
