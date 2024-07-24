namespace EventBus;

public abstract class EventBusOptionsBase
{
	/// <summary>
	/// 是否支持消费
	/// </summary>
	public bool IsConsumer { get; set; } = false;
}
