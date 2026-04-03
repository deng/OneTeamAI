using System.ComponentModel.DataAnnotations;
using Chatbot.Api.Domain.Common;
using Chatbot.Api.Domain.Enums;

namespace Chatbot.Api.Domain.Entities;

public class TicketActivity : EntityBase
{
    public Guid TeamId { get; set; }

    public Team? Team { get; set; }

    public Guid TicketId { get; set; }

    public Ticket? Ticket { get; set; }

    public Guid? ActorMemberId { get; set; }

    public Member? ActorMember { get; set; }

    public Guid? ActorUserId { get; set; }

    public User? ActorUser { get; set; }

    public TicketActivityType ActivityType { get; set; }

    [MaxLength(256)]
    public string Summary { get; set; } = string.Empty;

    [MaxLength(4096)]
    public string? Detail { get; set; }
}
