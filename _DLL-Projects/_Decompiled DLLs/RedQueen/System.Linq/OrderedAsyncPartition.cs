using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq;

[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 0, 1 })]
[_003C101b90a7_002Ded16_002D473d_002D9ce2_002D3a947e6817ed_003ENullableContext(1)]
internal sealed class OrderedAsyncPartition<[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(2)] TElement> : AsyncIterator<TElement>, IAsyncPartition<TElement>, IAsyncIListProvider<TElement>, IAsyncEnumerable<TElement>
{
	private readonly OrderedAsyncEnumerable<TElement> _source;

	private readonly int _minIndexInclusive;

	private readonly int _maxIndexInclusive;

	[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 2, 1 })]
	private TElement[] _buffer;

	[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(2)]
	private int[] _indexes;

	private int _minIndexIterator;

	private int _maxIndexIterator;

	public OrderedAsyncPartition(OrderedAsyncEnumerable<TElement> source, int minIndexInclusive, int maxIndexInclusive)
	{
		_source = source;
		_minIndexInclusive = minIndexInclusive;
		_maxIndexInclusive = maxIndexInclusive;
	}

	public override AsyncIteratorBase<TElement> Clone()
	{
		return new OrderedAsyncPartition<TElement>(_source, _minIndexInclusive, _maxIndexInclusive);
	}

	[_003C101b90a7_002Ded16_002D473d_002D9ce2_002D3a947e6817ed_003ENullableContext(0)]
	public ValueTask<int> GetCountAsync(bool onlyIfCheap, CancellationToken cancellationToken)
	{
		return _source.GetCountAsync(_minIndexInclusive, _maxIndexInclusive, onlyIfCheap, cancellationToken);
	}

	public IAsyncPartition<TElement> Skip(int count)
	{
		int num = _minIndexInclusive + count;
		if ((uint)num > (uint)_maxIndexInclusive)
		{
			return AsyncEnumerable.EmptyAsyncIterator<TElement>.Instance;
		}
		return new OrderedAsyncPartition<TElement>(_source, num, _maxIndexInclusive);
	}

	public IAsyncPartition<TElement> Take(int count)
	{
		int num = _minIndexInclusive + count - 1;
		if ((uint)num >= (uint)_maxIndexInclusive)
		{
			return this;
		}
		return new OrderedAsyncPartition<TElement>(_source, _minIndexInclusive, num);
	}

	[return: _003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 0, 1, 1 })]
	public ValueTask<TElement[]> ToArrayAsync(CancellationToken cancellationToken)
	{
		return _source.ToArrayAsync(_minIndexInclusive, _maxIndexInclusive, cancellationToken);
	}

	[return: _003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 0, 1, 1 })]
	public ValueTask<List<TElement>> ToListAsync(CancellationToken cancellationToken)
	{
		return _source.ToListAsync(_minIndexInclusive, _maxIndexInclusive, cancellationToken);
	}

	[return: _003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 0, 0, 1 })]
	public ValueTask<Maybe<TElement>> TryGetElementAtAsync(int index, CancellationToken cancellationToken)
	{
		if ((uint)index <= (uint)(_maxIndexInclusive - _minIndexInclusive))
		{
			return _source.TryGetElementAtAsync(index + _minIndexInclusive, cancellationToken);
		}
		return new ValueTask<Maybe<TElement>>(default(Maybe<TElement>));
	}

	[return: _003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 0, 0, 1 })]
	public ValueTask<Maybe<TElement>> TryGetFirstAsync(CancellationToken cancellationToken)
	{
		return _source.TryGetElementAtAsync(_minIndexInclusive, cancellationToken);
	}

	[return: _003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 0, 0, 1 })]
	public ValueTask<Maybe<TElement>> TryGetLastAsync(CancellationToken cancellationToken)
	{
		return _source.TryGetLastAsync(_minIndexInclusive, _maxIndexInclusive, cancellationToken);
	}

	[_003C101b90a7_002Ded16_002D473d_002D9ce2_002D3a947e6817ed_003ENullableContext(0)]
	protected override async ValueTask<bool> MoveNextCore()
	{
		AsyncIteratorState state = _state;
		if (state != AsyncIteratorState.Allocated)
		{
			if (state == AsyncIteratorState.Iterating)
			{
				goto IL_02d6;
			}
		}
		else
		{
			_buffer = await _source.ToArrayAsync(_cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			_minIndexIterator = _minIndexInclusive;
			_maxIndexIterator = _maxIndexInclusive;
			int num = _buffer.Length;
			if (num > _minIndexIterator)
			{
				if (num <= _maxIndexIterator)
				{
					_maxIndexIterator = num - 1;
				}
				AsyncEnumerableSorter<TElement> asyncEnumerableSorter = _source.GetAsyncEnumerableSorter(_cancellationToken);
				if (_minIndexIterator == _maxIndexIterator)
				{
					_current = await asyncEnumerableSorter.ElementAt(_buffer, _buffer.Length, _minIndexIterator).ConfigureAwait(continueOnCapturedContext: false);
					_minIndexIterator = int.MaxValue;
					_maxIndexIterator = int.MinValue;
					_state = AsyncIteratorState.Iterating;
					return true;
				}
				_indexes = await asyncEnumerableSorter.Sort(_buffer, _buffer.Length, _minIndexIterator, _maxIndexIterator).ConfigureAwait(continueOnCapturedContext: false);
				_state = AsyncIteratorState.Iterating;
				goto IL_02d6;
			}
			await DisposeAsync();
		}
		goto IL_0380;
		IL_02d6:
		if (_minIndexIterator <= _maxIndexIterator)
		{
			_current = _buffer[_indexes[_minIndexIterator++]];
			return true;
		}
		await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
		goto IL_0380;
		IL_0380:
		return false;
	}

	public override async ValueTask DisposeAsync()
	{
		_buffer = null;
		_indexes = null;
		await base.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
	}
}
