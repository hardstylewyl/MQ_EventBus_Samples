using System.Text.Json;
using EventBus.Serialize;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NewLife;
using NewLife.Caching;
using NewLife.Caching.Queues;

namespace EventBus.Redis;


internal sealed record RedisMessage(string EventName, byte[] MessageBody);

public sealed class RedisEventBus(
	IOptions<RedisEventBusOptions> options,
	IEventBusSerializer serializer,
	IOptions<EventBusSubscriptionInfo> subscriptionInfo,
	ILogger<RedisEventBus> logger,
	IServiceProvider serviceProvider)
	: AbstractEventBus<RedisEventBusOptions>(options, serializer, subscriptionInfo, logger, serviceProvider), IDisposable
{
	private FullRedis? fullRedis;
	private RedisReliableQueue<string>? _consumerQueue;

	public override Task InitializeAsync(CancellationToken cancellation = default)
	{
		//初始化client对象
		fullRedis = new FullRedis(Options.Host, Options.Password, Options.DbNumber)
		{
			Timeout = 5000
		};

		if (!Options.IsConsumer)
		{
			logger.LogWarning("当前不支持消费消息，如需支持消费请在配置项目配置IsConsumer:true");
			return Task.CompletedTask;
		}

		//初始化一个需要确认的消费队列
		_consumerQueue = fullRedis.GetReliableQueue<string>(Options.ConsumerTopic);

		//当消息发布后10s没有收到确认，将重新放入主队列
		_consumerQueue.RetryInterval = 10;

		return Task.CompletedTask;
	}

	public override Task PublishCoreAsync(EventBase @event, CancellationToken cancellation = default)
	{
		//获取一个需要确认的生产队列
		var producerQueue = fullRedis?.GetReliableQueue<RedisMessage>(Options.ProducerTopic);

		//构建消息
		var eventName = @event.GetType().Name;
		var message = Serializer.SerializeToUtf8Bytes(@event);
		var redisMessage = new RedisMessage(eventName, message);

		//填充至队列（消息发布）
		producerQueue?.Add(redisMessage);

		producerQueue.TryDispose();

		return Task.CompletedTask;
	}

	public override async Task<EMetadata?> ReceiveMessageAsync(CancellationToken cancellationToken = default)
	{
		var msgJson = await _consumerQueue!.TakeOneAsync(10, cancellationToken);
		if (msgJson is null)
		{
			return null;
		}

		var message = JsonSerializer.Deserialize<RedisMessage>(msgJson)!;

		return EMetadata.Create(message.EventName, message.MessageBody,
			 (ct) =>
		{
			_consumerQueue.Acknowledge(msgJson);
			return Task.CompletedTask;
		});

	}


	public void Dispose()
	{
		fullRedis?.Dispose();
		_consumerQueue?.Dispose();
	}
}
