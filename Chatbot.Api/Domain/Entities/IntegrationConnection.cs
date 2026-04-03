using System.ComponentModel.DataAnnotations;
using Chatbot.Api.Domain.Common;
using Chatbot.Api.Domain.Enums;

namespace Chatbot.Api.Domain.Entities;

public class IntegrationConnection : EntityBase
{
    public Guid TeamId { get; set; }

    public Team? Team { get; set; }

    public ExternalSystemType ExternalSystemType { get; set; } = ExternalSystemType.Unknown;

    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(512)]
    public string BaseUrl { get; set; } = string.Empty;

    public string? AuthConfig { get; set; }

    public bool IsEnabled { get; set; } = true;
}
