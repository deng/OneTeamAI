namespace Chatbot.Api.Domain.Common;

public abstract class EntityBase
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public long CreatedAtMs { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    public long UpdatedAtMs { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
}
