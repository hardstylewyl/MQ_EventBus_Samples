namespace EventBus;

public class EMetadata
{
	public string Key { get; set; } = default!;
	public byte[] Value { get; set; } = default!;
	public Func<CancellationToken, Task> AckFunc { get; set; } = default!;
	public Func<CancellationToken, Task> FailFunc { get; set; } = (ct) => Task.CompletedTask;

	public void WithFailFunc(Func<CancellationToken, Task> func)
	{
		FailFunc = func;
	}

	public static EMetadata Create(string key, byte[] value, Func<CancellationToken, Task> ackFunc)
	{
		ArgumentNullException.ThrowIfNull(key, nameof(key));
		ArgumentNullException.ThrowIfNull(value, nameof(value));
		ArgumentNullException.ThrowIfNull(ackFunc, nameof(ackFunc));

		return new EMetadata { Key = key, Value = value, AckFunc = ackFunc };
	}
}
