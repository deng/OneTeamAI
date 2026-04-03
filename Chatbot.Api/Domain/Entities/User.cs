using System.ComponentModel.DataAnnotations;
using Chatbot.Api.Domain.Common;

namespace Chatbot.Api.Domain.Entities;

public class User : EntityBase
{
    [MaxLength(128)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(512)]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(128)]
    public string DisplayName { get; set; } = string.Empty;

    [MaxLength(128)]
    public string? CompanyName { get; set; }

    public ICollection<Team> OwnedTeams { get; set; } = [];

    public ICollection<Member> Memberships { get; set; } = [];

    public ICollection<UserSession> Sessions { get; set; } = [];

    public ICollection<TeamInvitation> SentInvitations { get; set; } = [];

    public ICollection<TeamInvitation> AcceptedInvitations { get; set; } = [];
}
