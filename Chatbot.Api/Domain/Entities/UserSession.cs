using System.ComponentModel.DataAnnotations;
using Chatbot.Api.Domain.Common;

namespace Chatbot.Api.Domain.Entities;

public class UserSession : EntityBase
{
    public Guid UserId { get; set; }

    public User? User { get; set; }

    [MaxLength(128)]
    public string TokenHash { get; set; } = string.Empty;

    [MaxLength(256)]
    public string? UserAgent { get; set; }

    [MaxLength(64)]
    public string? IpAddress { get; set; }

    public DateTimeOffset LastSeenAt { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }

    [MaxLength(64)]
    public string? RevokedReason { get; set; }
}
