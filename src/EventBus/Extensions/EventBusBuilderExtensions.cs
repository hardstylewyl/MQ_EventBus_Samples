using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using EventBus.Polly;
using Microsoft.Extensions.DependencyInjection;

namespace EventBus.Extensions;

public static class EventBusBuilderExtensions
{
	public static IEventBusBuilder ConfigureJsonOptions(this IEventBusBuilder eventBusBuilder,
		Action<JsonSerializerOptions> configure)
	{
		eventBusBuilder.Services.Configure<EventBusSubscriptionInfo>(o => { configure(o.JsonSerializerOptions); });

		return eventBusBuilder;
	}

	//添加一个集成事件的订阅
	public static IEventBusBuilder AddSubscription<T,
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
	TH>(
		this IEventBusBuilder eventBusBuilder)
		where T : EventBase
		where TH : class, IEventHandler<T>
	{
		eventBusBuilder.Services.AddKeyedTransient<IEventHandler, TH>(typeof(T));

		eventBusBuilder.Services.Configure<EventBusSubscriptionInfo>(o =>
		{
			o.EventTypes[typeof(T).Name] = typeof(T);
		});

		return eventBusBuilder;
	}

	//配置重试策略
	public static IEventBusBuilder ConfigureRetryStrategy(this IEventBusBuilder eventBusBuilder,
		Action<EventBusPolicyOptions>? configure = null)
	{
		var options = new EventBusPolicyOptions();
		configure?.Invoke(options);
		EventBusPolicyProvider.Init(options);
		return eventBusBuilder;
	}
}
