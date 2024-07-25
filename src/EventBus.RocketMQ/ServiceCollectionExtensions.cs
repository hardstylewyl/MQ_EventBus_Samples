using EventBus.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventBus.RocketMQ;

public static class ServiceCollectionExtensions
{
	public static IEventBusBuilder AddRocketMQEventBus(this IServiceCollection services, Action<RocketMQEventBusOptions> optionSetup)
		=> services.AddEventBusCore<RocketMQEventBus, RocketMQEventBusOptions>(optionSetup);

	public static IEventBusBuilder AddRocketMQEventBus(this IServiceCollection services, IConfigurationManager configuration)
		=> services.AddEventBusCore<RocketMQEventBus, RocketMQEventBusOptions>(configuration);

	public static IEventBusBuilder AddRocketMQEventBus(this IServiceCollection services, IConfigurationManager configuration, string name)
		=> services.AddEventBusCore<RocketMQEventBus, RocketMQEventBusOptions>(configuration, name);
}
