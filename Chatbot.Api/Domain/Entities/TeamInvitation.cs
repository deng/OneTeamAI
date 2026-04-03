using System.ComponentModel.DataAnnotations;
using Chatbot.Api.Domain.Common;
using Chatbot.Api.Domain.Enums;

namespace Chatbot.Api.Domain.Entities;

public class TeamInvitation : EntityBase
{
    public Guid TeamId { get; set; }

    public Team? Team { get; set; }

    [MaxLength(128)]
    public string Email { get; set; } = string.Empty;

    public MemberRole Role { get; set; } = MemberRole.Operator;

    [MaxLength(128)]
    public string? Title { get; set; }

    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;

    public Guid InvitedByUserId { get; set; }

    public User? InvitedByUser { get; set; }

    public Guid? AcceptedByUserId { get; set; }

    public User? AcceptedByUser { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset? RespondedAt { get; set; }
}
