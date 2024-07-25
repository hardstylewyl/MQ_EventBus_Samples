using System.Collections.Concurrent;
using EventBus.Serialize;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NewLife.Log;
using NewLife.RocketMQ;

namespace EventBus.RocketMQ
{

	public sealed class RocketMQEventBus(
		IOptions<RocketMQEventBusOptions> options,
		IEventBusSerializer serializer,
		IOptions<EventBusSubscriptionInfo> subscriptionInfo,
		ILogger<AbstractEventBus<RocketMQEventBusOptions>> logger,
		IServiceProvider serviceProvider)
		: AbstractEventBus<RocketMQEventBusOptions>(options, serializer, subscriptionInfo, logger, serviceProvider), IDisposable
	{

		private Consumer? _consumer;
		private Producer? _producer;

		public override Task InitializeAsync(CancellationToken cancellation = default)
		{
			if (!Options.IsConsumer)
			{
				return Task.CompletedTask;
			}

			_consumer = new Consumer
			{
				NameServerAddress = Options.Endpoints,
				Topic = Options.ConsumerTopic,
				Group = Options.ConsumerGroup,


				FromLastOffset = true,
				BatchSize = 20,
				//采用二进制序列化的格式
				//SerializeType = SerializeType.ROCKETMQ,

				Log = XTrace.Log,
			};

			//异步消费消息
			_consumer.OnConsumeAsync = (queue, exts, ct) =>
			{
				CountdownEvent countdown = new(exts.Length);
				var count = exts.Length;
				foreach (var ext in exts)
				{
					var eData = EMetadata.Create(ext.Tags, ext.Body, ct =>
					{
						countdown.Signal();
						Interlocked.Decrement(ref count);
						return Task.CompletedTask;
					});

					eData.WithFailFunc(ct =>
					{
						countdown.Signal();
						return Task.CompletedTask;
					});
					dataQueue.Add(eData, ct);
				}

				countdown.Wait(ct);
				return Task.FromResult(count == 0);
			};

			//启动消费者开始进行消费
			_consumer.Start();
			return Task.CompletedTask;
		}

		public override async Task PublishCoreAsync(EventBase @event, CancellationToken cancellation = default)
		{
			_producer ??= new Producer
			{
				NameServerAddress = Options.Endpoints,
				Topic = Options.ProducerTopic,
				Group = Options.ProducerGroup,
				//采用二进制序列化的格式
				//SerializeType = SerializeType.ROCKETMQ,

				Log = XTrace.Log,
			};

			//要进行start才能使用
			_producer.Start();

			//Tag 是消息的一种分类标签，用于进一步细化同一主题（ConsumerTopic）下的消息类型
			//Key 通常用于标识单个消息的唯一性，在业务层面，它可以帮助开发者查找或追踪特定的消息,（可以为一个消息定义多个key）
			//Message Group (Consumer Group)：同消费组概念  消息组决定消息传递顺序。
			//构建一个消息
			var messageKey = Guid.NewGuid().ToString();
			var tags = @event.GetType().Name;

			//发布消息
			await _producer.PublishAsync(@event, tags, messageKey);

			//释放生产者
			_producer.Dispose();
		}

		private BlockingCollection<EMetadata> dataQueue = [];
		public override Task<EMetadata?> ReceiveMessageAsync(CancellationToken cancellationToken = default)
		{
			dataQueue.TryTake(out var eData, TimeSpan.FromMilliseconds(100));

			return Task.FromResult(eData);
		}

		public void Dispose()
		{
			_consumer?.Dispose();
			_producer?.Dispose();
		}
	}
}
