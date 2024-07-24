namespace EventBus.Serialize;


public interface IEventBusSerializer
{
	byte[] SerializeToUtf8Bytes<TEvent>(TEvent @event)
		where TEvent : EventBase;

	string Serialize<TEvent>(TEvent @event)
	where TEvent : EventBase;


	EventBase Deserialize(byte[] data, Type type);

	EventBase Deserialize(string data, Type type);
}



