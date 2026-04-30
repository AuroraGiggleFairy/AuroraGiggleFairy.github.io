using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq;

[_003C101b90a7_002Ded16_002D473d_002D9ce2_002D3a947e6817ed_003ENullableContext(1)]
[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 0, 1 })]
internal sealed class AsyncListPartition<[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(2)] TSource> : AsyncIterator<TSource>, IAsyncPartition<TSource>, IAsyncIListProvider<TSource>, IAsyncEnumerable<TSource>
{
	private readonly IList<TSource> _source;

	private readonly int _minIndexInclusive;

	private readonly int _maxIndexInclusive;

	private int _index;

	private int Count
	{
		get
		{
			int count = _source.Count;
			if (count <= _minIndexInclusive)
			{
				return 0;
			}
			return Math.Min(count - 1, _maxIndexInclusive) - _minIndexInclusive + 1;
		}
	}

	public AsyncListPartition(IList<TSource> source, int minIndexInclusive, int maxIndexInclusive)
	{
		_source = source;
		_minIndexInclusive = minIndexInclusive;
		_maxIndexInclusive = maxIndexInclusive;
		_index = 0;
	}

	public override AsyncIteratorBase<TSource> Clone()
	{
		return new AsyncListPartition<TSource>(_source, _minIndexInclusive, _maxIndexInclusive);
	}

	[_003C101b90a7_002Ded16_002D473d_002D9ce2_002D3a947e6817ed_003ENullableContext(0)]
	protected override ValueTask<bool> MoveNextCore()
	{
		if ((uint)_index <= (uint)(_maxIndexInclusive - _minIndexInclusive) && _index < _source.Count - _minIndexInclusive)
		{
			_current = _source[_minIndexInclusive + _index];
			_index++;
			return new ValueTask<bool>(result: true);
		}
		return Core();
		[_003C101b90a7_002Ded16_002D473d_002D9ce2_002D3a947e6817ed_003ENullableContext(0)]
		async ValueTask<bool> Core()
		{
			await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			return false;
		}
	}

	public IAsyncPartition<TSource> Skip(int count)
	{
		int num = _minIndexInclusive + count;
		if ((uint)num > (uint)_maxIndexInclusive)
		{
			return AsyncEnumerable.EmptyAsyncIterator<TSource>.Instance;
		}
		return new AsyncListPartition<TSource>(_source, num, _maxIndexInclusive);
	}

	public IAsyncPartition<TSource> Take(int count)
	{
		int num = _minIndexInclusive + count - 1;
		if ((uint)num >= (uint)_maxIndexInclusive)
		{
			return this;
		}
		return new AsyncListPartition<TSource>(_source, _minIndexInclusive, num);
	}

	[return: _003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 0, 0, 1 })]
	public ValueTask<Maybe<TSource>> TryGetElementAtAsync(int index, CancellationToken cancellationToken)
	{
		if ((uint)index <= (uint)(_maxIndexInclusive - _minIndexInclusive) && index < _source.Count - _minIndexInclusive)
		{
			return new ValueTask<Maybe<TSource>>(new Maybe<TSource>(_source[_minIndexInclusive + index]));
		}
		return new ValueTask<Maybe<TSource>>(default(Maybe<TSource>));
	}

	[return: _003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 0, 0, 1 })]
	public ValueTask<Maybe<TSource>> TryGetFirstAsync(CancellationToken cancellationToken)
	{
		if (_source.Count > _minIndexInclusive)
		{
			return new ValueTask<Maybe<TSource>>(new Maybe<TSource>(_source[_minIndexInclusive]));
		}
		return new ValueTask<Maybe<TSource>>(default(Maybe<TSource>));
	}

	[return: _003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 0, 0, 1 })]
	public ValueTask<Maybe<TSource>> TryGetLastAsync(CancellationToken cancellationToken)
	{
		int num = _source.Count - 1;
		if (num >= _minIndexInclusive)
		{
			return new ValueTask<Maybe<TSource>>(new Maybe<TSource>(_source[Math.Min(num, _maxIndexInclusive)]));
		}
		return new ValueTask<Maybe<TSource>>(default(Maybe<TSource>));
	}

	[return: _003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 0, 1, 1 })]
	public ValueTask<TSource[]> ToArrayAsync(CancellationToken cancellationToken)
	{
		int count = Count;
		if (count == 0)
		{
			return new ValueTask<TSource[]>(Array.Empty<TSource>());
		}
		TSource[] array = new TSource[count];
		int num = 0;
		int num2 = _minIndexInclusive;
		while (num != array.Length)
		{
			array[num] = _source[num2];
			num++;
			num2++;
		}
		return new ValueTask<TSource[]>(array);
	}

	[return: _003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 0, 1, 1 })]
	public ValueTask<List<TSource>> ToListAsync(CancellationToken cancellationToken)
	{
		int count = Count;
		if (count == 0)
		{
			return new ValueTask<List<TSource>>(new List<TSource>());
		}
		List<TSource> list = new List<TSource>(count);
		int num = _minIndexInclusive + count;
		for (int i = _minIndexInclusive; i != num; i++)
		{
			list.Add(_source[i]);
		}
		return new ValueTask<List<TSource>>(list);
	}

	[_003C101b90a7_002Ded16_002D473d_002D9ce2_002D3a947e6817ed_003ENullableContext(0)]
	public ValueTask<int> GetCountAsync(bool onlyIfCheap, CancellationToken cancellationToken)
	{
		return new ValueTask<int>(Count);
	}
}
