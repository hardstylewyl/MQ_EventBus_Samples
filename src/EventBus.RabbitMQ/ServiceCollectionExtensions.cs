using EventBus.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventBus.RabbitMQ;

public static class ServiceCollectionExtensions
{
	public static IEventBusBuilder AddRabbitMQEventBus(this IServiceCollection services, Action<RabbitMQEventBusOptions> optionSetup)
		=> services.AddEventBusCore<RabbitMQEventBus, RabbitMQEventBusOptions>(optionSetup);

	public static IEventBusBuilder AddRabbitMQEventBus(this IServiceCollection services, IConfigurationManager configuration)
		=> services.AddEventBusCore<RabbitMQEventBus, RabbitMQEventBusOptions>(configuration);

	public static IEventBusBuilder AddRabbitMQEventBus(this IServiceCollection services, IConfigurationManager configuration, string name)
		=> services.AddEventBusCore<RabbitMQEventBus, RabbitMQEventBusOptions>(configuration, name);
}
