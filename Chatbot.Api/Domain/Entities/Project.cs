using System.ComponentModel.DataAnnotations;
using Chatbot.Api.Domain.Common;
using Chatbot.Api.Domain.Enums;

namespace Chatbot.Api.Domain.Entities;

public class Project : ExternalMappedEntityBase
{
    public Guid TeamId { get; set; }

    public Team? Team { get; set; }

    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2048)]
    public string? Description { get; set; }

    [MaxLength(128)]
    public string? StageLabel { get; set; }

    [MaxLength(4096)]
    public string? Summary { get; set; }

    [MaxLength(4096)]
    public string? RiskSummary { get; set; }

    [MaxLength(4096)]
    public string? NextSteps { get; set; }

    public ProjectStatus Status { get; set; } = ProjectStatus.Draft;

    public Guid? LeadMemberId { get; set; }

    public Member? LeadMember { get; set; }

    public ICollection<ProjectMember> ProjectMembers { get; set; } = [];

    public ICollection<ConciergeApp> ConciergeApps { get; set; } = [];

    public ICollection<Ticket> Tickets { get; set; } = [];
}
