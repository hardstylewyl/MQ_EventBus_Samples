using EventBus.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventBus.Kafka;

public static class ServiceCollectionExtensions
{
	public static IEventBusBuilder AddKafkaEventBus(this IServiceCollection services, Action<KafkaEventBusOptions> optionSetup)
		=> services.AddEventBusCore<KafkaEventBus, KafkaEventBusOptions>(optionSetup);

	public static IEventBusBuilder AddKafkaEventBus(this IServiceCollection services, IConfigurationManager configuration)
		=> services.AddEventBusCore<KafkaEventBus, KafkaEventBusOptions>(configuration);

	public static IEventBusBuilder AddKafkaEventBus(this IServiceCollection services, IConfigurationManager configuration, string name)
		=> services.AddEventBusCore<KafkaEventBus, KafkaEventBusOptions>(configuration, name);

}
