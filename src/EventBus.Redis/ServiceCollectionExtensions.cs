using EventBus.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventBus.Redis;

public static class ServiceCollectionExtensions
{
	public static IEventBusBuilder AddRedisEventBus(this IServiceCollection services, Action<RedisEventBusOptions> optionSetup)
		=> services.AddEventBusCore<RedisEventBus, RedisEventBusOptions>(optionSetup);

	public static IEventBusBuilder AddRedisEventBus(this IServiceCollection services, IConfigurationManager configuration)
		=> services.AddEventBusCore<RedisEventBus, RedisEventBusOptions>(configuration);

	public static IEventBusBuilder AddRedisEventBus(this IServiceCollection services, IConfigurationManager configuration, string name)
		=> services.AddEventBusCore<RedisEventBus, RedisEventBusOptions>(configuration, name);
}
