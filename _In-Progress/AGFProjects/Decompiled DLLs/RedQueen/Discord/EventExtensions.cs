using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord;

internal static class EventExtensions
{
	public static async Task InvokeAsync(this AsyncEvent<Func<Task>> eventHandler)
	{
		IReadOnlyList<Func<Task>> subscribers = eventHandler.Subscriptions;
		for (int i = 0; i < subscribers.Count; i++)
		{
			await subscribers[i]().ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	public static async Task InvokeAsync<T>(this AsyncEvent<Func<T, Task>> eventHandler, T arg)
	{
		IReadOnlyList<Func<T, Task>> subscribers = eventHandler.Subscriptions;
		for (int i = 0; i < subscribers.Count; i++)
		{
			await subscribers[i](arg).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	public static async Task InvokeAsync<T1, T2>(this AsyncEvent<Func<T1, T2, Task>> eventHandler, T1 arg1, T2 arg2)
	{
		IReadOnlyList<Func<T1, T2, Task>> subscribers = eventHandler.Subscriptions;
		for (int i = 0; i < subscribers.Count; i++)
		{
			await subscribers[i](arg1, arg2).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	public static async Task InvokeAsync<T1, T2, T3>(this AsyncEvent<Func<T1, T2, T3, Task>> eventHandler, T1 arg1, T2 arg2, T3 arg3)
	{
		IReadOnlyList<Func<T1, T2, T3, Task>> subscribers = eventHandler.Subscriptions;
		for (int i = 0; i < subscribers.Count; i++)
		{
			await subscribers[i](arg1, arg2, arg3).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	public static async Task InvokeAsync<T1, T2, T3, T4>(this AsyncEvent<Func<T1, T2, T3, T4, Task>> eventHandler, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
	{
		IReadOnlyList<Func<T1, T2, T3, T4, Task>> subscribers = eventHandler.Subscriptions;
		for (int i = 0; i < subscribers.Count; i++)
		{
			await subscribers[i](arg1, arg2, arg3, arg4).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	public static async Task InvokeAsync<T1, T2, T3, T4, T5>(this AsyncEvent<System.Func<T1, T2, T3, T4, T5, Task>> eventHandler, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
	{
		IReadOnlyList<System.Func<T1, T2, T3, T4, T5, Task>> subscribers = eventHandler.Subscriptions;
		for (int i = 0; i < subscribers.Count; i++)
		{
			await subscribers[i](arg1, arg2, arg3, arg4, arg5).ConfigureAwait(continueOnCapturedContext: false);
		}
	}
}
