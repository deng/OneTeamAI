using Chatbot.Api.Domain.Enums;

namespace Chatbot.Api.Domain.Common;

public abstract class ExternalMappedEntityBase : EntityBase
{
    public RecordSourceType SourceType { get; set; } = RecordSourceType.Local;

    public ExternalSystemType? ExternalSystemType { get; set; }

    public string? ExternalId { get; set; }

    public string? ExternalRef { get; set; }
}
