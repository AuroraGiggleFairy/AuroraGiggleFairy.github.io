using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.Collections.Generic;

[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(0)]
[_003C101b90a7_002Ded16_002D473d_002D9ce2_002D3a947e6817ed_003ENullableContext(1)]
internal static class AsyncEnumerator
{
	[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 0, 1 })]
	private sealed class AnonymousAsyncIterator<[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(2)] T> : AsyncIterator<T>
	{
		private readonly Func<T> _currentFunc;

		[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 1, 0 })]
		private readonly Func<ValueTask<bool>> _moveNext;

		[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(2)]
		private Func<ValueTask> _dispose;

		public AnonymousAsyncIterator([_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 1, 0 })] Func<ValueTask<bool>> moveNext, Func<T> currentFunc, Func<ValueTask> dispose)
		{
			_moveNext = moveNext;
			_currentFunc = currentFunc;
			_dispose = dispose;
			GetAsyncEnumerator(default(CancellationToken));
		}

		public override AsyncIteratorBase<T> Clone()
		{
			throw new NotSupportedException("AnonymousAsyncIterator cannot be cloned. It is only intended for use as an iterator.");
		}

		public override async ValueTask DisposeAsync()
		{
			Func<ValueTask> func = Interlocked.Exchange(ref _dispose, null);
			if (func != null)
			{
				await func().ConfigureAwait(continueOnCapturedContext: false);
			}
			await base.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
		}

		[_003C101b90a7_002Ded16_002D473d_002D9ce2_002D3a947e6817ed_003ENullableContext(0)]
		protected override async ValueTask<bool> MoveNextCore()
		{
			AsyncIteratorState state = _state;
			if (state != AsyncIteratorState.Allocated)
			{
				if (state != AsyncIteratorState.Iterating)
				{
					goto IL_0127;
				}
			}
			else
			{
				_state = AsyncIteratorState.Iterating;
			}
			if (await _moveNext().ConfigureAwait(continueOnCapturedContext: false))
			{
				_current = _currentFunc();
				return true;
			}
			await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			goto IL_0127;
			IL_0127:
			return false;
		}
	}

	[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(0)]
	private sealed class WithCancellationAsyncEnumerator<[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(2)] T> : IAsyncEnumerator<T>, IAsyncDisposable
	{
		private readonly IAsyncEnumerator<T> _source;

		private readonly CancellationToken _cancellationToken;

		public T Current => _source.Current;

		public WithCancellationAsyncEnumerator(IAsyncEnumerator<T> source, CancellationToken cancellationToken)
		{
			_source = source;
			_cancellationToken = cancellationToken;
		}

		public ValueTask DisposeAsync()
		{
			return _source.DisposeAsync();
		}

		[_003C101b90a7_002Ded16_002D473d_002D9ce2_002D3a947e6817ed_003ENullableContext(0)]
		public ValueTask<bool> MoveNextAsync()
		{
			_cancellationToken.ThrowIfCancellationRequested();
			return _source.MoveNextAsync();
		}
	}

	public static IAsyncEnumerator<T> Create<[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(2)] T>([_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 1, 0 })] Func<ValueTask<bool>> moveNextAsync, Func<T> getCurrent, Func<ValueTask> disposeAsync)
	{
		if (moveNextAsync == null)
		{
			throw Error.ArgumentNull("moveNextAsync");
		}
		return new AnonymousAsyncIterator<T>(moveNextAsync, getCurrent, disposeAsync);
	}

	[_003C101b90a7_002Ded16_002D473d_002D9ce2_002D3a947e6817ed_003ENullableContext(0)]
	public static ValueTask<bool> MoveNextAsync<[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(2)] T>([_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(1)] this IAsyncEnumerator<T> source, CancellationToken cancellationToken)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		cancellationToken.ThrowIfCancellationRequested();
		return source.MoveNextAsync();
	}

	public static IAsyncEnumerator<T> WithCancellation<[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(2)] T>(this IAsyncEnumerator<T> source, CancellationToken cancellationToken)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (cancellationToken == default(CancellationToken))
		{
			return source;
		}
		return new WithCancellationAsyncEnumerator<T>(source, cancellationToken);
	}
}
