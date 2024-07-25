using System.Collections.Concurrent;
using EventBus.Serialize;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EventBus.RabbitMQ;

public sealed class RabbitMQEventBus(IOptions<RabbitMQEventBusOptions> options,
	IEventBusSerializer serializer,
	IOptions<EventBusSubscriptionInfo> subscriptionInfo,
	ILogger<RabbitMQEventBus> logger,
	IServiceProvider serviceProvider)
	: AbstractEventBus<RabbitMQEventBusOptions>(options, serializer, subscriptionInfo, logger, serviceProvider), IDisposable
{

	private IConnection? _amqpClient;
	private IModel? _consumerChannel;
	//使用阻塞队列来暂存消息
	private readonly BlockingCollection<EMetadata> dataQueue = [];

	public override Task InitializeAsync(CancellationToken cancellation = default)
	{
		var amqpClientFactroy = new ConnectionFactory()
		{
			Uri = new Uri(Options.ConnectionString),
			DispatchConsumersAsync = true
		};

		//初始化链接
		_amqpClient = amqpClientFactroy.CreateConnection();

		//不支持消费
		if (!Options.IsConsumer)
		{
			return Task.CompletedTask;
		}

		//初始化消费信道
		_consumerChannel = _amqpClient.CreateModel();

		//配置管道交换机
		_consumerChannel.ExchangeDeclare(exchange: Options.ExchangeName, type: "direct");
		//配置管道消费队列
		_consumerChannel.QueueDeclare(
			queue: Options.QueueName,
			durable: true,
			exclusive: false,
			autoDelete: false,
			arguments: null);


		//定义异步消费者	
		var consumer = new AsyncEventingBasicConsumer(_consumerChannel);
		//收到消息回调
		consumer.Received += (sender, args) =>
		{
			var eData = EMetadata.Create(args.RoutingKey, args.Body.ToArray(), ct =>
			{
				//消费成功提交
				_consumerChannel.BasicAck(args.DeliveryTag, multiple: false);
				return Task.CompletedTask;
			});

			//消费失败提交
			eData.WithFailFunc(ct =>
			{
				//通过BasicNack方法拒绝消息并设置requeue为true，以便消息重新入队等待再次消费。
				_consumerChannel.BasicNack(deliveryTag: args.DeliveryTag, multiple: false, requeue: true);
				return Task.CompletedTask;
			});

			// 加入阻塞队列
			dataQueue.Add(eData);

			return Task.CompletedTask;
		};

		//配置消费者接收消息参数	
		_consumerChannel.BasicConsume(
			queue: Options.QueueName,
			autoAck: false,
			consumer: consumer);

		//QueueBind则配置了消息的路由规则
		foreach (var (eventName, _) in SubscriptionInfo.EventTypes)
		{
			_consumerChannel.QueueBind(
				queue: Options.QueueName,
				exchange: Options.ExchangeName,
				routingKey: eventName);
		}

		return Task.CompletedTask;
	}

	public override Task PublishCoreAsync(EventBase @event, CancellationToken cancellation = default)
	{
		var channel = _amqpClient!.CreateModel();

		//交换机定义
		channel.ExchangeDeclare(exchange: Options.ExchangeName, type: "direct");

		//配置信道属性
		var properties = channel.CreateBasicProperties();
		// persistent 持续的
		properties.DeliveryMode = 2;

		//构建消息键（routingKey）和消息体
		var routingKey = @event.GetType().Name;
		var body = Serializer.SerializeToUtf8Bytes(@event);

		//发布消息
		channel.BasicPublish(Options.ExchangeName, routingKey, properties, body);

		//关闭发布管道
		channel.Close();
		return Task.CompletedTask;
	}



	public override Task<EMetadata?> ReceiveMessageAsync(CancellationToken cancellationToken = default)
	{
		dataQueue.TryTake(out var eData, TimeSpan.FromMilliseconds(100));

		return Task.FromResult(eData);
	}

	public void Dispose()
	{
		_amqpClient?.Dispose();
		_consumerChannel?.Dispose();
	}
}
