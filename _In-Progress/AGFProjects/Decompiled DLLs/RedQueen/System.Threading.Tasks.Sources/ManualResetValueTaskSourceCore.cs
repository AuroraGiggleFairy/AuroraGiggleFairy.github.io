using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

namespace System.Threading.Tasks.Sources;

[StructLayout(LayoutKind.Auto)]
internal struct ManualResetValueTaskSourceCore<TResult>
{
	private Action<object> _continuation;

	private object _continuationState;

	private ExecutionContext _executionContext;

	private object _capturedContext;

	private bool _completed;

	private TResult _result;

	private ExceptionDispatchInfo _error;

	private short _version;

	public bool RunContinuationsAsynchronously { get; set; }

	public short Version => _version;

	public void Reset()
	{
		_version++;
		_completed = false;
		_result = default(TResult);
		_error = null;
		_executionContext = null;
		_capturedContext = null;
		_continuation = null;
		_continuationState = null;
	}

	public void SetResult(TResult result)
	{
		_result = result;
		SignalCompletion();
	}

	public void SetException(Exception error)
	{
		_error = ExceptionDispatchInfo.Capture(error);
		SignalCompletion();
	}

	public ValueTaskSourceStatus GetStatus(short token)
	{
		ValidateToken(token);
		if (_continuation != null && _completed)
		{
			if (_error != null)
			{
				if (!(_error.SourceException is OperationCanceledException))
				{
					return ValueTaskSourceStatus.Faulted;
				}
				return ValueTaskSourceStatus.Canceled;
			}
			return ValueTaskSourceStatus.Succeeded;
		}
		return ValueTaskSourceStatus.Pending;
	}

	public TResult GetResult(short token)
	{
		ValidateToken(token);
		if (!_completed)
		{
			throw new InvalidOperationException();
		}
		_error?.Throw();
		return _result;
	}

	public void OnCompleted(Action<object> continuation, object state, short token, ValueTaskSourceOnCompletedFlags flags)
	{
		if (continuation == null)
		{
			throw new ArgumentNullException("continuation");
		}
		ValidateToken(token);
		if ((flags & ValueTaskSourceOnCompletedFlags.FlowExecutionContext) != ValueTaskSourceOnCompletedFlags.None)
		{
			_executionContext = ExecutionContext.Capture();
		}
		if ((flags & ValueTaskSourceOnCompletedFlags.UseSchedulingContext) != ValueTaskSourceOnCompletedFlags.None)
		{
			SynchronizationContext current = SynchronizationContext.Current;
			if (current != null && current.GetType() != typeof(SynchronizationContext))
			{
				_capturedContext = current;
			}
			else
			{
				TaskScheduler current2 = TaskScheduler.Current;
				if (current2 != TaskScheduler.Default)
				{
					_capturedContext = current2;
				}
			}
		}
		object obj = _continuation;
		if (obj == null)
		{
			_continuationState = state;
			obj = Interlocked.CompareExchange(ref _continuation, continuation, null);
		}
		if (obj == null)
		{
			return;
		}
		if (obj != ManualResetValueTaskSourceCoreShared.s_sentinel)
		{
			throw new InvalidOperationException();
		}
		object capturedContext = _capturedContext;
		if (capturedContext != null)
		{
			if (!(capturedContext is SynchronizationContext synchronizationContext))
			{
				if (capturedContext is TaskScheduler scheduler)
				{
					Task.Factory.StartNew(continuation, state, CancellationToken.None, TaskCreationOptions.DenyChildAttach, scheduler);
				}
			}
			else
			{
				synchronizationContext.Post(delegate(object s)
				{
					Tuple<Action<object>, object> tuple = (Tuple<Action<object>, object>)s;
					tuple.Item1(tuple.Item2);
				}, Tuple.Create(continuation, state));
			}
		}
		else
		{
			Task.Factory.StartNew(continuation, state, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
		}
	}

	private void ValidateToken(short token)
	{
		if (token != _version)
		{
			throw new InvalidOperationException();
		}
	}

	private void SignalCompletion()
	{
		if (_completed)
		{
			throw new InvalidOperationException();
		}
		_completed = true;
		if (_continuation == null && Interlocked.CompareExchange(ref _continuation, ManualResetValueTaskSourceCoreShared.s_sentinel, null) == null)
		{
			return;
		}
		if (_executionContext != null)
		{
			ExecutionContext.Run(_executionContext, delegate(object s)
			{
				((ManualResetValueTaskSourceCore<TResult>)s).InvokeContinuation();
			}, this);
		}
		else
		{
			InvokeContinuation();
		}
	}

	private void InvokeContinuation()
	{
		object capturedContext = _capturedContext;
		if (capturedContext != null)
		{
			if (!(capturedContext is SynchronizationContext synchronizationContext))
			{
				if (capturedContext is TaskScheduler scheduler)
				{
					Task.Factory.StartNew(_continuation, _continuationState, CancellationToken.None, TaskCreationOptions.DenyChildAttach, scheduler);
				}
			}
			else
			{
				synchronizationContext.Post(delegate(object s)
				{
					Tuple<Action<object>, object> tuple = (Tuple<Action<object>, object>)s;
					tuple.Item1(tuple.Item2);
				}, Tuple.Create(_continuation, _continuationState));
			}
		}
		else if (RunContinuationsAsynchronously)
		{
			Task.Factory.StartNew(_continuation, _continuationState, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
		}
		else
		{
			_continuation(_continuationState);
		}
	}
}
