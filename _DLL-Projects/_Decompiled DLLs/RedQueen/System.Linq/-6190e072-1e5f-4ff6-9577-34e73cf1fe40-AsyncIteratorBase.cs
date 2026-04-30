using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq;

internal abstract class _003C6190e072_002D1e5f_002D4ff6_002D9577_002D34e73cf1fe40_003EAsyncIteratorBase<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource> : IAsyncEnumerable<TSource>, IAsyncEnumerator<TSource>, IAsyncDisposable
{
	private readonly int _threadId;

	protected _003C2475bc46_002D0e46_002D4b76_002Db59d_002D9532aae81fd6_003EAsyncIteratorState _state;

	protected CancellationToken _cancellationToken;

	[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(1)]
	public abstract TSource Current
	{
		[_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(1)]
		get;
	}

	protected _003C6190e072_002D1e5f_002D4ff6_002D9577_002D34e73cf1fe40_003EAsyncIteratorBase()
	{
		_threadId = Environment.CurrentManagedThreadId;
	}

	[_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(1)]
	public IAsyncEnumerator<TSource> GetAsyncEnumerator(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		_003C6190e072_002D1e5f_002D4ff6_002D9577_002D34e73cf1fe40_003EAsyncIteratorBase<TSource> obj = ((_state == _003C2475bc46_002D0e46_002D4b76_002Db59d_002D9532aae81fd6_003EAsyncIteratorState.New && _threadId == Environment.CurrentManagedThreadId) ? this : Clone());
		obj._state = _003C2475bc46_002D0e46_002D4b76_002Db59d_002D9532aae81fd6_003EAsyncIteratorState.Allocated;
		obj._cancellationToken = cancellationToken;
		return obj;
	}

	public virtual ValueTask DisposeAsync()
	{
		_state = _003C2475bc46_002D0e46_002D4b76_002Db59d_002D9532aae81fd6_003EAsyncIteratorState.Disposed;
		return default(ValueTask);
	}

	public async ValueTask<bool> MoveNextAsync()
	{
		if (_state == _003C2475bc46_002D0e46_002D4b76_002Db59d_002D9532aae81fd6_003EAsyncIteratorState.Disposed)
		{
			return false;
		}
		try
		{
			return await MoveNextCore().ConfigureAwait(continueOnCapturedContext: false);
		}
		catch
		{
			await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			throw;
		}
	}

	[_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(1)]
	public abstract _003C6190e072_002D1e5f_002D4ff6_002D9577_002D34e73cf1fe40_003EAsyncIteratorBase<TSource> Clone();

	protected abstract ValueTask<bool> MoveNextCore();
}
