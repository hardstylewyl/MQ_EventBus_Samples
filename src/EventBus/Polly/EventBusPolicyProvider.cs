using System.Net.Sockets;
using Polly;
using Polly.Retry;

namespace EventBus.Polly;

public static class EventBusPolicyProvider
{
	private static readonly EventBusPolicyOptions DefaultPolicyOptions = new();

	static EventBusPolicyProvider()
	{
		Init(DefaultPolicyOptions);
	}

	//发布消息重试策略 异步
	public static AsyncRetryPolicy PublishAsyncRetryPolicy = null!;

	//消费消息重试策略 异步
	public static AsyncRetryPolicy ConsumerAsyncRetryPolicy = null!;

	//创建链接重试策略 同步
	public static RetryPolicy CreateConnectionRetryPolicy = null!;

	public static void Init(EventBusPolicyOptions options)
	{
		PublishAsyncRetryPolicy =
			Policy.Handle<Exception>()
				.WaitAndRetryAsync(options.PublishRetryCount,
					retryAttempt =>
						TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

		ConsumerAsyncRetryPolicy =
			Policy.Handle<Exception>()
				.WaitAndRetryAsync(options.ConsumerRetryCount,
					retryAttempt =>
						TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

		CreateConnectionRetryPolicy =
			Policy.Handle<Exception>()
				.Or<SocketException>()
				.WaitAndRetry(options.CreateConnectionRetryCount,
					retryAttempt =>
						TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
	}
}
