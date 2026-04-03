using System.ComponentModel.DataAnnotations;
using Chatbot.Api.Domain.Common;

namespace Chatbot.Api.Domain.Entities;

public class ProjectMember : EntityBase
{
    public Guid ProjectId { get; set; }

    public Project? Project { get; set; }

    public Guid MemberId { get; set; }

    public Member? Member { get; set; }

    [MaxLength(128)]
    public string? RoleLabel { get; set; }
}
