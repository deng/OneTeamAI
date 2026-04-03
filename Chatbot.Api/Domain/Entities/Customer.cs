using System.ComponentModel.DataAnnotations;
using Chatbot.Api.Domain.Common;
using Chatbot.Api.Domain.Enums;

namespace Chatbot.Api.Domain.Entities;

public class Customer : ExternalMappedEntityBase
{
    public Guid TeamId { get; set; }

    public Team? Team { get; set; }

    [MaxLength(128)]
    public string DisplayName { get; set; } = string.Empty;

    [MaxLength(128)]
    public string? Email { get; set; }

    [MaxLength(64)]
    public string? PhoneNumber { get; set; }

    [MaxLength(128)]
    public string? CompanyName { get; set; }

    [MaxLength(128)]
    public string? SourceLabel { get; set; }

    [MaxLength(256)]
    public string? Tags { get; set; }

    public CustomerFollowUpStatus FollowUpStatus { get; set; } = CustomerFollowUpStatus.New;

    public DateTimeOffset? LastContactedAt { get; set; }

    public Guid? ProjectId { get; set; }

    public Project? Project { get; set; }

    [MaxLength(2048)]
    public string? Notes { get; set; }

    public CustomerStatus Status { get; set; } = CustomerStatus.Anonymous;

    public ICollection<Conversation> Conversations { get; set; } = [];
}
