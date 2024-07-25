namespace EventBus.RocketMQ
{
	public sealed class RocketMQEventBusOptions : EventBusOptionsBase
	{
		//不使用阿里云无需设置AccessKey SecretKey
		public string AccessKey { get; set; } = string.Empty;

		public string SecretKey { get; set; } = string.Empty;

		//mq地址
		public string Endpoints { get; set; } = string.Empty;

		//消费话题   类比rabbitmq交换机
		public string ConsumerTopic { get; set; } = string.Empty;

		//消费组   类比rabbitmq消费队列
		public string ConsumerGroup { get; set; } = string.Empty;

		//生产话题
		public string ProducerTopic { get; set; } = string.Empty;

		//生产组
		public string ProducerGroup { get; set; } = string.Empty;

	}
}
