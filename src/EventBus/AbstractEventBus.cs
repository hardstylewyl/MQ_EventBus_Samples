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


	public abstract Task PublishCoreAsync(EventBase @event, CancellationToken cancellation = default);

	public abstract Task<EMetadata?> ReceiveMessageAsync(CancellationToken cancellationToken = default);

	public abstract Task InitializeAsync(CancellationToken cancellation = default);


	public virtual async Task PublishAsync(EventBase @event, CancellationToken cancellation = default)
	{
		var policy = EventBusPolicyProvider.PublishAsyncRetryPolicy;

		await policy.ExecuteAsync(async () =>
		{
			try
			{
				await PublishCoreAsync(@event, cancellation);
				_logger.LogInformation("Sent Event [{Name}] Id [{Id}] Success!!", @event.GetType().Name, @event.Id);
			}
			catch (Exception ex)
			{
				_logger.LogError("Sent Event [{Name}] Id [{Id}] Error {error}", @event.GetType().Name, @event.Id, ex);
				throw;
			}
		});

	}

	public virtual async Task StartConsumeOneAsync(CancellationToken cancellationToken = default)
	{
		var data = await ReceiveMessageAsync(cancellationToken);

		if (data is null)
			return;

		//寻找支持的反序列化类型根据key
		if (!_subscriptionInfo.EventTypes.TryGetValue(data.Key, out var eventType))
		{
			_logger.LogWarning("Event [{Name}] type is not supported", data.Key);
			return;
		}

		var eventId = Guid.Empty;
		try
		{
			//寻找对应类型的处理器
			await using var scope = _serviceProvider.CreateAsyncScope();
			foreach (var eventHandler in scope.ServiceProvider.GetKeyedServices<IEventHandler>(eventType))
			{
				//为每个处理器都构建个新的事件副本通过反序化的方式
				var @event = _serializer.Deserialize(data.Value, eventType);
				eventId = @event.Id;
				await eventHandler.Handle(@event);
			}

			//提交消费成功
			await data.AckFunc(cancellationToken);
			//_logger.LogInformation("Processing Event [{Name}] Id [{Id}] Success！！", data.Key, eventId);
		}
		catch (Exception ex)
		{
			//提交消费失败
			await data.FailFunc(cancellationToken);
			_logger.LogError(ex, "Processing Event [{Name}] Id [{Id}] Error {error}", data.Key, eventId, ex);
		}

	}

	public virtual async Task StartAsync(CancellationToken cancellationToken)
	{
		try
		{
			//初始化
			await InitializeAsync(cancellationToken);

			_logger.LogInformation("Event bus initialization successful！！");
		}
		catch (Exception)
		{
			_logger.LogCritical("Event bus initialization failed！！");
			throw;
		}

		//支持消费则默认开启消费
		if (_options.IsConsumer)
		{
			_logger.LogInformation("Start event consumption ！");

			_ = Task.Factory.StartNew(async () =>
			{
				while (!cancellationToken.IsCancellationRequested)
				{
					await StartConsumeOneAsync(cancellationToken);
				}
			}, TaskCreationOptions.LongRunning);

		}
		else
		{
			_logger.LogWarning("Currently, consumption messages are not supported." +
				" To support consumption, please configure IsConsumer: true in the configuration item");
		}
	}

	public virtual Task StopAsync(CancellationToken cancellationToken)
	{
		return Task.CompletedTask;
	}

}
