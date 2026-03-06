using System;
using System.Threading.Tasks;

namespace Discord;

internal static class TaskCompletionSourceExtensions
{
	public static Task SetResultAsync<T>(this TaskCompletionSource<T> source, T result)
	{
		return Task.Run(delegate
		{
			source.SetResult(result);
		});
	}

	public static Task<bool> TrySetResultAsync<T>(this TaskCompletionSource<T> source, T result)
	{
		return Task.Run(() => source.TrySetResult(result));
	}

	public static Task SetExceptionAsync<T>(this TaskCompletionSource<T> source, Exception ex)
	{
		return Task.Run(delegate
		{
			source.SetException(ex);
		});
	}

	public static Task<bool> TrySetExceptionAsync<T>(this TaskCompletionSource<T> source, Exception ex)
	{
		return Task.Run(() => source.TrySetException(ex));
	}

	public static Task SetCanceledAsync<T>(this TaskCompletionSource<T> source)
	{
		return Task.Run(delegate
		{
			source.SetCanceled();
		});
	}

	public static Task<bool> TrySetCanceledAsync<T>(this TaskCompletionSource<T> source)
	{
		return Task.Run(() => source.TrySetCanceled());
	}
}
