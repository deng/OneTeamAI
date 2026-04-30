using System.ComponentModel.DataAnnotations;
using Chatbot.Api.Domain.Common;

namespace Chatbot.Api.Domain.Entities;

public class Team : EntityBase
{
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1024)]
    public string? Description { get; set; }

    [MaxLength(128)]
    public string? BrandName { get; set; }

    public Guid OwnerUserId { get; set; }

    public User? OwnerUser { get; set; }

    public ICollection<Member> Members { get; set; } = [];

    public ICollection<AiMemberTemplate> AiMemberTemplates { get; set; } = [];

    public ICollection<Project> Projects { get; set; } = [];

    public ICollection<ConciergeApp> ConciergeApps { get; set; } = [];

    public ICollection<IntegrationConnection> IntegrationConnections { get; set; } = [];

    public ICollection<TeamInvitation> Invitations { get; set; } = [];
}
