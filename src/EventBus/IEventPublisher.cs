namespace EventBus;

public interface IEventPublisher
{
	Task PublishAsync(EventBase @event, CancellationToken cancellation = default);
}
