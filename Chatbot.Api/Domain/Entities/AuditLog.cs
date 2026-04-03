using System.ComponentModel.DataAnnotations;
using Chatbot.Api.Domain.Common;

namespace Chatbot.Api.Domain.Entities;

public class AuditLog : EntityBase
{
    public Guid? TeamId { get; set; }

    public Team? Team { get; set; }

    public Guid? UserId { get; set; }

    public User? User { get; set; }

    [MaxLength(128)]
    public string ActionType { get; set; } = string.Empty;

    [MaxLength(128)]
    public string EntityType { get; set; } = string.Empty;

    public Guid? EntityId { get; set; }

    [MaxLength(2048)]
    public string Summary { get; set; } = string.Empty;

    [MaxLength(64)]
    public string Result { get; set; } = "success";

    [MaxLength(128)]
    public string? IpAddress { get; set; }
}
