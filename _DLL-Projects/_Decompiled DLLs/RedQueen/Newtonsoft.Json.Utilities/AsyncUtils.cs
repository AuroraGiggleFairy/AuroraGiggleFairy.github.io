using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Newtonsoft.Json.Utilities;

[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
internal static class AsyncUtils
{
	public static readonly Task<bool> False = Task.FromResult(result: false);

	public static readonly Task<bool> True = Task.FromResult(result: true);

	internal static readonly Task CompletedTask = Task.Delay(0);

	internal static Task<bool> ToAsync(this bool value)
	{
		if (!value)
		{
			return False;
		}
		return True;
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
	public static Task CancelIfRequestedAsync(this CancellationToken cancellationToken)
	{
		if (!cancellationToken.IsCancellationRequested)
		{
			return null;
		}
		return cancellationToken.FromCanceled();
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
	[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
	public static Task<T> CancelIfRequestedAsync<T>(this CancellationToken cancellationToken)
	{
		if (!cancellationToken.IsCancellationRequested)
		{
			return null;
		}
		return cancellationToken.FromCanceled<T>();
	}

	public static Task FromCanceled(this CancellationToken cancellationToken)
	{
		return new Task(delegate
		{
		}, cancellationToken);
	}

	public static Task<T> FromCanceled<[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] T>(this CancellationToken cancellationToken)
	{
		return new Task<T>([_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(0)] () => default(T), cancellationToken);
	}

	public static Task WriteAsync(this TextWriter writer, char value, CancellationToken cancellationToken)
	{
		if (!cancellationToken.IsCancellationRequested)
		{
			return writer.WriteAsync(value);
		}
		return cancellationToken.FromCanceled();
	}

	public static Task WriteAsync(this TextWriter writer, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] string value, CancellationToken cancellationToken)
	{
		if (!cancellationToken.IsCancellationRequested)
		{
			return writer.WriteAsync(value);
		}
		return cancellationToken.FromCanceled();
	}

	public static Task WriteAsync(this TextWriter writer, char[] value, int start, int count, CancellationToken cancellationToken)
	{
		if (!cancellationToken.IsCancellationRequested)
		{
			return writer.WriteAsync(value, start, count);
		}
		return cancellationToken.FromCanceled();
	}

	public static Task<int> ReadAsync(this TextReader reader, char[] buffer, int index, int count, CancellationToken cancellationToken)
	{
		if (!cancellationToken.IsCancellationRequested)
		{
			return reader.ReadAsync(buffer, index, count);
		}
		return cancellationToken.FromCanceled<int>();
	}

	public static bool IsCompletedSuccessfully(this Task task)
	{
		return task.Status == TaskStatus.RanToCompletion;
	}
}
