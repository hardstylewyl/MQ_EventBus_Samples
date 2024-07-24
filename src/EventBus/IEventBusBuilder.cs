using Microsoft.Extensions.DependencyInjection;

namespace EventBus;

public interface IEventBusBuilder
{
	public IServiceCollection Services { get; }
}


public class EventBusBuilder(IServiceCollection services) : IEventBusBuilder
{
	public IServiceCollection Services => services;

	public static IEventBusBuilder CreateBuilder(IServiceCollection services)
	{
		return new EventBusBuilder(services);
	}
}
