
using System.Net;
using Confluent.Kafka;
using EventBus.Serialize;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EventBus.Kafka;

public sealed class KafkaEventBus(IOptions<KafkaEventBusOptions> options,
	IEventBusSerializer serializer,
	IOptions<EventBusSubscriptionInfo> subscriptionInfo,
	ILogger<KafkaEventBus> logger,
	IServiceProvider serviceProvider)
	: AbstractEventBus<KafkaEventBusOptions>(options, serializer, subscriptionInfo, logger, serviceProvider), IDisposable
{

	private IConsumer<string, byte[]>? _consumer;
	private IProducer<string, byte[]>? _producer;


	public override Task InitializeAsync(CancellationToken cancellation = default)
	{
		if (!Options.IsConsumer)
		{
			return Task.CompletedTask;
		}

		//消费者配置
		var config = new ConsumerConfig
		{
			GroupId = Options.ConsumerGroupId,
			BootstrapServers = Options.BootstrapServers,
			//从最早的记录开始消费
			AutoOffsetReset = AutoOffsetReset.Earliest,
			//取消自动提交
			EnableAutoCommit = false,
			//每当使用者到达分区的末尾时，都会发出RD_KAFKA_RESP_ERR__PARTITION_EOF事件。
			EnablePartitionEof = true,
		};

		_consumer = new ConsumerBuilder<string, byte[]>(config)
			.Build();

		//订阅一个主题
		_consumer.Subscribe(Options.ConsumerTopic);

		return Task.CompletedTask;
	}

	public override async Task PublishCoreAsync(EventBase @event, CancellationToken cancellation = default)
	{
		_producer = new ProducerBuilder<string, byte[]>(
			 new ProducerConfig()
			 {
				 BootstrapServers = Options.BootstrapServers,
				 ClientId = Dns.GetHostName(),
			 })
			.Build();

		//构建消息体
		var body = new Message<string, byte[]>()
		{
			Key = @event.GetType().Name,
			Value = Serializer.SerializeToUtf8Bytes(@event)
		};

		//其中需要注意的是如果你的场景并发非常之高，官方文档推荐的方法是Produce而不是ProduceAsync
		await _producer.ProduceAsync(Options.ProducerTopic, body, cancellation);

		//释放链接对象
		_producer.Dispose();
	}

	public override Task<EMetadata?> ReceiveMessageAsync(CancellationToken cancellationToken = default)
	{
		var consumeResult = _consumer!.Consume(cancellationToken);
		//是否到达分区末尾 到达则进行跳过
		if (consumeResult.IsPartitionEOF)
		{
			logger.LogInformation(
				$"Reached end of topic {consumeResult.Topic}, partition {consumeResult.Partition}, offset {consumeResult.Offset}.");
			return Task.FromResult<EMetadata?>(null);
		}

		//解析数据
		var message = consumeResult.Message;
		var eData = EMetadata.Create(message.Key, message.Value, ct =>
		{
			//提交消费
			_consumer.Commit(consumeResult);
			return Task.CompletedTask;
		});

		//执行错误时
		eData.WithFailFunc(ct =>
		{
			return Task.CompletedTask;
		});

		return Task.FromResult<EMetadata?>(eData);
	}

	public void Dispose()
	{
		_consumer?.Dispose();
		_producer?.Dispose();
	}
}
