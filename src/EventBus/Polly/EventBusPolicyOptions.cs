namespace EventBus.Polly;

public sealed class EventBusPolicyOptions
{
	//最大消息发布重试次数
	public int PublishRetryCount { get; set; } = 3;

	//最大重试消费次数
	public int ConsumerRetryCount { get; set; } = 3;

	//最大创建链接重试次数
	public int CreateConnectionRetryCount { get; set; } = 5;
}
