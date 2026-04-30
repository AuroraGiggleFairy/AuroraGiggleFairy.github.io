using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq;

[_003C101b90a7_002Ded16_002D473d_002D9ce2_002D3a947e6817ed_003ENullableContext(1)]
[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(0)]
internal abstract class AsyncIteratorBase<[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(2)] TSource> : IAsyncEnumerable<TSource>, IAsyncEnumerator<TSource>, IAsyncDisposable
{
	private readonly int _threadId;

	protected AsyncIteratorState _state;

	protected CancellationToken _cancellationToken;

	public abstract TSource Current { get; }

	protected AsyncIteratorBase()
	{
		_threadId = Environment.CurrentManagedThreadId;
	}

	public IAsyncEnumerator<TSource> GetAsyncEnumerator(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		AsyncIteratorBase<TSource> obj = ((_state == AsyncIteratorState.New && _threadId == Environment.CurrentManagedThreadId) ? this : Clone());
		obj._state = AsyncIteratorState.Allocated;
		obj._cancellationToken = cancellationToken;
		return obj;
	}

	public virtual ValueTask DisposeAsync()
	{
		_state = AsyncIteratorState.Disposed;
		return default(ValueTask);
	}

	[_003C101b90a7_002Ded16_002D473d_002D9ce2_002D3a947e6817ed_003ENullableContext(0)]
	public async ValueTask<bool> MoveNextAsync()
	{
		if (_state == AsyncIteratorState.Disposed)
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

	public abstract AsyncIteratorBase<TSource> Clone();

	[_003C101b90a7_002Ded16_002D473d_002D9ce2_002D3a947e6817ed_003ENullableContext(0)]
	protected abstract ValueTask<bool> MoveNextCore();

	public virtual IAsyncEnumerable<TResult> Select<[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(2)] TResult>(Func<TSource, TResult> selector)
	{
		return new AsyncEnumerable.SelectEnumerableAsyncIterator<TSource, TResult>(this, selector);
	}

	public virtual IAsyncEnumerable<TResult> Select<[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(2)] TResult>([_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 1, 1, 0, 1 })] Func<TSource, ValueTask<TResult>> selector)
	{
		return new AsyncEnumerable.SelectEnumerableAsyncIteratorWithTask<TSource, TResult>(this, selector);
	}

	public virtual IAsyncEnumerable<TResult> Select<[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(2)] TResult>([_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 1, 1, 0, 1 })] Func<TSource, CancellationToken, ValueTask<TResult>> selector)
	{
		return new AsyncEnumerable.SelectEnumerableAsyncIteratorWithTaskAndCancellation<TSource, TResult>(this, selector);
	}

	public virtual IAsyncEnumerable<TSource> Where(Func<TSource, bool> predicate)
	{
		return new AsyncEnumerable.WhereEnumerableAsyncIterator<TSource>(this, predicate);
	}

	public virtual IAsyncEnumerable<TSource> Where([_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 1, 1, 0 })] Func<TSource, ValueTask<bool>> predicate)
	{
		return new AsyncEnumerable.WhereEnumerableAsyncIteratorWithTask<TSource>(this, predicate);
	}

	public virtual IAsyncEnumerable<TSource> Where([_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 1, 1, 0 })] Func<TSource, CancellationToken, ValueTask<bool>> predicate)
	{
		return new AsyncEnumerable.WhereEnumerableAsyncIteratorWithTaskAndCancellation<TSource>(this, predicate);
	}
}
