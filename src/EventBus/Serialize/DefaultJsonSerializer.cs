using System.Text.Json;
using Microsoft.Extensions.Options;

namespace EventBus.Serialize;

public sealed class DefaultJsonSerializer : IEventBusSerializer
{
	private readonly EventBusSubscriptionInfo _subscriptionInfo;

	public DefaultJsonSerializer(IOptions<EventBusSubscriptionInfo> subscriptionInfoOptions)
	{
		_subscriptionInfo = subscriptionInfoOptions.Value;
	}

	public EventBase Deserialize(byte[] bytes, Type type)
	{
		return JsonSerializer.Deserialize(bytes, type, _subscriptionInfo.JsonSerializerOptions)
			as EventBase ?? throw new JsonException($"Unable to deserialize {type.Name} message");
	}

	public EventBase Deserialize(string data, Type type)
	{
		return JsonSerializer.Deserialize(data, type, _subscriptionInfo.JsonSerializerOptions)
			as EventBase ?? throw new JsonException($"Unable to deserialize {type.Name} message");
	}

	public string Serialize<TEvent>(TEvent @event) where TEvent : EventBase
	{
		return JsonSerializer.Serialize(@event, @event.GetType(), _subscriptionInfo.JsonSerializerOptions);
	}

	public byte[] SerializeToUtf8Bytes<TEvent>(TEvent @event) where TEvent : EventBase
	{
		return JsonSerializer.SerializeToUtf8Bytes(@event, @event.GetType(), _subscriptionInfo.JsonSerializerOptions);
	}

}
