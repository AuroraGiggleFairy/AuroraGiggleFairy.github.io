using System.Runtime.InteropServices;
using System.Threading;

namespace System.Runtime.CompilerServices;

[StructLayout(LayoutKind.Auto)]
internal struct AsyncIteratorMethodBuilder
{
	private AsyncTaskMethodBuilder _methodBuilder;

	private object _id;

	internal object ObjectIdForDebugger => _id ?? Interlocked.CompareExchange(ref _id, new object(), null) ?? _id;

	public static AsyncIteratorMethodBuilder Create()
	{
		return new AsyncIteratorMethodBuilder
		{
			_methodBuilder = AsyncTaskMethodBuilder.Create()
		};
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void MoveNext<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
	{
		_methodBuilder.Start(ref stateMachine);
	}

	public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine
	{
		_methodBuilder.AwaitOnCompleted(ref awaiter, ref stateMachine);
	}

	public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine
	{
		_methodBuilder.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);
	}

	public void Complete()
	{
		_methodBuilder.SetResult();
	}
}
