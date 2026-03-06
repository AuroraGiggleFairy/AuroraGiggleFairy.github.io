using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

namespace System.Runtime.CompilerServices;

[StructLayout(LayoutKind.Auto)]
[_003C8fc9b3f2_002Dc007_002D481e_002Daac7_002D514446e17bef_003EIsReadOnly]
internal struct ConfiguredValueTaskAwaitable(ValueTask value)
{
	[StructLayout(LayoutKind.Auto)]
	[_003C8fc9b3f2_002Dc007_002D481e_002Daac7_002D514446e17bef_003EIsReadOnly]
	public struct ConfiguredValueTaskAwaiter(ValueTask value) : ICriticalNotifyCompletion, INotifyCompletion
	{
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
				task.ConfigureAwait(_value._continueOnCapturedContext).GetAwaiter().OnCompleted(continuation);
			}
			else if (obj != null)
			{
				Unsafe.As<IValueTaskSource>(obj).OnCompleted(ValueTaskAwaiter.s_invokeActionDelegate, continuation, _value._token, (ValueTaskSourceOnCompletedFlags)(2 | (_value._continueOnCapturedContext ? 1 : 0)));
			}
			else
			{
				ValueTask.CompletedTask.ConfigureAwait(_value._continueOnCapturedContext).GetAwaiter().OnCompleted(continuation);
			}
		}

		public void UnsafeOnCompleted(Action continuation)
		{
			object obj = _value._obj;
			if (obj is Task task)
			{
				task.ConfigureAwait(_value._continueOnCapturedContext).GetAwaiter().UnsafeOnCompleted(continuation);
			}
			else if (obj != null)
			{
				Unsafe.As<IValueTaskSource>(obj).OnCompleted(ValueTaskAwaiter.s_invokeActionDelegate, continuation, _value._token, _value._continueOnCapturedContext ? ValueTaskSourceOnCompletedFlags.UseSchedulingContext : ValueTaskSourceOnCompletedFlags.None);
			}
			else
			{
				ValueTask.CompletedTask.ConfigureAwait(_value._continueOnCapturedContext).GetAwaiter().UnsafeOnCompleted(continuation);
			}
		}
	}

	private readonly ValueTask _value = value;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ConfiguredValueTaskAwaiter GetAwaiter()
	{
		return new ConfiguredValueTaskAwaiter(_value);
	}
}
[StructLayout(LayoutKind.Auto)]
[_003C8fc9b3f2_002Dc007_002D481e_002Daac7_002D514446e17bef_003EIsReadOnly]
internal struct ConfiguredValueTaskAwaitable<TResult>(ValueTask<TResult> value)
{
	[StructLayout(LayoutKind.Auto)]
	[_003C8fc9b3f2_002Dc007_002D481e_002Daac7_002D514446e17bef_003EIsReadOnly]
	public struct ConfiguredValueTaskAwaiter(ValueTask<TResult> value) : ICriticalNotifyCompletion, INotifyCompletion
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
				task.ConfigureAwait(_value._continueOnCapturedContext).GetAwaiter().OnCompleted(continuation);
			}
			else if (obj != null)
			{
				Unsafe.As<IValueTaskSource<TResult>>(obj).OnCompleted(ValueTaskAwaiter.s_invokeActionDelegate, continuation, _value._token, (ValueTaskSourceOnCompletedFlags)(2 | (_value._continueOnCapturedContext ? 1 : 0)));
			}
			else
			{
				ValueTask.CompletedTask.ConfigureAwait(_value._continueOnCapturedContext).GetAwaiter().OnCompleted(continuation);
			}
		}

		public void UnsafeOnCompleted(Action continuation)
		{
			object obj = _value._obj;
			if (obj is Task<TResult> task)
			{
				task.ConfigureAwait(_value._continueOnCapturedContext).GetAwaiter().UnsafeOnCompleted(continuation);
			}
			else if (obj != null)
			{
				Unsafe.As<IValueTaskSource<TResult>>(obj).OnCompleted(ValueTaskAwaiter.s_invokeActionDelegate, continuation, _value._token, _value._continueOnCapturedContext ? ValueTaskSourceOnCompletedFlags.UseSchedulingContext : ValueTaskSourceOnCompletedFlags.None);
			}
			else
			{
				ValueTask.CompletedTask.ConfigureAwait(_value._continueOnCapturedContext).GetAwaiter().UnsafeOnCompleted(continuation);
			}
		}
	}

	private readonly ValueTask<TResult> _value = value;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ConfiguredValueTaskAwaiter GetAwaiter()
	{
		return new ConfiguredValueTaskAwaiter(_value);
	}
}
