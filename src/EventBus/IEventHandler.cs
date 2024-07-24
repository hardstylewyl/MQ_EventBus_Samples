namespace EventBus;

public interface IEventHandler<in TEvent> : IEventHandler
	where TEvent : EventBase
{
	Task Handle(TEvent @event);

	Task IEventHandler.Handle(EventBase @event) => Handle((TEvent)@event);
}

public interface IEventHandler
{
	Task Handle(EventBase @event);
}
