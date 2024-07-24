using EventBus.Serialize;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EventBus.Extensions;

public static class ServiceCollectionExtensions
{
	public static IEventBusBuilder AddEventBusCore<TEventBus, TOptions>(this IServiceCollection services, Action<TOptions> optionSetup)
		where TOptions : EventBusOptionsBase, new()
		where TEventBus : AbstractEventBus<TOptions>
	{
		services.Configure(optionSetup);
		return AddEventBusCore<TEventBus, TOptions>(services);
	}

	public static IEventBusBuilder AddEventBusCore<TEventBus, TOptions>(this IServiceCollection services, IConfigurationManager configuration)
		where TOptions : EventBusOptionsBase, new()
		where TEventBus : AbstractEventBus<TOptions>
	{
		return AddEventBusCore<TEventBus, TOptions>(services, configuration, nameof(TOptions));
	}

	public static IEventBusBuilder AddEventBusCore<TEventBus, TOptions>(this IServiceCollection services, IConfigurationManager configuration, string name)
		where TOptions : EventBusOptionsBase, new()
		where TEventBus : AbstractEventBus<TOptions>
	{
		services.Configure<TOptions>(configuration.GetSection(name));
		return AddEventBusCore<TEventBus, TOptions>(services);
	}

	internal static IEventBusBuilder AddEventBusCore<TEventBus, TOptions>(this IServiceCollection services)
		where TOptions : EventBusOptionsBase, new()
		where TEventBus : AbstractEventBus<TOptions>
	{
		services.AddSingleton<IEventPublisher, TEventBus>();
		services.AddSingleton<IEventBusSerializer, DefaultJsonSerializer>();

		//创建一个后台服务用于事件订阅来进行集成事件消费
		services.AddSingleton<IHostedService>(sp =>
			(TEventBus)sp.GetRequiredService<IEventPublisher>());

		return EventBusBuilder.CreateBuilder(services);
	}
}
