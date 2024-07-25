namespace EventBus.Kafka;

public sealed class KafkaEventBusOptions : EventBusOptionsBase
{
	public string BootstrapServers { get; set; } = string.Empty;

	//消费组Id
	public string ConsumerGroupId { get; set; } = string.Empty;

	//消费话题 可以是多个
	public string ConsumerTopic { get; set; } = string.Empty;

	//生产话题 可以是多个
	public string ProducerTopic { get; set; } = string.Empty;
}
