using EventBus;

namespace RedisEventBus.Test;

public sealed class HelloEvent : EventBase
{
	public string Data { get; set; } = "Hello";
}


public sealed class HelloEventHandler : IEventHandler<HelloEvent>
{
	public Task Handle(HelloEvent @event)
	{

		Console.WriteLine($"收到消息{@event}");
		return Task.CompletedTask;
	}
}
