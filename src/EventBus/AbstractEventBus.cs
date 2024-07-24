using EventBus.Polly;
using EventBus.Serialize;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EventBus;

public abstract class AbstractEventBus<TOptions>
	: IEventPublisher, IHostedService
	where TOptions : EventBusOptionsBase, new()
{

	private readonly ILogger<AbstractEventBus<TOptions>> _logger;
	private readonly IServiceProvider _serviceProvider;
	private readonly EventBusSubscriptionInfo _subscriptionInfo;
	private readonly IEventBusSerializer _serializer;
	private readonly TOptions _options;

	protected TOptions Options => _options;
	protected EventBusSubscriptionInfo SubscriptionInfo => _subscriptionInfo;
	protected IEventBusSerializer Serializer => _serializer;

	protected AbstractEventBus(IOptions<TOptions> options, IEventBusSerializer serializer, IOptions<EventBusSubscriptionInfo> subscriptionInfo, ILogger<AbstractEventBus<TOptions>> logger, IServiceProvider serviceProvider)
	{

		_serviceProvider = serviceProvider;
		_logger = logger;
		_options = options.Value;
		_serializer = serializer;
		_subscriptionInfo = subscriptionInfo.Value;
	}

	public virtual async Task PublishAsync(EventBase @event, CancellationToken cancellation = default)
	{
		var policy = EventBusPolicyProvider.PublishAsyncRetryPolicy;

		await policy.ExecuteAsync(async () =>
		{
			try
			{
				await PublishCoreAsync(@event, cancellation);
			}
			catch (Exception ex)
			{
				_logger.LogError("Event Id:{Id} publish message error{exception}", @event.Id, ex);
				throw;
			}
		});

	}


	public abstract Task PublishCoreAsync(EventBase @event, CancellationToken cancellation = default);



	public abstract Task<EMetadata?> ReceiveMessageAsync(CancellationToken cancellationToken = default);



	public virtual async Task StartConsumeOneAsync(CancellationToken cancellationToken = default)
	{
		var data = await ReceiveMessageAsync(cancellationToken);

		if (data is null)
			return;

		//寻找支持的反序列化类型根据key
		if (!_subscriptionInfo.EventTypes.TryGetValue(data.Key, out var eventType))
		{
			_logger.LogWarning("Unable to resolve event type for event name {EventName}", data.Key);
			return;
		}

		try
		{
			//寻找对应类型的处理器
			await using var scope = _serviceProvider.CreateAsyncScope();
			foreach (var eventHandler in scope.ServiceProvider.GetKeyedServices<IEventHandler>(eventType))
			{
				//为每个处理器都构建个新的事件副本通过反序化的方式
				var @event = _serializer.Deserialize(data.Value, eventType);
				await eventHandler.Handle(@event);
			}

			//提交消费成功
			await data.AckFunc(cancellationToken);
		}
		catch (Exception ex)
		{
			//提交消费失败
			await data.FailFunc(cancellationToken);
			_logger.LogWarning(ex, "Error Processing Event \"{Event}\"", data.Key);
		}

	}



	//进行初始化
	public abstract Task InitializeAsync(CancellationToken cancellation = default);


	public virtual async Task StartAsync(CancellationToken cancellationToken)
	{
		//初始化
		await InitializeAsync(cancellationToken);

		//支持消费则默认开启消费
		if (_options.IsConsumer)
		{
			_ = Task.Factory.StartNew(async () =>
			{
				while (!cancellationToken.IsCancellationRequested)
				{
					await StartConsumeOneAsync(cancellationToken);
				}
			}, TaskCreationOptions.LongRunning);

		}
	}

	public virtual Task StopAsync(CancellationToken cancellationToken)
	{
		return Task.CompletedTask;
	}


}
