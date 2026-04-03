using System.ComponentModel.DataAnnotations;
using Chatbot.Api.Domain.Common;
using Chatbot.Api.Domain.Enums;

namespace Chatbot.Api.Domain.Entities;

public class ConciergeApp : EntityBase
{
    public Guid TeamId { get; set; }

    public Team? Team { get; set; }

    public Guid ProjectId { get; set; }

    public Project? Project { get; set; }

    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2048)]
    public string? Description { get; set; }

    [MaxLength(2048)]
    public string? ServiceScope { get; set; }

    [MaxLength(2048)]
    public string? WelcomeMessage { get; set; }

    [MaxLength(2048)]
    public string? FaqScope { get; set; }

    [MaxLength(256)]
    public string? BusinessHours { get; set; }

    [MaxLength(128)]
    public string? ChannelLabel { get; set; }

    public ConciergeAppStatus Status { get; set; } = ConciergeAppStatus.Draft;

    public Guid? PrimaryAiMemberId { get; set; }

    public Member? PrimaryAiMember { get; set; }

    public string? TicketCreationPolicy { get; set; }

    public string? HumanHandoffPolicy { get; set; }

    public ICollection<Conversation> Conversations { get; set; } = [];
}
