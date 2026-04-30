using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

namespace System.Runtime.CompilerServices;

[_003C8fc9b3f2_002Dc007_002D481e_002Daac7_002D514446e17bef_003EIsReadOnly]
internal struct ValueTaskAwaiter(ValueTask value) : ICriticalNotifyCompletion, INotifyCompletion
{
	internal static readonly Action<object> s_invokeActionDelegate = delegate(object state)
	{
		if (!(state is Action action))
		{
			System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.state);
		}
		else
		{
			action();
		}
	};

	private readonly ValueTask _value = value;

	public bool IsCompleted
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return _value.IsCompleted;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[StackTraceHidden]
	public void GetResult()
	{
		_value.ThrowIfCompletedUnsuccessfully();
	}

	public void OnCompleted(Action continuation)
	{
		object obj = _value._obj;
		if (obj is Task task)
		{
			task.GetAwaiter().OnCompleted(continuation);
		}
		else if (obj != null)
		{
			Unsafe.As<IValueTaskSource>(obj).OnCompleted(s_invokeActionDelegate, continuation, _value._token, ValueTaskSourceOnCompletedFlags.UseSchedulingContext | ValueTaskSourceOnCompletedFlags.FlowExecutionContext);
		}
		else
		{
			ValueTask.CompletedTask.GetAwaiter().OnCompleted(continuation);
		}
	}

	public void UnsafeOnCompleted(Action continuation)
	{
		object obj = _value._obj;
		if (obj is Task task)
		{
			task.GetAwaiter().UnsafeOnCompleted(continuation);
		}
		else if (obj != null)
		{
			Unsafe.As<IValueTaskSource>(obj).OnCompleted(s_invokeActionDelegate, continuation, _value._token, ValueTaskSourceOnCompletedFlags.UseSchedulingContext);
		}
		else
		{
			ValueTask.CompletedTask.GetAwaiter().UnsafeOnCompleted(continuation);
		}
	}
}
[_003C8fc9b3f2_002Dc007_002D481e_002Daac7_002D514446e17bef_003EIsReadOnly]
internal struct ValueTaskAwaiter<TResult>(ValueTask<TResult> value) : ICriticalNotifyCompletion, INotifyCompletion
{
	private readonly ValueTask<TResult> _value = value;

	public bool IsCompleted
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return _value.IsCompleted;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[StackTraceHidden]
	public TResult GetResult()
	{
		return _value.Result;
	}

	public void OnCompleted(Action continuation)
	{
		object obj = _value._obj;
		if (obj is Task<TResult> task)
		{
			task.GetAwaiter().OnCompleted(continuation);
		}
		else if (obj != null)
		{
			Unsafe.As<IValueTaskSource<TResult>>(obj).OnCompleted(ValueTaskAwaiter.s_invokeActionDelegate, continuation, _value._token, ValueTaskSourceOnCompletedFlags.UseSchedulingContext | ValueTaskSourceOnCompletedFlags.FlowExecutionContext);
		}
		else
		{
			ValueTask.CompletedTask.GetAwaiter().OnCompleted(continuation);
		}
	}

	public void UnsafeOnCompleted(Action continuation)
	{
		object obj = _value._obj;
		if (obj is Task<TResult> task)
		{
			task.GetAwaiter().UnsafeOnCompleted(continuation);
		}
		else if (obj != null)
		{
			Unsafe.As<IValueTaskSource<TResult>>(obj).OnCompleted(ValueTaskAwaiter.s_invokeActionDelegate, continuation, _value._token, ValueTaskSourceOnCompletedFlags.UseSchedulingContext);
		}
		else
		{
			ValueTask.CompletedTask.GetAwaiter().UnsafeOnCompleted(continuation);
		}
	}
}
