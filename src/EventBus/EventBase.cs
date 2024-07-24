using System.Text.Json.Serialization;

namespace EventBus;

public abstract class EventBase
{
	[JsonInclude]
	public Guid Id { get; set; } = Guid.NewGuid();

	[JsonInclude]
	public DateTimeOffset CreationTimeOnUtc { get; set; } = DateTimeOffset.UtcNow;
}
