namespace EventBus.RabbitMQ;

public sealed class RabbitMQEventBusOptions : EventBusOptionsBase
{
	//链接字符串如 "amqp://guest:guest@localhost:5672/"
	public string ConnectionString { get; set; } = string.Empty;

	//交换机名称
	public string ExchangeName { get; set; } = string.Empty;

	//消费队列名称
	public string QueueName { get; set; } = string.Empty;
}
