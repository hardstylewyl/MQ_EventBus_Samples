namespace EventBus.Redis;

public sealed class RedisEventBusOptions : EventBusOptionsBase
{
	public string Host { get; set; } = string.Empty;
	public string Password { get; set; } = string.Empty;

	//使用的db序号
	public int DbNumber { get; set; } = 0;

	//消费话题
	public string ConsumerTopic { get; set; } = string.Empty;

	//生产话题
	public string ProducerTopic { get; set; } = string.Empty;
}
