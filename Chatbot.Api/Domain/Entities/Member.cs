using System.ComponentModel.DataAnnotations;
using Chatbot.Api.Domain.Common;
using Chatbot.Api.Domain.Enums;

namespace Chatbot.Api.Domain.Entities;

public class Member : EntityBase
{
    public Guid TeamId { get; set; }

    public Team? Team { get; set; }

    public MemberType MemberType { get; set; } = MemberType.Human;

    public MemberRole Role { get; set; } = MemberRole.Operator;

    public MemberStatus Status { get; set; } = MemberStatus.Active;

    public Guid? UserId { get; set; }

    public User? User { get; set; }

    [MaxLength(128)]
    public string DisplayName { get; set; } = string.Empty;

    [MaxLength(128)]
    public string? Title { get; set; }

    public AIMemberProfile? AiProfile { get; set; }

    public ICollection<Project> LeadProjects { get; set; } = [];

    public ICollection<ProjectMember> ProjectMemberships { get; set; } = [];

    public ICollection<Ticket> AssignedTickets { get; set; } = [];
}
