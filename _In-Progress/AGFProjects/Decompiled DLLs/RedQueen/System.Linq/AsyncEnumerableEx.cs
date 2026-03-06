using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq;

[_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(1)]
[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(0)]
internal static class AsyncEnumerableEx
{
	[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 0, 1 })]
	private sealed class DistinctAsyncIterator<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TKey> : _003Cba629dd4_002Dc8a2_002D48e8_002Db566_002D5f30ee16bebd_003EAsyncIterator<TSource>, IAsyncIListProvider<TSource>, IAsyncEnumerable<TSource>
	{
		[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 2, 1 })]
		private readonly IEqualityComparer<TKey> _comparer;

		private readonly Func<TSource, TKey> _keySelector;

		private readonly IAsyncEnumerable<TSource> _source;

		[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 2, 1 })]
		private IAsyncEnumerator<TSource> _enumerator;

		[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 2, 1 })]
		private _003Cfb00fe1d_002D4791_002D4daf_002D927b_002De0981736d749_003ESet<TKey> _set;

		public DistinctAsyncIterator(IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<TKey> comparer)
		{
			_source = source;
			_keySelector = keySelector;
			_comparer = comparer;
		}

		[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 0, 1, 1 })]
		public async ValueTask<TSource[]> ToArrayAsync(CancellationToken cancellationToken)
		{
			return (await FillSetAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).ToArray();
		}

		[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 0, 1, 1 })]
		public async ValueTask<List<TSource>> ToListAsync(CancellationToken cancellationToken)
		{
			return await FillSetAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}

		[_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(0)]
		public async ValueTask<int> GetCountAsync(bool onlyIfCheap, CancellationToken cancellationToken)
		{
			if (onlyIfCheap)
			{
				return -1;
			}
			int count = 0;
			_003Cfb00fe1d_002D4791_002D4daf_002D927b_002De0981736d749_003ESet<TKey> s = new _003Cfb00fe1d_002D4791_002D4daf_002D927b_002De0981736d749_003ESet<TKey>(_comparer);
			IAsyncEnumerator<TSource> enu = _source.GetAsyncEnumerator(cancellationToken);
			try
			{
				while (await enu.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false))
				{
					TSource current = enu.Current;
					if (s.Add(_keySelector(current)))
					{
						count++;
					}
				}
			}
			finally
			{
				await enu.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			}
			return count;
		}

		public override _003C6190e072_002D1e5f_002D4ff6_002D9577_002D34e73cf1fe40_003EAsyncIteratorBase<TSource> Clone()
		{
			return new DistinctAsyncIterator<TSource, TKey>(_source, _keySelector, _comparer);
		}

		public override async ValueTask DisposeAsync()
		{
			if (_enumerator != null)
			{
				await _enumerator.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
				_enumerator = null;
				_set = null;
			}
			await base.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
		}

		[_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(0)]
		protected override async ValueTask<bool> MoveNextCore()
		{
			_003C2475bc46_002D0e46_002D4b76_002Db59d_002D9532aae81fd6_003EAsyncIteratorState state = _state;
			TSource current;
			if (state != _003C2475bc46_002D0e46_002D4b76_002Db59d_002D9532aae81fd6_003EAsyncIteratorState.Allocated)
			{
				if (state == _003C2475bc46_002D0e46_002D4b76_002Db59d_002D9532aae81fd6_003EAsyncIteratorState.Iterating)
				{
					while (await _enumerator.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false))
					{
						current = _enumerator.Current;
						if (_set.Add(_keySelector(current)))
						{
							_current = current;
							return true;
						}
					}
				}
				await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
				return false;
			}
			_enumerator = _source.GetAsyncEnumerator(_cancellationToken);
			if (!(await _enumerator.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false)))
			{
				await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
				return false;
			}
			current = _enumerator.Current;
			_set = new _003Cfb00fe1d_002D4791_002D4daf_002D927b_002De0981736d749_003ESet<TKey>(_comparer);
			_set.Add(_keySelector(current));
			_current = current;
			_state = _003C2475bc46_002D0e46_002D4b76_002Db59d_002D9532aae81fd6_003EAsyncIteratorState.Iterating;
			return true;
		}

		private async Task<List<TSource>> FillSetAsync(CancellationToken cancellationToken)
		{
			_003Cfb00fe1d_002D4791_002D4daf_002D927b_002De0981736d749_003ESet<TKey> s = new _003Cfb00fe1d_002D4791_002D4daf_002D927b_002De0981736d749_003ESet<TKey>(_comparer);
			List<TSource> r = new List<TSource>();
			IAsyncEnumerator<TSource> enu = _source.GetAsyncEnumerator(cancellationToken);
			try
			{
				while (await enu.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false))
				{
					TSource current = enu.Current;
					if (s.Add(_keySelector(current)))
					{
						r.Add(current);
					}
				}
			}
			finally
			{
				await enu.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			}
			return r;
		}
	}

	[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 0, 1 })]
	private sealed class DistinctAsyncIteratorWithTask<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TKey> : _003Cba629dd4_002Dc8a2_002D48e8_002Db566_002D5f30ee16bebd_003EAsyncIterator<TSource>, IAsyncIListProvider<TSource>, IAsyncEnumerable<TSource>
	{
		[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 2, 1 })]
		private readonly IEqualityComparer<TKey> _comparer;

		[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 0, 1 })]
		private readonly Func<TSource, ValueTask<TKey>> _keySelector;

		private readonly IAsyncEnumerable<TSource> _source;

		[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 2, 1 })]
		private IAsyncEnumerator<TSource> _enumerator;

		[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 2, 1 })]
		private _003Cfb00fe1d_002D4791_002D4daf_002D927b_002De0981736d749_003ESet<TKey> _set;

		public DistinctAsyncIteratorWithTask(IAsyncEnumerable<TSource> source, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 0, 1 })] Func<TSource, ValueTask<TKey>> keySelector, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<TKey> comparer)
		{
			_source = source;
			_keySelector = keySelector;
			_comparer = comparer;
		}

		[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 0, 1, 1 })]
		public async ValueTask<TSource[]> ToArrayAsync(CancellationToken cancellationToken)
		{
			return (await FillSetAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).ToArray();
		}

		[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 0, 1, 1 })]
		public async ValueTask<List<TSource>> ToListAsync(CancellationToken cancellationToken)
		{
			return await FillSetAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}

		[_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(0)]
		public async ValueTask<int> GetCountAsync(bool onlyIfCheap, CancellationToken cancellationToken)
		{
			if (onlyIfCheap)
			{
				return -1;
			}
			int count = 0;
			_003Cfb00fe1d_002D4791_002D4daf_002D927b_002De0981736d749_003ESet<TKey> s = new _003Cfb00fe1d_002D4791_002D4daf_002D927b_002De0981736d749_003ESet<TKey>(_comparer);
			IAsyncEnumerator<TSource> enu = _source.GetAsyncEnumerator(cancellationToken);
			try
			{
				while (await enu.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false))
				{
					TSource current = enu.Current;
					_003Cfb00fe1d_002D4791_002D4daf_002D927b_002De0981736d749_003ESet<TKey> _003Cfb00fe1d_002D4791_002D4daf_002D927b_002De0981736d749_003ESet2 = s;
					if (_003Cfb00fe1d_002D4791_002D4daf_002D927b_002De0981736d749_003ESet2.Add(await _keySelector(current).ConfigureAwait(continueOnCapturedContext: false)))
					{
						count++;
					}
				}
			}
			finally
			{
				await enu.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			}
			return count;
		}

		public override _003C6190e072_002D1e5f_002D4ff6_002D9577_002D34e73cf1fe40_003EAsyncIteratorBase<TSource> Clone()
		{
			return new DistinctAsyncIteratorWithTask<TSource, TKey>(_source, _keySelector, _comparer);
		}

		public override async ValueTask DisposeAsync()
		{
			if (_enumerator != null)
			{
				await _enumerator.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
				_enumerator = null;
				_set = null;
			}
			await base.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
		}

		[_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(0)]
		protected override async ValueTask<bool> MoveNextCore()
		{
			_003C2475bc46_002D0e46_002D4b76_002Db59d_002D9532aae81fd6_003EAsyncIteratorState state = _state;
			TSource element;
			_003Cfb00fe1d_002D4791_002D4daf_002D927b_002De0981736d749_003ESet<TKey> set;
			if (state != _003C2475bc46_002D0e46_002D4b76_002Db59d_002D9532aae81fd6_003EAsyncIteratorState.Allocated)
			{
				if (state == _003C2475bc46_002D0e46_002D4b76_002Db59d_002D9532aae81fd6_003EAsyncIteratorState.Iterating)
				{
					while (await _enumerator.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false))
					{
						element = _enumerator.Current;
						set = _set;
						if (set.Add(await _keySelector(element).ConfigureAwait(continueOnCapturedContext: false)))
						{
							_current = element;
							return true;
						}
					}
				}
				await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
				return false;
			}
			_enumerator = _source.GetAsyncEnumerator(_cancellationToken);
			if (!(await _enumerator.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false)))
			{
				await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
				return false;
			}
			element = _enumerator.Current;
			_set = new _003Cfb00fe1d_002D4791_002D4daf_002D927b_002De0981736d749_003ESet<TKey>(_comparer);
			set = _set;
			set.Add(await _keySelector(element).ConfigureAwait(continueOnCapturedContext: false));
			_current = element;
			_state = _003C2475bc46_002D0e46_002D4b76_002Db59d_002D9532aae81fd6_003EAsyncIteratorState.Iterating;
			return true;
		}

		[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 0, 1, 1 })]
		private async ValueTask<List<TSource>> FillSetAsync(CancellationToken cancellationToken)
		{
			_003Cfb00fe1d_002D4791_002D4daf_002D927b_002De0981736d749_003ESet<TKey> s = new _003Cfb00fe1d_002D4791_002D4daf_002D927b_002De0981736d749_003ESet<TKey>(_comparer);
			List<TSource> r = new List<TSource>();
			IAsyncEnumerator<TSource> enu = _source.GetAsyncEnumerator(cancellationToken);
			try
			{
				while (await enu.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false))
				{
					TSource item = enu.Current;
					_003Cfb00fe1d_002D4791_002D4daf_002D927b_002De0981736d749_003ESet<TKey> _003Cfb00fe1d_002D4791_002D4daf_002D927b_002De0981736d749_003ESet2 = s;
					if (_003Cfb00fe1d_002D4791_002D4daf_002D927b_002De0981736d749_003ESet2.Add(await _keySelector(item).ConfigureAwait(continueOnCapturedContext: false)))
					{
						r.Add(item);
					}
				}
			}
			finally
			{
				await enu.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			}
			return r;
		}
	}

	[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 0, 1 })]
	private sealed class DistinctAsyncIteratorWithTaskAndCancellation<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TKey> : _003Cba629dd4_002Dc8a2_002D48e8_002Db566_002D5f30ee16bebd_003EAsyncIterator<TSource>, IAsyncIListProvider<TSource>, IAsyncEnumerable<TSource>
	{
		[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 2, 1 })]
		private readonly IEqualityComparer<TKey> _comparer;

		[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 0, 1 })]
		private readonly Func<TSource, CancellationToken, ValueTask<TKey>> _keySelector;

		private readonly IAsyncEnumerable<TSource> _source;

		[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 2, 1 })]
		private IAsyncEnumerator<TSource> _enumerator;

		[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 2, 1 })]
		private _003Cfb00fe1d_002D4791_002D4daf_002D927b_002De0981736d749_003ESet<TKey> _set;

		public DistinctAsyncIteratorWithTaskAndCancellation(IAsyncEnumerable<TSource> source, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 0, 1 })] Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<TKey> comparer)
		{
			_source = source;
			_keySelector = keySelector;
			_comparer = comparer;
		}

		[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 0, 1, 1 })]
		public async ValueTask<TSource[]> ToArrayAsync(CancellationToken cancellationToken)
		{
			return (await FillSetAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).ToArray();
		}

		[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 0, 1, 1 })]
		public async ValueTask<List<TSource>> ToListAsync(CancellationToken cancellationToken)
		{
			return await FillSetAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}

		[_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(0)]
		public async ValueTask<int> GetCountAsync(bool onlyIfCheap, CancellationToken cancellationToken)
		{
			if (onlyIfCheap)
			{
				return -1;
			}
			int count = 0;
			_003Cfb00fe1d_002D4791_002D4daf_002D927b_002De0981736d749_003ESet<TKey> s = new _003Cfb00fe1d_002D4791_002D4daf_002D927b_002De0981736d749_003ESet<TKey>(_comparer);
			IAsyncEnumerator<TSource> enu = _source.GetAsyncEnumerator(cancellationToken);
			try
			{
				while (await enu.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false))
				{
					TSource current = enu.Current;
					_003Cfb00fe1d_002D4791_002D4daf_002D927b_002De0981736d749_003ESet<TKey> _003Cfb00fe1d_002D4791_002D4daf_002D927b_002De0981736d749_003ESet2 = s;
					if (_003Cfb00fe1d_002D4791_002D4daf_002D927b_002De0981736d749_003ESet2.Add(await _keySelector(current, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)))
					{
						count++;
					}
				}
			}
			finally
			{
				await enu.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			}
			return count;
		}

		public override _003C6190e072_002D1e5f_002D4ff6_002D9577_002D34e73cf1fe40_003EAsyncIteratorBase<TSource> Clone()
		{
			return new DistinctAsyncIteratorWithTaskAndCancellation<TSource, TKey>(_source, _keySelector, _comparer);
		}

		public override async ValueTask DisposeAsync()
		{
			if (_enumerator != null)
			{
				await _enumerator.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
				_enumerator = null;
				_set = null;
			}
			await base.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
		}

		[_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(0)]
		protected override async ValueTask<bool> MoveNextCore()
		{
			_003C2475bc46_002D0e46_002D4b76_002Db59d_002D9532aae81fd6_003EAsyncIteratorState state = _state;
			TSource element;
			_003Cfb00fe1d_002D4791_002D4daf_002D927b_002De0981736d749_003ESet<TKey> set;
			if (state != _003C2475bc46_002D0e46_002D4b76_002Db59d_002D9532aae81fd6_003EAsyncIteratorState.Allocated)
			{
				if (state == _003C2475bc46_002D0e46_002D4b76_002Db59d_002D9532aae81fd6_003EAsyncIteratorState.Iterating)
				{
					while (await _enumerator.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false))
					{
						element = _enumerator.Current;
						set = _set;
						if (set.Add(await _keySelector(element, _cancellationToken).ConfigureAwait(continueOnCapturedContext: false)))
						{
							_current = element;
							return true;
						}
					}
				}
				await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
				return false;
			}
			_enumerator = _source.GetAsyncEnumerator(_cancellationToken);
			if (!(await _enumerator.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false)))
			{
				await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
				return false;
			}
			element = _enumerator.Current;
			_set = new _003Cfb00fe1d_002D4791_002D4daf_002D927b_002De0981736d749_003ESet<TKey>(_comparer);
			set = _set;
			set.Add(await _keySelector(element, _cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
			_current = element;
			_state = _003C2475bc46_002D0e46_002D4b76_002Db59d_002D9532aae81fd6_003EAsyncIteratorState.Iterating;
			return true;
		}

		[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 0, 1, 1 })]
		private async ValueTask<List<TSource>> FillSetAsync(CancellationToken cancellationToken)
		{
			_003Cfb00fe1d_002D4791_002D4daf_002D927b_002De0981736d749_003ESet<TKey> s = new _003Cfb00fe1d_002D4791_002D4daf_002D927b_002De0981736d749_003ESet<TKey>(_comparer);
			List<TSource> r = new List<TSource>();
			IAsyncEnumerator<TSource> enu = _source.GetAsyncEnumerator(cancellationToken);
			try
			{
				while (await enu.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false))
				{
					TSource item = enu.Current;
					_003Cfb00fe1d_002D4791_002D4daf_002D927b_002De0981736d749_003ESet<TKey> _003Cfb00fe1d_002D4791_002D4daf_002D927b_002De0981736d749_003ESet2 = s;
					if (_003Cfb00fe1d_002D4791_002D4daf_002D927b_002De0981736d749_003ESet2.Add(await _keySelector(item, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)))
					{
						r.Add(item);
					}
				}
			}
			finally
			{
				await enu.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			}
			return r;
		}
	}

	[_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(0)]
	private sealed class NeverAsyncEnumerable<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TValue> : IAsyncEnumerable<TValue>
	{
		private sealed class NeverAsyncEnumerator : IAsyncEnumerator<TValue>, IAsyncDisposable
		{
			private readonly CancellationToken _token;

			private CancellationTokenRegistration _registration;

			private bool _once;

			[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(1)]
			public TValue Current
			{
				[_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(1)]
				get
				{
					throw new InvalidOperationException();
				}
			}

			public NeverAsyncEnumerator(CancellationToken token)
			{
				_token = token;
			}

			public ValueTask DisposeAsync()
			{
				_registration.Dispose();
				return default(ValueTask);
			}

			public ValueTask<bool> MoveNextAsync()
			{
				if (_once)
				{
					return new ValueTask<bool>(result: false);
				}
				_once = true;
				TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
				_registration = _token.Register(delegate(object state)
				{
					((TaskCompletionSource<bool>)state).TrySetCanceled(_token);
				}, taskCompletionSource);
				return new ValueTask<bool>(taskCompletionSource.Task);
			}
		}

		[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(1)]
		internal static readonly NeverAsyncEnumerable<TValue> Instance = new NeverAsyncEnumerable<TValue>();

		[_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(1)]
		public IAsyncEnumerator<TValue> GetAsyncEnumerator(CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return new NeverAsyncEnumerator(cancellationToken);
		}
	}

	[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 0, 1 })]
	private sealed class OnErrorResumeNextAsyncIterator<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource> : _003Cba629dd4_002Dc8a2_002D48e8_002Db566_002D5f30ee16bebd_003EAsyncIterator<TSource>
	{
		private readonly IEnumerable<IAsyncEnumerable<TSource>> _sources;

		[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 2, 1 })]
		private IAsyncEnumerator<TSource> _enumerator;

		[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 2, 1, 1 })]
		private IEnumerator<IAsyncEnumerable<TSource>> _sourcesEnumerator;

		public OnErrorResumeNextAsyncIterator(IEnumerable<IAsyncEnumerable<TSource>> sources)
		{
			_sources = sources;
		}

		public override _003C6190e072_002D1e5f_002D4ff6_002D9577_002D34e73cf1fe40_003EAsyncIteratorBase<TSource> Clone()
		{
			return new OnErrorResumeNextAsyncIterator<TSource>(_sources);
		}

		public override async ValueTask DisposeAsync()
		{
			if (_sourcesEnumerator != null)
			{
				_sourcesEnumerator.Dispose();
				_sourcesEnumerator = null;
			}
			if (_enumerator != null)
			{
				await _enumerator.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
				_enumerator = null;
			}
			await base.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
		}

		[_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(0)]
		protected override async ValueTask<bool> MoveNextCore()
		{
			_003C2475bc46_002D0e46_002D4b76_002Db59d_002D9532aae81fd6_003EAsyncIteratorState state = _state;
			if (state != _003C2475bc46_002D0e46_002D4b76_002Db59d_002D9532aae81fd6_003EAsyncIteratorState.Allocated)
			{
				if (state != _003C2475bc46_002D0e46_002D4b76_002Db59d_002D9532aae81fd6_003EAsyncIteratorState.Iterating)
				{
					goto IL_0191;
				}
			}
			else
			{
				_sourcesEnumerator = _sources.GetEnumerator();
				_state = _003C2475bc46_002D0e46_002D4b76_002Db59d_002D9532aae81fd6_003EAsyncIteratorState.Iterating;
			}
			while (true)
			{
				if (_enumerator == null)
				{
					if (!_sourcesEnumerator.MoveNext())
					{
						break;
					}
					_enumerator = _sourcesEnumerator.Current.GetAsyncEnumerator(_cancellationToken);
				}
				try
				{
					if (await _enumerator.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false))
					{
						_current = _enumerator.Current;
						return true;
					}
				}
				catch
				{
				}
				await _enumerator.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
				_enumerator = null;
			}
			goto IL_0191;
			IL_0191:
			await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			return false;
		}
	}

	[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(0)]
	private sealed class ReturnEnumerable<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TValue> : IAsyncEnumerable<TValue>, IAsyncIListProvider<TValue>
	{
		[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(0)]
		private sealed class ReturnEnumerator : IAsyncEnumerator<TValue>, IAsyncDisposable
		{
			private bool _once;

			public TValue Current { get; private set; }

			public ReturnEnumerator(TValue current)
			{
				Current = current;
			}

			public ValueTask DisposeAsync()
			{
				Current = default(TValue);
				return default(ValueTask);
			}

			[_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(0)]
			public ValueTask<bool> MoveNextAsync()
			{
				if (_once)
				{
					return new ValueTask<bool>(result: false);
				}
				_once = true;
				return new ValueTask<bool>(result: true);
			}
		}

		private readonly TValue _value;

		public ReturnEnumerable(TValue value)
		{
			_value = value;
		}

		public IAsyncEnumerator<TValue> GetAsyncEnumerator(CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return new ReturnEnumerator(_value);
		}

		[_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(0)]
		public ValueTask<int> GetCountAsync(bool onlyIfCheap, CancellationToken cancellationToken)
		{
			return new ValueTask<int>(1);
		}

		[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 0, 1, 1 })]
		public ValueTask<TValue[]> ToArrayAsync(CancellationToken cancellationToken)
		{
			return new ValueTask<TValue[]>(new TValue[1] { _value });
		}

		[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 0, 1, 1 })]
		public ValueTask<List<TValue>> ToListAsync(CancellationToken cancellationToken)
		{
			return new ValueTask<List<TValue>>(new List<TValue>(1) { _value });
		}
	}

	[_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(0)]
	private sealed class ThrowEnumerable<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TValue> : IAsyncEnumerable<TValue>
	{
		private sealed class ThrowEnumerator : IAsyncEnumerator<TValue>, IAsyncDisposable
		{
			private ValueTask<bool> _moveNextThrows;

			[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(1)]
			public TValue Current
			{
				[_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(1)]
				get
				{
					return default(TValue);
				}
			}

			public ThrowEnumerator(ValueTask<bool> moveNextThrows)
			{
				_moveNextThrows = moveNextThrows;
			}

			public ValueTask DisposeAsync()
			{
				_moveNextThrows = new ValueTask<bool>(result: false);
				return default(ValueTask);
			}

			public ValueTask<bool> MoveNextAsync()
			{
				ValueTask<bool> moveNextThrows = _moveNextThrows;
				_moveNextThrows = new ValueTask<bool>(result: false);
				return moveNextThrows;
			}
		}

		private readonly ValueTask<bool> _moveNextThrows;

		public ThrowEnumerable(ValueTask<bool> moveNextThrows)
		{
			_moveNextThrows = moveNextThrows;
		}

		[_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(1)]
		public IAsyncEnumerator<TValue> GetAsyncEnumerator(CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return new ThrowEnumerator(_moveNextThrows);
		}
	}

	[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 0, 1 })]
	private sealed class TimeoutAsyncIterator<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource> : _003Cba629dd4_002Dc8a2_002D48e8_002Db566_002D5f30ee16bebd_003EAsyncIterator<TSource>
	{
		private readonly IAsyncEnumerable<TSource> _source;

		private readonly TimeSpan _timeout;

		[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 2, 1 })]
		private IAsyncEnumerator<TSource> _enumerator;

		[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)]
		private Task _loserTask;

		[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)]
		private CancellationTokenSource _sourceCTS;

		public TimeoutAsyncIterator(IAsyncEnumerable<TSource> source, TimeSpan timeout)
		{
			_source = source;
			_timeout = timeout;
		}

		public override _003C6190e072_002D1e5f_002D4ff6_002D9577_002D34e73cf1fe40_003EAsyncIteratorBase<TSource> Clone()
		{
			return new TimeoutAsyncIterator<TSource>(_source, _timeout);
		}

		public override async ValueTask DisposeAsync()
		{
			if (_loserTask != null)
			{
				await _loserTask.ConfigureAwait(continueOnCapturedContext: false);
				_loserTask = null;
				_enumerator = null;
			}
			else if (_enumerator != null)
			{
				await _enumerator.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
				_enumerator = null;
			}
			if (_sourceCTS != null)
			{
				_sourceCTS.Dispose();
				_sourceCTS = null;
			}
			await base.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
		}

		[_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(0)]
		protected override async ValueTask<bool> MoveNextCore()
		{
			_003C2475bc46_002D0e46_002D4b76_002Db59d_002D9532aae81fd6_003EAsyncIteratorState state = _state;
			if (state != _003C2475bc46_002D0e46_002D4b76_002Db59d_002D9532aae81fd6_003EAsyncIteratorState.Allocated)
			{
				if (state != _003C2475bc46_002D0e46_002D4b76_002Db59d_002D9532aae81fd6_003EAsyncIteratorState.Iterating)
				{
					goto IL_0272;
				}
			}
			else
			{
				_sourceCTS = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken);
				_enumerator = _source.GetAsyncEnumerator(_sourceCTS.Token);
				_state = _003C2475bc46_002D0e46_002D4b76_002Db59d_002D9532aae81fd6_003EAsyncIteratorState.Iterating;
			}
			ValueTask<bool> moveNext = _enumerator.MoveNextAsync();
			if (!moveNext.IsCompleted)
			{
				using CancellationTokenSource delayCts = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken);
				Task delay = Task.Delay(_timeout, delayCts.Token);
				Task<bool> next = moveNext.AsTask();
				if (await Task.WhenAny(next, delay).ConfigureAwait(continueOnCapturedContext: false) == delay)
				{
					_loserTask = next.ContinueWith([_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(0)] (Task<bool> _, object obj) => ((IAsyncDisposable)obj).DisposeAsync().AsTask(), _enumerator);
					_sourceCTS.Cancel();
					throw new TimeoutException();
				}
				delayCts.Cancel();
			}
			if (await moveNext.ConfigureAwait(continueOnCapturedContext: false))
			{
				_current = _enumerator.Current;
				return true;
			}
			goto IL_0272;
			IL_0272:
			await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			return false;
		}
	}

	public static IAsyncEnumerable<TSource> Amb<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(this IAsyncEnumerable<TSource> first, IAsyncEnumerable<TSource> second)
	{
		if (first == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("first");
		}
		if (second == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("second");
		}
		return Core(first, second, default(CancellationToken));
		[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })]
		static async IAsyncEnumerable<TSource> Core([_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })] IAsyncEnumerable<TSource> asyncEnumerable, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })] IAsyncEnumerable<TSource> asyncEnumerable2, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			IAsyncEnumerator<TSource> firstEnumerator = null;
			IAsyncEnumerator<TSource> secondEnumerator = null;
			Task<bool> firstMoveNext = null;
			Task<bool> secondMoveNext = null;
			CancellationTokenSource firstCancelToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			CancellationTokenSource secondCancelToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			try
			{
				firstEnumerator = asyncEnumerable.GetAsyncEnumerator(firstCancelToken.Token);
				firstMoveNext = firstEnumerator.MoveNextAsync().AsTask();
				secondEnumerator = asyncEnumerable2.GetAsyncEnumerator(secondCancelToken.Token);
				secondMoveNext = secondEnumerator.MoveNextAsync().AsTask();
			}
			catch
			{
				secondCancelToken.Cancel();
				firstCancelToken.Cancel();
				await Task.WhenAll(AwaitMoveNextAsyncAndDispose(secondMoveNext, secondEnumerator), AwaitMoveNextAsyncAndDispose(firstMoveNext, firstEnumerator)).ConfigureAwait(continueOnCapturedContext: false);
				throw;
			}
			Task<bool> task = await Task.WhenAny<bool>(firstMoveNext, secondMoveNext).ConfigureAwait(continueOnCapturedContext: false);
			IAsyncEnumerator<TSource> winner;
			Task disposeLoser;
			if (task == firstMoveNext)
			{
				winner = firstEnumerator;
				secondCancelToken.Cancel();
				disposeLoser = AwaitMoveNextAsyncAndDispose(secondMoveNext, secondEnumerator);
			}
			else
			{
				winner = secondEnumerator;
				firstCancelToken.Cancel();
				disposeLoser = AwaitMoveNextAsyncAndDispose(firstMoveNext, firstEnumerator);
			}
			try
			{
				ConfiguredAsyncDisposable I_3 = winner.ConfigureAwait(continueOnCapturedContext: false);
				try
				{
					if (await task.ConfigureAwait(continueOnCapturedContext: false))
					{
						yield return winner.Current;
						while (await winner.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false))
						{
							yield return winner.Current;
						}
					}
				}
				finally
				{
					IAsyncDisposable asyncDisposable = I_3 as IAsyncDisposable;
					if (asyncDisposable != null)
					{
						await asyncDisposable.DisposeAsync();
					}
				}
			}
			finally
			{
				await disposeLoser.ConfigureAwait(continueOnCapturedContext: false);
			}
		}
	}

	public static IAsyncEnumerable<TSource> Amb<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(params IAsyncEnumerable<TSource>[] sources)
	{
		if (sources == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("sources");
		}
		return Core(sources, default(CancellationToken));
		[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })]
		static async IAsyncEnumerable<TSource> Core([_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 0 })] IAsyncEnumerable<TSource>[] array, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			int n = array.Length;
			IAsyncEnumerator<TSource>[] enumerators = new IAsyncEnumerator<TSource>[n];
			Task<bool>[] moveNexts = new Task<bool>[n];
			CancellationTokenSource[] individualTokenSources = new CancellationTokenSource[n];
			for (int i = 0; i < n; i++)
			{
				individualTokenSources[i] = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			}
			try
			{
				for (int j = 0; j < n; j++)
				{
					moveNexts[j] = (enumerators[j] = array[j].GetAsyncEnumerator(individualTokenSources[j].Token)).MoveNextAsync().AsTask();
				}
			}
			catch
			{
				Task[] array2 = new Task[n];
				for (int num = n - 1; num >= 0; num--)
				{
					individualTokenSources[num].Cancel();
					array2[num] = AwaitMoveNextAsyncAndDispose(moveNexts[num], enumerators[num]);
				}
				await Task.WhenAll(array2).ConfigureAwait(continueOnCapturedContext: false);
				throw;
			}
			Task<bool> task = await Task.WhenAny(moveNexts).ConfigureAwait(continueOnCapturedContext: false);
			int num2 = Array.IndexOf(moveNexts, task);
			IAsyncEnumerator<TSource> winner = enumerators[num2];
			List<Task> list = new List<Task>(n - 1);
			for (int num3 = n - 1; num3 >= 0; num3--)
			{
				if (num3 != num2)
				{
					individualTokenSources[num3].Cancel();
					Task item = AwaitMoveNextAsyncAndDispose(moveNexts[num3], enumerators[num3]);
					list.Add(item);
				}
			}
			Task cleanupLosers = Task.WhenAll(list);
			try
			{
				ConfiguredAsyncDisposable I_3 = winner.ConfigureAwait(continueOnCapturedContext: false);
				try
				{
					if (await task.ConfigureAwait(continueOnCapturedContext: false))
					{
						yield return winner.Current;
						while (await winner.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false))
						{
							yield return winner.Current;
						}
					}
				}
				finally
				{
					IAsyncDisposable asyncDisposable = I_3 as IAsyncDisposable;
					if (asyncDisposable != null)
					{
						await asyncDisposable.DisposeAsync();
					}
				}
			}
			finally
			{
				await cleanupLosers.ConfigureAwait(continueOnCapturedContext: false);
			}
		}
	}

	public static IAsyncEnumerable<TSource> Amb<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(this IEnumerable<IAsyncEnumerable<TSource>> sources)
	{
		if (sources == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("sources");
		}
		return Amb(sources.ToArray());
	}

	[_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(2)]
	[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(1)]
	private static async Task AwaitMoveNextAsyncAndDispose<T>(Task<bool> moveNextAsync, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 2, 1 })] IAsyncEnumerator<T> enumerator)
	{
		if (enumerator == null)
		{
			return;
		}
		ConfiguredAsyncDisposable I_0 = enumerator.ConfigureAwait(continueOnCapturedContext: false);
		try
		{
			if (moveNextAsync != null)
			{
				try
				{
					await moveNextAsync.ConfigureAwait(continueOnCapturedContext: false);
				}
				catch (TaskCanceledException)
				{
				}
			}
		}
		finally
		{
			IAsyncDisposable asyncDisposable = I_0 as IAsyncDisposable;
			if (asyncDisposable != null)
			{
				await asyncDisposable.DisposeAsync();
			}
		}
	}

	public static IAsyncEnumerable<IList<TSource>> Buffer<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(this IAsyncEnumerable<TSource> source, int count)
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (count <= 0)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentOutOfRange("count");
		}
		return Core(source, count, default(CancellationToken));
		[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 0 })]
		static async IAsyncEnumerable<IList<TSource>> Core([_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })] IAsyncEnumerable<TSource> source2, int num, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			List<TSource> buffer = new List<TSource>(num);
			await foreach (TSource item in source2.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				buffer.Add(item);
				if (buffer.Count == num)
				{
					yield return buffer;
					buffer = new List<TSource>(num);
				}
			}
			if (buffer.Count > 0)
			{
				yield return buffer;
			}
		}
	}

	public static IAsyncEnumerable<IList<TSource>> Buffer<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(this IAsyncEnumerable<TSource> source, int count, int skip)
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (count <= 0)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentOutOfRange("count");
		}
		if (skip <= 0)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentOutOfRange("skip");
		}
		return Core(source, count, skip, default(CancellationToken));
		[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 0 })]
		static async IAsyncEnumerable<IList<TSource>> Core([_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })] IAsyncEnumerable<TSource> source2, int num2, int num, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			Queue<IList<TSource>> buffers = new Queue<IList<TSource>>();
			int index = 0;
			await foreach (TSource item in source2.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				if (index++ % num == 0)
				{
					buffers.Enqueue(new List<TSource>(num2));
				}
				foreach (IList<TSource> item2 in buffers)
				{
					item2.Add(item);
				}
				if (buffers.Count > 0 && buffers.Peek().Count == num2)
				{
					yield return buffers.Dequeue();
				}
			}
			while (buffers.Count > 0)
			{
				yield return buffers.Dequeue();
			}
		}
	}

	public static IAsyncEnumerable<TSource> Catch<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(0)] TException>(this IAsyncEnumerable<TSource> source, Func<TException, IAsyncEnumerable<TSource>> handler) where TException : Exception
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (handler == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("handler");
		}
		return Core(source, handler, default(CancellationToken));
		[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })]
		static async IAsyncEnumerable<TSource> Core([_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })] IAsyncEnumerable<TSource> enumerable, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 1, 0 })] Func<TException, IAsyncEnumerable<TSource>> func, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			IAsyncEnumerable<TSource> err = null;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = _003Cc67a3c72_002Dc19d_002D43dc_002D869b_002Da94521f87115_003EAsyncEnumerableExt.GetConfiguredAsyncEnumerator(enumerable, cancellationToken, continueOnCapturedContext: false);
			try
			{
				while (true)
				{
					TSource current;
					try
					{
						if (await e.MoveNextAsync())
						{
							current = e.Current;
							goto IL_011b;
						}
					}
					catch (TException arg)
					{
						err = func(arg);
					}
					break;
					IL_011b:
					yield return current;
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			if (err != null)
			{
				await foreach (TSource item in err.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
				{
					yield return item;
				}
			}
		}
	}

	public static IAsyncEnumerable<TSource> Catch<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(0)] TException>(this IAsyncEnumerable<TSource> source, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 0, 1, 1 })] Func<TException, ValueTask<IAsyncEnumerable<TSource>>> handler) where TException : Exception
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (handler == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("handler");
		}
		return Core(source, handler, default(CancellationToken));
		[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })]
		static async IAsyncEnumerable<TSource> Core([_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })] IAsyncEnumerable<TSource> enumerable, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 0, 1, 0 })] Func<TException, ValueTask<IAsyncEnumerable<TSource>>> func, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			IAsyncEnumerable<TSource> err = null;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = _003Cc67a3c72_002Dc19d_002D43dc_002D869b_002Da94521f87115_003EAsyncEnumerableExt.GetConfiguredAsyncEnumerator(enumerable, cancellationToken, continueOnCapturedContext: false);
			try
			{
				TSource c = default(TSource);
				object obj = default(object);
				while (true)
				{
					int num = 0;
					try
					{
						if (await e.MoveNextAsync())
						{
							c = e.Current;
							goto IL_0136;
						}
					}
					catch (TException ex)
					{
						obj = ex;
						num = 1;
						goto IL_0136;
					}
					break;
					IL_0136:
					if (num == 1)
					{
						TException arg = (TException)obj;
						err = await func(arg).ConfigureAwait(continueOnCapturedContext: false);
						break;
					}
					obj = null;
					yield return c;
					c = default(TSource);
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			if (err != null)
			{
				await foreach (TSource item in err.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
				{
					yield return item;
				}
			}
		}
	}

	public static IAsyncEnumerable<TSource> Catch<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(0)] TException>(this IAsyncEnumerable<TSource> source, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 0, 1, 1 })] Func<TException, CancellationToken, ValueTask<IAsyncEnumerable<TSource>>> handler) where TException : Exception
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (handler == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("handler");
		}
		return Core(source, handler, default(CancellationToken));
		[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })]
		static async IAsyncEnumerable<TSource> Core([_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })] IAsyncEnumerable<TSource> enumerable, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 0, 1, 0 })] Func<TException, CancellationToken, ValueTask<IAsyncEnumerable<TSource>>> func, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			IAsyncEnumerable<TSource> err = null;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = _003Cc67a3c72_002Dc19d_002D43dc_002D869b_002Da94521f87115_003EAsyncEnumerableExt.GetConfiguredAsyncEnumerator(enumerable, cancellationToken, continueOnCapturedContext: false);
			try
			{
				TSource c = default(TSource);
				object obj = default(object);
				while (true)
				{
					int num = 0;
					try
					{
						if (await e.MoveNextAsync())
						{
							c = e.Current;
							goto IL_0136;
						}
					}
					catch (TException ex)
					{
						obj = ex;
						num = 1;
						goto IL_0136;
					}
					break;
					IL_0136:
					if (num == 1)
					{
						TException arg = (TException)obj;
						err = await func(arg, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
						break;
					}
					obj = null;
					yield return c;
					c = default(TSource);
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			if (err != null)
			{
				await foreach (TSource item in err.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
				{
					yield return item;
				}
			}
		}
	}

	public static IAsyncEnumerable<TSource> Catch<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(this IEnumerable<IAsyncEnumerable<TSource>> sources)
	{
		if (sources == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("sources");
		}
		return CatchCore(sources);
	}

	public static IAsyncEnumerable<TSource> Catch<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(params IAsyncEnumerable<TSource>[] sources)
	{
		if (sources == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("sources");
		}
		return CatchCore(sources);
	}

	public static IAsyncEnumerable<TSource> Catch<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(this IAsyncEnumerable<TSource> first, IAsyncEnumerable<TSource> second)
	{
		if (first == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("first");
		}
		if (second == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("second");
		}
		return CatchCore(new IAsyncEnumerable<TSource>[2] { first, second });
	}

	private static async IAsyncEnumerable<TSource> CatchCore<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(IEnumerable<IAsyncEnumerable<TSource>> sources, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
	{
		ExceptionDispatchInfo error = null;
		foreach (IAsyncEnumerable<TSource> source2 in sources)
		{
			{
				ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = _003Cc67a3c72_002Dc19d_002D43dc_002D869b_002Da94521f87115_003EAsyncEnumerableExt.GetConfiguredAsyncEnumerator(source2, cancellationToken, continueOnCapturedContext: false);
				try
				{
					error = null;
					while (true)
					{
						TSource current2;
						try
						{
							if (!(await e.MoveNextAsync()))
							{
								break;
							}
							current2 = e.Current;
							goto IL_013a;
						}
						catch (Exception source)
						{
							error = ExceptionDispatchInfo.Capture(source);
						}
						break;
						IL_013a:
						yield return current2;
					}
					if (error != null)
					{
						continue;
					}
					break;
				}
				finally
				{
					IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
					if (asyncDisposable != null)
					{
						await asyncDisposable.DisposeAsync();
					}
				}
			}
		}
		error?.Throw();
	}

	public static IAsyncEnumerable<TSource> Concat<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(this IAsyncEnumerable<IAsyncEnumerable<TSource>> sources)
	{
		if (sources == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("sources");
		}
		return Core(sources, default(CancellationToken));
		[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })]
		static async IAsyncEnumerable<TSource> Core([_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 0 })] IAsyncEnumerable<IAsyncEnumerable<TSource>> source, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			await foreach (IAsyncEnumerable<TSource> item in source.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				await foreach (TSource item2 in item.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
				{
					yield return item2;
				}
			}
		}
	}

	public static IAsyncEnumerable<TSource> Concat<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(this IEnumerable<IAsyncEnumerable<TSource>> sources)
	{
		if (sources == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("sources");
		}
		return Core(sources, default(CancellationToken));
		[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })]
		static async IAsyncEnumerable<TSource> Core([_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 0 })] IEnumerable<IAsyncEnumerable<TSource>> enumerable, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			foreach (IAsyncEnumerable<TSource> item in enumerable)
			{
				await foreach (TSource item2 in item.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
				{
					yield return item2;
				}
			}
		}
	}

	public static IAsyncEnumerable<TSource> Concat<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(params IAsyncEnumerable<TSource>[] sources)
	{
		if (sources == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("sources");
		}
		return Core(sources, default(CancellationToken));
		[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })]
		static async IAsyncEnumerable<TSource> Core([_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 0 })] IAsyncEnumerable<TSource>[] array, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			foreach (IAsyncEnumerable<TSource> source in array)
			{
				await foreach (TSource item in source.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
				{
					yield return item;
				}
			}
		}
	}

	public static IAsyncEnumerable<TSource> Defer<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(Func<IAsyncEnumerable<TSource>> factory)
	{
		if (factory == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("factory");
		}
		return Core(factory, default(CancellationToken));
		[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })]
		static async IAsyncEnumerable<TSource> Core([_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 0 })] Func<IAsyncEnumerable<TSource>> func, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			await foreach (TSource item in func().WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				yield return item;
			}
		}
	}

	public static IAsyncEnumerable<TSource> Defer<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(Func<Task<IAsyncEnumerable<TSource>>> factory)
	{
		if (factory == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("factory");
		}
		return Core(factory, default(CancellationToken));
		[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })]
		static async IAsyncEnumerable<TSource> Core([_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 1, 0 })] Func<Task<IAsyncEnumerable<TSource>>> func, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			await foreach (TSource item in (await func().ConfigureAwait(continueOnCapturedContext: false)).WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				yield return item;
			}
		}
	}

	public static IAsyncEnumerable<TSource> Defer<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(Func<CancellationToken, Task<IAsyncEnumerable<TSource>>> factory)
	{
		if (factory == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("factory");
		}
		return Core(factory, default(CancellationToken));
		[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })]
		static async IAsyncEnumerable<TSource> Core([_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 1, 0 })] Func<CancellationToken, Task<IAsyncEnumerable<TSource>>> func, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			await foreach (TSource item in (await func(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				yield return item;
			}
		}
	}

	public static IAsyncEnumerable<TSource> Distinct<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector)
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("keySelector");
		}
		return DistinctCore(source, keySelector, null);
	}

	public static IAsyncEnumerable<TSource> Distinct<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<TKey> comparer)
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("keySelector");
		}
		return DistinctCore(source, keySelector, comparer);
	}

	public static IAsyncEnumerable<TSource> Distinct<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TKey>(this IAsyncEnumerable<TSource> source, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 0, 1 })] Func<TSource, ValueTask<TKey>> keySelector)
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("keySelector");
		}
		return DistinctCore(source, keySelector, (IEqualityComparer<TKey>)null);
	}

	public static IAsyncEnumerable<TSource> Distinct<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TKey>(this IAsyncEnumerable<TSource> source, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 0, 1 })] Func<TSource, CancellationToken, ValueTask<TKey>> keySelector)
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("keySelector");
		}
		return DistinctCore(source, keySelector, null);
	}

	public static IAsyncEnumerable<TSource> Distinct<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TKey>(this IAsyncEnumerable<TSource> source, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 0, 1 })] Func<TSource, ValueTask<TKey>> keySelector, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<TKey> comparer)
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("keySelector");
		}
		return DistinctCore(source, keySelector, comparer);
	}

	public static IAsyncEnumerable<TSource> Distinct<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TKey>(this IAsyncEnumerable<TSource> source, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 0, 1 })] Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<TKey> comparer)
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("keySelector");
		}
		return DistinctCore(source, keySelector, comparer);
	}

	private static IAsyncEnumerable<TSource> DistinctCore<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TKey>(IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<TKey> comparer)
	{
		return new DistinctAsyncIterator<TSource, TKey>(source, keySelector, comparer);
	}

	private static IAsyncEnumerable<TSource> DistinctCore<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TKey>(IAsyncEnumerable<TSource> source, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 0, 1 })] Func<TSource, ValueTask<TKey>> keySelector, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<TKey> comparer)
	{
		return new DistinctAsyncIteratorWithTask<TSource, TKey>(source, keySelector, comparer);
	}

	private static IAsyncEnumerable<TSource> DistinctCore<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TKey>(IAsyncEnumerable<TSource> source, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 0, 1 })] Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<TKey> comparer)
	{
		return new DistinctAsyncIteratorWithTaskAndCancellation<TSource, TKey>(source, keySelector, comparer);
	}

	public static IAsyncEnumerable<TSource> DistinctUntilChanged<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(this IAsyncEnumerable<TSource> source)
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		return DistinctUntilChangedCore(source, null);
	}

	public static IAsyncEnumerable<TSource> DistinctUntilChanged<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(this IAsyncEnumerable<TSource> source, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<TSource> comparer)
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		return DistinctUntilChangedCore(source, comparer);
	}

	public static IAsyncEnumerable<TSource> DistinctUntilChanged<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector)
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("keySelector");
		}
		return DistinctUntilChangedCore(source, keySelector, null);
	}

	public static IAsyncEnumerable<TSource> DistinctUntilChanged<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<TKey> comparer)
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("keySelector");
		}
		return DistinctUntilChangedCore(source, keySelector, comparer);
	}

	public static IAsyncEnumerable<TSource> DistinctUntilChanged<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TKey>(this IAsyncEnumerable<TSource> source, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 0, 1 })] Func<TSource, ValueTask<TKey>> keySelector)
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("keySelector");
		}
		return DistinctUntilChangedCore(source, keySelector, (IEqualityComparer<TKey>)null);
	}

	public static IAsyncEnumerable<TSource> DistinctUntilChanged<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TKey>(this IAsyncEnumerable<TSource> source, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 0, 1 })] Func<TSource, CancellationToken, ValueTask<TKey>> keySelector)
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("keySelector");
		}
		return DistinctUntilChangedCore(source, keySelector, null);
	}

	public static IAsyncEnumerable<TSource> DistinctUntilChanged<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TKey>(this IAsyncEnumerable<TSource> source, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 0, 1 })] Func<TSource, ValueTask<TKey>> keySelector, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<TKey> comparer)
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("keySelector");
		}
		return DistinctUntilChangedCore(source, keySelector, comparer);
	}

	public static IAsyncEnumerable<TSource> DistinctUntilChanged<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TKey>(this IAsyncEnumerable<TSource> source, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 0, 1 })] Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, IEqualityComparer<TKey> comparer)
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("keySelector");
		}
		return DistinctUntilChangedCore(source, keySelector, comparer);
	}

	private static IAsyncEnumerable<TSource> DistinctUntilChangedCore<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(IAsyncEnumerable<TSource> source, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<TSource> comparer)
	{
		if (comparer == null)
		{
			comparer = EqualityComparer<TSource>.Default;
		}
		return Core(source, comparer, default(CancellationToken));
		[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })]
		static async IAsyncEnumerable<TSource> Core([_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })] IAsyncEnumerable<TSource> enumerable, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })] IEqualityComparer<TSource> equalityComparer, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = _003Cc67a3c72_002Dc19d_002D43dc_002D869b_002Da94521f87115_003EAsyncEnumerableExt.GetConfiguredAsyncEnumerator(enumerable, cancellationToken, continueOnCapturedContext: false);
			try
			{
				if (await e.MoveNextAsync())
				{
					TSource latest = e.Current;
					yield return latest;
					while (await e.MoveNextAsync())
					{
						TSource current = e.Current;
						if (!equalityComparer.Equals(latest, current))
						{
							latest = current;
							yield return latest;
						}
					}
					yield break;
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
		}
	}

	private static IAsyncEnumerable<TSource> DistinctUntilChangedCore<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TKey>(IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<TKey> comparer)
	{
		if (comparer == null)
		{
			comparer = EqualityComparer<TKey>.Default;
		}
		return Core(source, keySelector, comparer, default(CancellationToken));
		[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })]
		static async IAsyncEnumerable<TSource> Core([_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })] IAsyncEnumerable<TSource> enumerable, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0, 0 })] Func<TSource, TKey> func, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 2, 0 })] IEqualityComparer<TKey> equalityComparer, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = _003Cc67a3c72_002Dc19d_002D43dc_002D869b_002Da94521f87115_003EAsyncEnumerableExt.GetConfiguredAsyncEnumerator(enumerable, cancellationToken, continueOnCapturedContext: false);
			try
			{
				if (await e.MoveNextAsync())
				{
					TSource current = e.Current;
					TKey latestKey = func(current);
					yield return current;
					while (await e.MoveNextAsync())
					{
						current = e.Current;
						TKey val = func(current);
						if (!equalityComparer.Equals(latestKey, val))
						{
							latestKey = val;
							yield return current;
						}
					}
					yield break;
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
		}
	}

	private static IAsyncEnumerable<TSource> DistinctUntilChangedCore<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TKey>(IAsyncEnumerable<TSource> source, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 0, 1 })] Func<TSource, ValueTask<TKey>> keySelector, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<TKey> comparer)
	{
		if (comparer == null)
		{
			comparer = EqualityComparer<TKey>.Default;
		}
		return Core(source, keySelector, comparer, default(CancellationToken));
		[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })]
		static async IAsyncEnumerable<TSource> Core([_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })] IAsyncEnumerable<TSource> enumerable, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0, 0, 0 })] Func<TSource, ValueTask<TKey>> func, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 2, 0 })] IEqualityComparer<TKey> equalityComparer, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = _003Cc67a3c72_002Dc19d_002D43dc_002D869b_002Da94521f87115_003EAsyncEnumerableExt.GetConfiguredAsyncEnumerator(enumerable, cancellationToken, continueOnCapturedContext: false);
			try
			{
				if (await e.MoveNextAsync())
				{
					TSource item = e.Current;
					TKey latestKey = await func(item).ConfigureAwait(continueOnCapturedContext: false);
					yield return item;
					while (await e.MoveNextAsync())
					{
						item = e.Current;
						TKey val = await func(item).ConfigureAwait(continueOnCapturedContext: false);
						if (!equalityComparer.Equals(latestKey, val))
						{
							latestKey = val;
							yield return item;
						}
					}
					yield break;
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
		}
	}

	private static IAsyncEnumerable<TSource> DistinctUntilChangedCore<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TKey>(IAsyncEnumerable<TSource> source, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 0, 1 })] Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<TKey> comparer)
	{
		if (comparer == null)
		{
			comparer = EqualityComparer<TKey>.Default;
		}
		return Core(source, keySelector, comparer, default(CancellationToken));
		[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })]
		static async IAsyncEnumerable<TSource> Core([_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })] IAsyncEnumerable<TSource> enumerable, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0, 0, 0 })] Func<TSource, CancellationToken, ValueTask<TKey>> func, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 2, 0 })] IEqualityComparer<TKey> equalityComparer, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = _003Cc67a3c72_002Dc19d_002D43dc_002D869b_002Da94521f87115_003EAsyncEnumerableExt.GetConfiguredAsyncEnumerator(enumerable, cancellationToken, continueOnCapturedContext: false);
			try
			{
				if (await e.MoveNextAsync())
				{
					TSource item = e.Current;
					TKey latestKey = await func(item, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					yield return item;
					while (await e.MoveNextAsync())
					{
						item = e.Current;
						TKey val = await func(item, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
						if (!equalityComparer.Equals(latestKey, val))
						{
							latestKey = val;
							yield return item;
						}
					}
					yield break;
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
		}
	}

	public static IAsyncEnumerable<TSource> Do<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(this IAsyncEnumerable<TSource> source, Action<TSource> onNext)
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (onNext == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("onNext");
		}
		return DoCore(source, onNext, null, null);
	}

	public static IAsyncEnumerable<TSource> Do<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(this IAsyncEnumerable<TSource> source, Action<TSource> onNext, Action onCompleted)
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (onNext == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("onNext");
		}
		if (onCompleted == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("onCompleted");
		}
		return DoCore(source, onNext, null, onCompleted);
	}

	public static IAsyncEnumerable<TSource> Do<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(this IAsyncEnumerable<TSource> source, Action<TSource> onNext, Action<Exception> onError)
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (onNext == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("onNext");
		}
		if (onError == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("onError");
		}
		return DoCore(source, onNext, onError, null);
	}

	public static IAsyncEnumerable<TSource> Do<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(this IAsyncEnumerable<TSource> source, Action<TSource> onNext, Action<Exception> onError, Action onCompleted)
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (onNext == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("onNext");
		}
		if (onError == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("onError");
		}
		if (onCompleted == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("onCompleted");
		}
		return DoCore(source, onNext, onError, onCompleted);
	}

	public static IAsyncEnumerable<TSource> Do<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, Task> onNext)
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (onNext == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("onNext");
		}
		return DoCore(source, onNext, null, null);
	}

	public static IAsyncEnumerable<TSource> Do<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, Task> onNext, Func<Task> onCompleted)
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (onNext == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("onNext");
		}
		if (onCompleted == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("onCompleted");
		}
		return DoCore(source, onNext, null, onCompleted);
	}

	public static IAsyncEnumerable<TSource> Do<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, Task> onNext, Func<Exception, Task> onError)
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (onNext == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("onNext");
		}
		if (onError == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("onError");
		}
		return DoCore(source, onNext, onError, null);
	}

	public static IAsyncEnumerable<TSource> Do<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, Task> onNext, Func<Exception, Task> onError, Func<Task> onCompleted)
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (onNext == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("onNext");
		}
		if (onError == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("onError");
		}
		if (onCompleted == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("onCompleted");
		}
		return DoCore(source, onNext, onError, onCompleted);
	}

	public static IAsyncEnumerable<TSource> Do<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, Task> onNext)
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (onNext == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("onNext");
		}
		return DoCore(source, onNext, null, null);
	}

	public static IAsyncEnumerable<TSource> Do<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, Task> onNext, Func<CancellationToken, Task> onCompleted)
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (onNext == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("onNext");
		}
		if (onCompleted == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("onCompleted");
		}
		return DoCore(source, onNext, null, onCompleted);
	}

	public static IAsyncEnumerable<TSource> Do<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, Task> onNext, Func<Exception, CancellationToken, Task> onError)
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (onNext == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("onNext");
		}
		if (onError == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("onError");
		}
		return DoCore(source, onNext, onError, null);
	}

	public static IAsyncEnumerable<TSource> Do<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, Task> onNext, Func<Exception, CancellationToken, Task> onError, Func<CancellationToken, Task> onCompleted)
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (onNext == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("onNext");
		}
		if (onError == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("onError");
		}
		if (onCompleted == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("onCompleted");
		}
		return DoCore(source, onNext, onError, onCompleted);
	}

	public static IAsyncEnumerable<TSource> Do<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(this IAsyncEnumerable<TSource> source, IObserver<TSource> observer)
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (observer == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("observer");
		}
		return DoCore(source, (Action<TSource>)observer.OnNext, (Action<Exception>)observer.OnError, (Action)observer.OnCompleted, default(CancellationToken));
	}

	private static async IAsyncEnumerable<TSource> DoCore<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(IAsyncEnumerable<TSource> source, Action<TSource> onNext, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 2, 1 })] Action<Exception> onError, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] Action onCompleted, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
	{
		ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = _003Cc67a3c72_002Dc19d_002D43dc_002D869b_002Da94521f87115_003EAsyncEnumerableExt.GetConfiguredAsyncEnumerator(source, cancellationToken, continueOnCapturedContext: false);
		try
		{
			while (true)
			{
				TSource current;
				try
				{
					if (!(await e.MoveNextAsync()))
					{
						break;
					}
					current = e.Current;
					onNext(current);
					goto IL_0127;
				}
				catch (OperationCanceledException)
				{
					throw;
				}
				catch (Exception obj) when (onError != null)
				{
					onError(obj);
					throw;
				}
				IL_0127:
				yield return current;
			}
			onCompleted?.Invoke();
		}
		finally
		{
			IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
			if (asyncDisposable != null)
			{
				await asyncDisposable.DisposeAsync();
			}
		}
	}

	private static async IAsyncEnumerable<TSource> DoCore<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(IAsyncEnumerable<TSource> source, Func<TSource, Task> onNext, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 2, 1, 1 })] Func<Exception, Task> onError, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 2, 1 })] Func<Task> onCompleted, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
	{
		ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = _003Cc67a3c72_002Dc19d_002D43dc_002D869b_002Da94521f87115_003EAsyncEnumerableExt.GetConfiguredAsyncEnumerator(source, cancellationToken, continueOnCapturedContext: false);
		try
		{
			while (true)
			{
				TSource item;
				try
				{
					if (!(await e.MoveNextAsync()))
					{
						break;
					}
					item = e.Current;
					await onNext(item).ConfigureAwait(continueOnCapturedContext: false);
					goto IL_0289;
				}
				catch (OperationCanceledException)
				{
					throw;
				}
				catch (Exception arg) when (onError != null)
				{
					await onError(arg).ConfigureAwait(continueOnCapturedContext: false);
					throw;
				}
				IL_0289:
				yield return item;
			}
			if (onCompleted != null)
			{
				await onCompleted().ConfigureAwait(continueOnCapturedContext: false);
			}
		}
		finally
		{
			IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
			if (asyncDisposable != null)
			{
				await asyncDisposable.DisposeAsync();
			}
		}
	}

	private static async IAsyncEnumerable<TSource> DoCore<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, Task> onNext, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 2, 1, 1 })] Func<Exception, CancellationToken, Task> onError, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 2, 1 })] Func<CancellationToken, Task> onCompleted, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
	{
		ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = _003Cc67a3c72_002Dc19d_002D43dc_002D869b_002Da94521f87115_003EAsyncEnumerableExt.GetConfiguredAsyncEnumerator(source, cancellationToken, continueOnCapturedContext: false);
		try
		{
			while (true)
			{
				TSource item;
				try
				{
					if (!(await e.MoveNextAsync()))
					{
						break;
					}
					item = e.Current;
					await onNext(item, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					goto IL_0295;
				}
				catch (OperationCanceledException)
				{
					throw;
				}
				catch (Exception arg) when (onError != null)
				{
					await onError(arg, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					throw;
				}
				IL_0295:
				yield return item;
			}
			if (onCompleted != null)
			{
				await onCompleted(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
		finally
		{
			IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
			if (asyncDisposable != null)
			{
				await asyncDisposable.DisposeAsync();
			}
		}
	}

	public static IAsyncEnumerable<TSource> Expand<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, IAsyncEnumerable<TSource>> selector)
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("selector");
		}
		return Core(source, selector, default(CancellationToken));
		[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })]
		static async IAsyncEnumerable<TSource> Core([_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })] IAsyncEnumerable<TSource> item, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0, 1, 0 })] Func<TSource, IAsyncEnumerable<TSource>> func, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			Queue<IAsyncEnumerable<TSource>> queue = new Queue<IAsyncEnumerable<TSource>>();
			queue.Enqueue(item);
			while (queue.Count > 0)
			{
				await foreach (TSource item in queue.Dequeue().WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
				{
					queue.Enqueue(func(item));
					yield return item;
				}
			}
		}
	}

	public static IAsyncEnumerable<TSource> Expand<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(this IAsyncEnumerable<TSource> source, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 0, 1, 1 })] Func<TSource, ValueTask<IAsyncEnumerable<TSource>>> selector)
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("selector");
		}
		return Core(source, selector, default(CancellationToken));
		[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })]
		static async IAsyncEnumerable<TSource> Core([_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })] IAsyncEnumerable<TSource> item, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0, 0, 1, 0 })] Func<TSource, ValueTask<IAsyncEnumerable<TSource>>> func, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			Queue<IAsyncEnumerable<TSource>> queue = new Queue<IAsyncEnumerable<TSource>>();
			queue.Enqueue(item);
			while (queue.Count > 0)
			{
				await foreach (TSource item2 in queue.Dequeue().WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
				{
					Queue<IAsyncEnumerable<TSource>> queue2 = queue;
					queue2.Enqueue(await func(item2).ConfigureAwait(continueOnCapturedContext: false));
					yield return item2;
				}
			}
		}
	}

	public static IAsyncEnumerable<TSource> Expand<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(this IAsyncEnumerable<TSource> source, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 0, 1, 1 })] Func<TSource, CancellationToken, ValueTask<IAsyncEnumerable<TSource>>> selector)
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("selector");
		}
		return Core(source, selector, default(CancellationToken));
		[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })]
		static async IAsyncEnumerable<TSource> Core([_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })] IAsyncEnumerable<TSource> item, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0, 0, 1, 0 })] Func<TSource, CancellationToken, ValueTask<IAsyncEnumerable<TSource>>> func, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			Queue<IAsyncEnumerable<TSource>> queue = new Queue<IAsyncEnumerable<TSource>>();
			queue.Enqueue(item);
			while (queue.Count > 0)
			{
				await foreach (TSource item2 in queue.Dequeue().WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
				{
					Queue<IAsyncEnumerable<TSource>> queue2 = queue;
					queue2.Enqueue(await func(item2, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
					yield return item2;
				}
			}
		}
	}

	public static IAsyncEnumerable<TSource> Finally<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(this IAsyncEnumerable<TSource> source, Action finallyAction)
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (finallyAction == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("finallyAction");
		}
		return Core(source, finallyAction, default(CancellationToken));
		[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })]
		static async IAsyncEnumerable<TSource> Core([_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })] IAsyncEnumerable<TSource> source2, Action action, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			try
			{
				await foreach (TSource item in source2.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
				{
					yield return item;
				}
			}
			finally
			{
				action();
			}
		}
	}

	public static IAsyncEnumerable<TSource> Finally<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(this IAsyncEnumerable<TSource> source, Func<Task> finallyAction)
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (finallyAction == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("finallyAction");
		}
		return Core(source, finallyAction, default(CancellationToken));
		[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })]
		static async IAsyncEnumerable<TSource> Core([_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })] IAsyncEnumerable<TSource> source2, Func<Task> func, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			try
			{
				await foreach (TSource item in source2.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
				{
					yield return item;
				}
			}
			finally
			{
				await func().ConfigureAwait(continueOnCapturedContext: false);
			}
		}
	}

	public static IAsyncEnumerable<TResult> Generate<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TState, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TResult>(TState initialState, Func<TState, bool> condition, Func<TState, TState> iterate, Func<TState, TResult> resultSelector)
	{
		if (condition == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("condition");
		}
		if (iterate == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("iterate");
		}
		if (resultSelector == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("resultSelector");
		}
		return Core(initialState, condition, iterate, resultSelector, default(CancellationToken));
		[_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(0)]
		[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })]
		static async IAsyncEnumerable<TResult> Core(TState val, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })] Func<TState, bool> func, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0, 0 })] Func<TState, TState> func3, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0, 0 })] Func<TState, TResult> func2, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			TState state = val;
			while (func(state))
			{
				yield return func2(state);
				state = func3(state);
			}
		}
	}

	public static IAsyncEnumerable<TSource> IgnoreElements<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(this IAsyncEnumerable<TSource> source)
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		return Core(source, default(CancellationToken));
		[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })]
		static async IAsyncEnumerable<TSource> Core([_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })] IAsyncEnumerable<TSource> source2, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			await foreach (TSource item in source2.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				_ = item;
			}
			yield break;
		}
	}

	[_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(0)]
	public static ValueTask<bool> IsEmptyAsync<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>([_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(1)] this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		return Core(source, cancellationToken);
		[_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(0)]
		static async ValueTask<bool> Core([_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })] IAsyncEnumerable<TSource> enumerable, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = _003Cc67a3c72_002Dc19d_002D43dc_002D869b_002Da94521f87115_003EAsyncEnumerableExt.GetConfiguredAsyncEnumerator(enumerable, cancellationToken2, continueOnCapturedContext: false);
			bool result;
			try
			{
				result = !(await e.MoveNextAsync());
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return result;
		}
	}

	[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 0, 1 })]
	public static ValueTask<TSource> MaxAsync<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(this IAsyncEnumerable<TSource> source, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 2, 1 })] IComparer<TSource> comparer, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		return Core(source, comparer, cancellationToken);
		[_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(0)]
		static async ValueTask<TSource> Core([_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })] IAsyncEnumerable<TSource> enumerable, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 2, 0 })] IComparer<TSource> comparer2, CancellationToken cancellationToken2)
		{
			if (comparer2 == null)
			{
				comparer2 = Comparer<TSource>.Default;
			}
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = _003Cc67a3c72_002Dc19d_002D43dc_002D869b_002Da94521f87115_003EAsyncEnumerableExt.GetConfiguredAsyncEnumerator(enumerable, cancellationToken2, continueOnCapturedContext: false);
			TSource result;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.NoElements();
				}
				TSource max = e.Current;
				while (await e.MoveNextAsync())
				{
					TSource current = e.Current;
					if (comparer2.Compare(current, max) > 0)
					{
						max = current;
					}
				}
				result = max;
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return result;
		}
	}

	[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 0, 1, 1 })]
	public static ValueTask<IList<TSource>> MaxByAsync<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("keySelector");
		}
		return MaxByCore(source, keySelector, null, cancellationToken);
	}

	[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 0, 1, 1 })]
	public static ValueTask<IList<TSource>> MaxByAsync<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 2, 1 })] IComparer<TKey> comparer, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("keySelector");
		}
		return MaxByCore(source, keySelector, comparer, cancellationToken);
	}

	[_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(2)]
	[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 0, 1, 1 })]
	public static ValueTask<IList<TSource>> MaxByAsync<TSource, TKey>([_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(1)] this IAsyncEnumerable<TSource> source, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 0, 1 })] Func<TSource, ValueTask<TKey>> keySelector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("keySelector");
		}
		return MaxByCore(source, keySelector, (IComparer<TKey>)null, cancellationToken);
	}

	[_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(2)]
	[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 0, 1, 1 })]
	public static ValueTask<IList<TSource>> MaxByAsync<TSource, TKey>([_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(1)] this IAsyncEnumerable<TSource> source, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 0, 1 })] Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("keySelector");
		}
		return MaxByCore(source, keySelector, null, cancellationToken);
	}

	[_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(2)]
	[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 0, 1, 1 })]
	public static ValueTask<IList<TSource>> MaxByAsync<TSource, TKey>([_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(1)] this IAsyncEnumerable<TSource> source, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 0, 1 })] Func<TSource, ValueTask<TKey>> keySelector, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 2, 1 })] IComparer<TKey> comparer, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("keySelector");
		}
		return MaxByCore(source, keySelector, comparer, cancellationToken);
	}

	[_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(2)]
	[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 0, 1, 1 })]
	public static ValueTask<IList<TSource>> MaxByAsync<TSource, TKey>([_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(1)] this IAsyncEnumerable<TSource> source, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 0, 1 })] Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 2, 1 })] IComparer<TKey> comparer, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("keySelector");
		}
		return MaxByCore(source, keySelector, comparer, cancellationToken);
	}

	[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 0, 1, 1 })]
	private static ValueTask<IList<TSource>> MaxByCore<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TKey>(IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 2, 1 })] IComparer<TKey> comparer, CancellationToken cancellationToken)
	{
		if (comparer == null)
		{
			comparer = Comparer<TKey>.Default;
		}
		return ExtremaBy(source, keySelector, [_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(0)] (TKey key, TKey minValue) => comparer.Compare(key, minValue), cancellationToken);
	}

	[_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(2)]
	[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 0, 1, 1 })]
	private static ValueTask<IList<TSource>> MaxByCore<TSource, TKey>([_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(1)] IAsyncEnumerable<TSource> source, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 0, 1 })] Func<TSource, ValueTask<TKey>> keySelector, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 2, 1 })] IComparer<TKey> comparer, CancellationToken cancellationToken)
	{
		if (comparer == null)
		{
			comparer = Comparer<TKey>.Default;
		}
		return ExtremaBy(source, keySelector, [_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(0)] (TKey key, TKey minValue) => comparer.Compare(key, minValue), cancellationToken);
	}

	[_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(2)]
	[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 0, 1, 1 })]
	private static ValueTask<IList<TSource>> MaxByCore<TSource, TKey>([_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(1)] IAsyncEnumerable<TSource> source, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 0, 1 })] Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 2, 1 })] IComparer<TKey> comparer, CancellationToken cancellationToken)
	{
		if (comparer == null)
		{
			comparer = Comparer<TKey>.Default;
		}
		return ExtremaBy(source, keySelector, [_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(0)] (TKey key, TKey minValue) => comparer.Compare(key, minValue), cancellationToken);
	}

	public static IAsyncEnumerable<TSource> Merge<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(params IAsyncEnumerable<TSource>[] sources)
	{
		if (sources == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("sources");
		}
		return Core(sources, default(CancellationToken));
		[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })]
		static async IAsyncEnumerable<TSource> Core([_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 0 })] IAsyncEnumerable<TSource>[] array, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			int count = array.Length;
			IAsyncEnumerator<TSource>[] enumerators = new IAsyncEnumerator<TSource>[count];
			Task<bool>[] moveNextTasks = new Task<bool>[count];
			try
			{
				for (int i = 0; i < count; i++)
				{
					moveNextTasks[i] = (enumerators[i] = array[i].GetAsyncEnumerator(cancellationToken)).MoveNextAsync().AsTask();
				}
				int active = count;
				while (active > 0)
				{
					Task<bool> task = await Task.WhenAny(moveNextTasks).ConfigureAwait(continueOnCapturedContext: false);
					int index = Array.IndexOf(moveNextTasks, task);
					IAsyncEnumerator<TSource> enumerator = enumerators[index];
					if (!(await task.ConfigureAwait(continueOnCapturedContext: false)))
					{
						moveNextTasks[index] = _003Ce4d7e9f8_002Ddb62_002D4cc3_002D9339_002D59957d96537a_003ETaskExt.Never;
						enumerators[index] = null;
						await enumerator.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
						active--;
					}
					else
					{
						TSource current = enumerator.Current;
						moveNextTasks[index] = enumerator.MoveNextAsync().AsTask();
						yield return current;
					}
				}
			}
			finally
			{
				List<Exception> errors = null;
				for (int active = count - 1; active >= 0; active--)
				{
					Task<bool> task2 = moveNextTasks[active];
					IAsyncEnumerator<TSource> enumerator = enumerators[active];
					try
					{
						try
						{
							if (task2 != null && task2 != _003Ce4d7e9f8_002Ddb62_002D4cc3_002D9339_002D59957d96537a_003ETaskExt.Never)
							{
								await task2.ConfigureAwait(continueOnCapturedContext: false);
							}
						}
						finally
						{
							if (enumerator != null)
							{
								await enumerator.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
							}
						}
					}
					catch (Exception item)
					{
						if (errors == null)
						{
							errors = new List<Exception>();
						}
						errors.Add(item);
					}
				}
				if (errors != null)
				{
					throw new AggregateException(errors);
				}
			}
		}
	}

	public static IAsyncEnumerable<TSource> Merge<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(this IEnumerable<IAsyncEnumerable<TSource>> sources)
	{
		if (sources == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("sources");
		}
		return sources.ToAsyncEnumerable().SelectMany([_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(0)] [return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })] (IAsyncEnumerable<TSource> source) => source);
	}

	public static IAsyncEnumerable<TSource> Merge<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(this IAsyncEnumerable<IAsyncEnumerable<TSource>> sources)
	{
		if (sources == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("sources");
		}
		return sources.SelectMany([_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(0)] [return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })] (IAsyncEnumerable<TSource> source) => source);
	}

	[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 0, 1 })]
	public static ValueTask<TSource> MinAsync<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(this IAsyncEnumerable<TSource> source, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 2, 1 })] IComparer<TSource> comparer, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		return Core(source, comparer, cancellationToken);
		[_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(0)]
		static async ValueTask<TSource> Core([_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })] IAsyncEnumerable<TSource> enumerable, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 2, 0 })] IComparer<TSource> comparer2, CancellationToken cancellationToken2)
		{
			if (comparer2 == null)
			{
				comparer2 = Comparer<TSource>.Default;
			}
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = _003Cc67a3c72_002Dc19d_002D43dc_002D869b_002Da94521f87115_003EAsyncEnumerableExt.GetConfiguredAsyncEnumerator(enumerable, cancellationToken2, continueOnCapturedContext: false);
			TSource result;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.NoElements();
				}
				TSource min = e.Current;
				while (await e.MoveNextAsync())
				{
					TSource current = e.Current;
					if (comparer2.Compare(current, min) < 0)
					{
						min = current;
					}
				}
				result = min;
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return result;
		}
	}

	[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 0, 1, 1 })]
	public static ValueTask<IList<TSource>> MinByAsync<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("keySelector");
		}
		return MinByCore(source, keySelector, null, cancellationToken);
	}

	[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 0, 1, 1 })]
	public static ValueTask<IList<TSource>> MinByAsync<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 2, 1 })] IComparer<TKey> comparer, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("keySelector");
		}
		return MinByCore(source, keySelector, comparer, cancellationToken);
	}

	[_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(2)]
	[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 0, 1, 1 })]
	public static ValueTask<IList<TSource>> MinByAsync<TSource, TKey>([_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(1)] this IAsyncEnumerable<TSource> source, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 0, 1 })] Func<TSource, ValueTask<TKey>> keySelector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("keySelector");
		}
		return MinByCore(source, keySelector, (IComparer<TKey>)null, cancellationToken);
	}

	[_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(2)]
	[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 0, 1, 1 })]
	public static ValueTask<IList<TSource>> MinByAsync<TSource, TKey>([_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(1)] this IAsyncEnumerable<TSource> source, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 0, 1 })] Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("keySelector");
		}
		return MinByCore(source, keySelector, null, cancellationToken);
	}

	[_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(2)]
	[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 0, 1, 1 })]
	public static ValueTask<IList<TSource>> MinByAsync<TSource, TKey>([_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(1)] this IAsyncEnumerable<TSource> source, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 0, 1 })] Func<TSource, ValueTask<TKey>> keySelector, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 2, 1 })] IComparer<TKey> comparer, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("keySelector");
		}
		return MinByCore(source, keySelector, comparer, cancellationToken);
	}

	[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 0, 1, 1 })]
	public static ValueTask<IList<TSource>> MinByAsync<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TKey>(this IAsyncEnumerable<TSource> source, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 0, 1 })] Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, IComparer<TKey> comparer, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("keySelector");
		}
		return MinByCore(source, keySelector, comparer, cancellationToken);
	}

	[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 0, 1, 1 })]
	private static ValueTask<IList<TSource>> MinByCore<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TKey>(IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 2, 1 })] IComparer<TKey> comparer, CancellationToken cancellationToken)
	{
		if (comparer == null)
		{
			comparer = Comparer<TKey>.Default;
		}
		return ExtremaBy(source, keySelector, [_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(0)] (TKey key, TKey minValue) => -comparer.Compare(key, minValue), cancellationToken);
	}

	[_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(2)]
	[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 0, 1, 1 })]
	private static ValueTask<IList<TSource>> MinByCore<TSource, TKey>([_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(1)] IAsyncEnumerable<TSource> source, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 0, 1 })] Func<TSource, ValueTask<TKey>> keySelector, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 2, 1 })] IComparer<TKey> comparer, CancellationToken cancellationToken)
	{
		if (comparer == null)
		{
			comparer = Comparer<TKey>.Default;
		}
		return ExtremaBy(source, keySelector, [_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(0)] (TKey key, TKey minValue) => -comparer.Compare(key, minValue), cancellationToken);
	}

	[_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(2)]
	[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 0, 1, 1 })]
	private static ValueTask<IList<TSource>> MinByCore<TSource, TKey>([_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(1)] IAsyncEnumerable<TSource> source, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 0, 1 })] Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 2, 1 })] IComparer<TKey> comparer, CancellationToken cancellationToken)
	{
		if (comparer == null)
		{
			comparer = Comparer<TKey>.Default;
		}
		return ExtremaBy(source, keySelector, [_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(0)] (TKey key, TKey minValue) => -comparer.Compare(key, minValue), cancellationToken);
	}

	[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 0, 1, 1 })]
	private static async ValueTask<IList<TSource>> ExtremaBy<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TKey>(IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, TKey, int> compare, CancellationToken cancellationToken)
	{
		List<TSource> result = new List<TSource>();
		ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = _003Cc67a3c72_002Dc19d_002D43dc_002D869b_002Da94521f87115_003EAsyncEnumerableExt.GetConfiguredAsyncEnumerator(source, cancellationToken, continueOnCapturedContext: false);
		try
		{
			if (!(await e.MoveNextAsync()))
			{
				throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.NoElements();
			}
			TSource current = e.Current;
			TKey resKey = keySelector(current);
			result.Add(current);
			while (await e.MoveNextAsync())
			{
				TSource current2 = e.Current;
				TKey val = keySelector(current2);
				int num = compare(val, resKey);
				if (num == 0)
				{
					result.Add(current2);
				}
				else if (num > 0)
				{
					result = new List<TSource> { current2 };
					resKey = val;
				}
			}
		}
		finally
		{
			IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
			if (asyncDisposable != null)
			{
				await asyncDisposable.DisposeAsync();
			}
		}
		return result;
	}

	[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 0, 1, 1 })]
	private static async ValueTask<IList<TSource>> ExtremaBy<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TKey>(IAsyncEnumerable<TSource> source, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 0, 1 })] Func<TSource, ValueTask<TKey>> keySelector, Func<TKey, TKey, int> compare, CancellationToken cancellationToken)
	{
		List<TSource> result = new List<TSource>();
		ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = _003Cc67a3c72_002Dc19d_002D43dc_002D869b_002Da94521f87115_003EAsyncEnumerableExt.GetConfiguredAsyncEnumerator(source, cancellationToken, continueOnCapturedContext: false);
		try
		{
			if (!(await e.MoveNextAsync()))
			{
				throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.NoElements();
			}
			TSource current = e.Current;
			TKey resKey = await keySelector(current).ConfigureAwait(continueOnCapturedContext: false);
			result.Add(current);
			while (await e.MoveNextAsync())
			{
				TSource cur = e.Current;
				TKey val = await keySelector(cur).ConfigureAwait(continueOnCapturedContext: false);
				int num = compare(val, resKey);
				if (num == 0)
				{
					result.Add(cur);
				}
				else if (num > 0)
				{
					result = new List<TSource> { cur };
					resKey = val;
				}
			}
		}
		finally
		{
			IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
			if (asyncDisposable != null)
			{
				await asyncDisposable.DisposeAsync();
			}
		}
		return result;
	}

	[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 0, 1, 1 })]
	private static async ValueTask<IList<TSource>> ExtremaBy<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TKey>(IAsyncEnumerable<TSource> source, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 0, 1 })] Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, Func<TKey, TKey, int> compare, CancellationToken cancellationToken)
	{
		List<TSource> result = new List<TSource>();
		ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = _003Cc67a3c72_002Dc19d_002D43dc_002D869b_002Da94521f87115_003EAsyncEnumerableExt.GetConfiguredAsyncEnumerator(source, cancellationToken, continueOnCapturedContext: false);
		try
		{
			if (!(await e.MoveNextAsync()))
			{
				throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.NoElements();
			}
			TSource current = e.Current;
			TKey resKey = await keySelector(current, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			result.Add(current);
			while (await e.MoveNextAsync())
			{
				TSource cur = e.Current;
				TKey val = await keySelector(cur, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				int num = compare(val, resKey);
				if (num == 0)
				{
					result.Add(cur);
				}
				else if (num > 0)
				{
					result = new List<TSource> { cur };
					resKey = val;
				}
			}
		}
		finally
		{
			IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
			if (asyncDisposable != null)
			{
				await asyncDisposable.DisposeAsync();
			}
		}
		return result;
	}

	public static IAsyncEnumerable<TValue> Never<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TValue>()
	{
		return NeverAsyncEnumerable<TValue>.Instance;
	}

	public static IAsyncEnumerable<TSource> OnErrorResumeNext<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(this IAsyncEnumerable<TSource> first, IAsyncEnumerable<TSource> second)
	{
		if (first == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("first");
		}
		if (second == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("second");
		}
		return OnErrorResumeNextCore(new IAsyncEnumerable<TSource>[2] { first, second });
	}

	public static IAsyncEnumerable<TSource> OnErrorResumeNext<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(params IAsyncEnumerable<TSource>[] sources)
	{
		if (sources == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("sources");
		}
		return OnErrorResumeNextCore(sources);
	}

	public static IAsyncEnumerable<TSource> OnErrorResumeNext<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(this IEnumerable<IAsyncEnumerable<TSource>> sources)
	{
		if (sources == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("sources");
		}
		return OnErrorResumeNextCore(sources);
	}

	private static IAsyncEnumerable<TSource> OnErrorResumeNextCore<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(IEnumerable<IAsyncEnumerable<TSource>> sources)
	{
		return new OnErrorResumeNextAsyncIterator<TSource>(sources);
	}

	public static IAsyncEnumerable<TResult> Repeat<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TResult>(TResult element)
	{
		return Core(element, default(CancellationToken));
		[_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(0)]
		[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })]
		static async IAsyncEnumerable<TResult> Core(TResult val, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			while (true)
			{
				cancellationToken.ThrowIfCancellationRequested();
				yield return val;
			}
		}
	}

	public static IAsyncEnumerable<TSource> Repeat<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(this IAsyncEnumerable<TSource> source)
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		return Core(source, default(CancellationToken));
		[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })]
		static async IAsyncEnumerable<TSource> Core([_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })] IAsyncEnumerable<TSource> source2, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			while (true)
			{
				await foreach (TSource item in source2.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
				{
					yield return item;
				}
			}
		}
	}

	public static IAsyncEnumerable<TSource> Repeat<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(this IAsyncEnumerable<TSource> source, int count)
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (count < 0)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentOutOfRange("count");
		}
		return Core(source, count, default(CancellationToken));
		[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })]
		static async IAsyncEnumerable<TSource> Core([_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })] IAsyncEnumerable<TSource> source2, int num, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			for (int i = 0; i < num; i++)
			{
				await foreach (TSource item in source2.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
				{
					yield return item;
				}
			}
		}
	}

	public static IAsyncEnumerable<TSource> Retry<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(this IAsyncEnumerable<TSource> source)
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		return ((IEnumerable<IAsyncEnumerable<TSource>>)new IAsyncEnumerable<TSource>[1] { source }).Repeat().Catch();
	}

	public static IAsyncEnumerable<TSource> Retry<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(this IAsyncEnumerable<TSource> source, int retryCount)
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (retryCount < 0)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentOutOfRange("retryCount");
		}
		return new IAsyncEnumerable<TSource>[1] { source }.Repeat(retryCount).Catch();
	}

	private static IEnumerable<TSource> Repeat<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(this IEnumerable<TSource> source)
	{
		while (true)
		{
			foreach (TSource item in source)
			{
				yield return item;
			}
		}
	}

	private static IEnumerable<TSource> Repeat<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(this IEnumerable<TSource> source, int count)
	{
		for (int i = 0; i < count; i++)
		{
			foreach (TSource item in source)
			{
				yield return item;
			}
		}
	}

	public static IAsyncEnumerable<TValue> Return<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TValue>(TValue value)
	{
		return new ReturnEnumerable<TValue>(value);
	}

	public static IAsyncEnumerable<TSource> Scan<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, TSource, TSource> accumulator)
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (accumulator == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("accumulator");
		}
		return Core(source, accumulator, default(CancellationToken));
		[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })]
		static async IAsyncEnumerable<TSource> Core([_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })] IAsyncEnumerable<TSource> enumerable, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0, 0, 0 })] Func<TSource, TSource, TSource> func, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = _003Cc67a3c72_002Dc19d_002D43dc_002D869b_002Da94521f87115_003EAsyncEnumerableExt.GetConfiguredAsyncEnumerator(enumerable, cancellationToken, continueOnCapturedContext: false);
			try
			{
				if (await e.MoveNextAsync())
				{
					TSource res = e.Current;
					while (await e.MoveNextAsync())
					{
						res = func(res, e.Current);
						yield return res;
					}
					yield break;
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
		}
	}

	public static IAsyncEnumerable<TAccumulate> Scan<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TAccumulate>(this IAsyncEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> accumulator)
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (accumulator == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("accumulator");
		}
		return Core(source, seed, accumulator, default(CancellationToken));
		[_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(0)]
		[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })]
		static async IAsyncEnumerable<TAccumulate> Core([_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })] IAsyncEnumerable<TSource> source2, TAccumulate val, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0, 0, 0 })] Func<TAccumulate, TSource, TAccumulate> func, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			TAccumulate res = val;
			await foreach (TSource item in source2.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				res = func(res, item);
				yield return res;
			}
		}
	}

	public static IAsyncEnumerable<TSource> Scan<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(this IAsyncEnumerable<TSource> source, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 1, 0, 1 })] Func<TSource, TSource, ValueTask<TSource>> accumulator)
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (accumulator == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("accumulator");
		}
		return Core(source, accumulator, default(CancellationToken));
		[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })]
		static async IAsyncEnumerable<TSource> Core([_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })] IAsyncEnumerable<TSource> enumerable, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0, 0, 0, 0 })] Func<TSource, TSource, ValueTask<TSource>> func, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = _003Cc67a3c72_002Dc19d_002D43dc_002D869b_002Da94521f87115_003EAsyncEnumerableExt.GetConfiguredAsyncEnumerator(enumerable, cancellationToken, continueOnCapturedContext: false);
			try
			{
				if (await e.MoveNextAsync())
				{
					TSource res = e.Current;
					while (await e.MoveNextAsync())
					{
						res = await func(res, e.Current).ConfigureAwait(continueOnCapturedContext: false);
						yield return res;
					}
					yield break;
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
		}
	}

	public static IAsyncEnumerable<TSource> Scan<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(this IAsyncEnumerable<TSource> source, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 1, 0, 1 })] Func<TSource, TSource, CancellationToken, ValueTask<TSource>> accumulator)
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (accumulator == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("accumulator");
		}
		return Core(source, accumulator, default(CancellationToken));
		[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })]
		static async IAsyncEnumerable<TSource> Core([_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })] IAsyncEnumerable<TSource> enumerable, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0, 0, 0, 0 })] Func<TSource, TSource, CancellationToken, ValueTask<TSource>> func, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = _003Cc67a3c72_002Dc19d_002D43dc_002D869b_002Da94521f87115_003EAsyncEnumerableExt.GetConfiguredAsyncEnumerator(enumerable, cancellationToken, continueOnCapturedContext: false);
			try
			{
				if (await e.MoveNextAsync())
				{
					TSource res = e.Current;
					while (await e.MoveNextAsync())
					{
						res = await func(res, e.Current, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
						yield return res;
					}
					yield break;
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
		}
	}

	public static IAsyncEnumerable<TAccumulate> Scan<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TAccumulate>(this IAsyncEnumerable<TSource> source, TAccumulate seed, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 1, 0, 1 })] Func<TAccumulate, TSource, ValueTask<TAccumulate>> accumulator)
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (accumulator == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("accumulator");
		}
		return Core(source, seed, accumulator, default(CancellationToken));
		[_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(0)]
		[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })]
		static async IAsyncEnumerable<TAccumulate> Core([_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })] IAsyncEnumerable<TSource> source2, TAccumulate val, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0, 0, 0, 0 })] Func<TAccumulate, TSource, ValueTask<TAccumulate>> func, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			TAccumulate res = val;
			await foreach (TSource item in source2.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				res = await func(res, item).ConfigureAwait(continueOnCapturedContext: false);
				yield return res;
			}
		}
	}

	public static IAsyncEnumerable<TAccumulate> Scan<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TAccumulate>(this IAsyncEnumerable<TSource> source, TAccumulate seed, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 1, 0, 1 })] Func<TAccumulate, TSource, CancellationToken, ValueTask<TAccumulate>> accumulator)
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (accumulator == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("accumulator");
		}
		return Core(source, seed, accumulator, default(CancellationToken));
		[_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(0)]
		[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })]
		static async IAsyncEnumerable<TAccumulate> Core([_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })] IAsyncEnumerable<TSource> source2, TAccumulate val, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0, 0, 0, 0 })] Func<TAccumulate, TSource, CancellationToken, ValueTask<TAccumulate>> func, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			TAccumulate res = val;
			await foreach (TSource item in source2.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				res = await func(res, item, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				yield return res;
			}
		}
	}

	public static IAsyncEnumerable<TOther> SelectMany<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TOther>(this IAsyncEnumerable<TSource> source, IAsyncEnumerable<TOther> other)
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (other == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("other");
		}
		return source.SelectMany([_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(0)] [return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })] (TSource _) => other);
	}

	public static IAsyncEnumerable<TSource> StartWith<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(this IAsyncEnumerable<TSource> source, params TSource[] values)
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		if (values == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("values");
		}
		return values.ToAsyncEnumerable().Concat(source);
	}

	public static IAsyncEnumerable<TValue> Throw<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TValue>(Exception exception)
	{
		if (exception == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("exception");
		}
		return new ThrowEnumerable<TValue>(new ValueTask<bool>(Task.FromException<bool>(exception)));
	}

	public static IAsyncEnumerable<TSource> Timeout<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource>(this IAsyncEnumerable<TSource> source, TimeSpan timeout)
	{
		if (source == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("source");
		}
		long num = (long)timeout.TotalMilliseconds;
		if (num < -1 || num > int.MaxValue)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentOutOfRange("timeout");
		}
		return new TimeoutAsyncIterator<TSource>(source, timeout);
	}

	public static IAsyncEnumerable<TSource> Using<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(0)] TResource>(Func<TResource> resourceFactory, Func<TResource, IAsyncEnumerable<TSource>> enumerableFactory) where TResource : IDisposable
	{
		if (resourceFactory == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("resourceFactory");
		}
		if (enumerableFactory == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("enumerableFactory");
		}
		return Core(resourceFactory, enumerableFactory, default(CancellationToken));
		[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })]
		static async IAsyncEnumerable<TSource> Core([_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })] Func<TResource> func, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0, 1, 0 })] Func<TResource, IAsyncEnumerable<TSource>> func2, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			using TResource resource = func();
			await foreach (TSource item in func2(resource).WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				yield return item;
			}
		}
	}

	public static IAsyncEnumerable<TSource> Using<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(0)] TResource>(Func<Task<TResource>> resourceFactory, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 0, 1, 1 })] Func<TResource, ValueTask<IAsyncEnumerable<TSource>>> enumerableFactory) where TResource : IDisposable
	{
		if (resourceFactory == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("resourceFactory");
		}
		if (enumerableFactory == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("enumerableFactory");
		}
		return Core(resourceFactory, enumerableFactory, default(CancellationToken));
		[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })]
		static async IAsyncEnumerable<TSource> Core([_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 0 })] Func<Task<TResource>> func, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0, 0, 1, 0 })] Func<TResource, ValueTask<IAsyncEnumerable<TSource>>> func2, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			using TResource resource = await func().ConfigureAwait(continueOnCapturedContext: false);
			await foreach (TSource item in (await func2(resource).ConfigureAwait(continueOnCapturedContext: false)).WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				yield return item;
			}
		}
	}

	public static IAsyncEnumerable<TSource> Using<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(0)] TResource>(Func<CancellationToken, Task<TResource>> resourceFactory, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 0, 1, 1 })] Func<TResource, CancellationToken, ValueTask<IAsyncEnumerable<TSource>>> enumerableFactory) where TResource : IDisposable
	{
		if (resourceFactory == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("resourceFactory");
		}
		if (enumerableFactory == null)
		{
			throw _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError.ArgumentNull("enumerableFactory");
		}
		return Core(resourceFactory, enumerableFactory, default(CancellationToken));
		[return: _003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0 })]
		static async IAsyncEnumerable<TSource> Core([_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 1, 0 })] Func<CancellationToken, Task<TResource>> func, [_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0, 0, 1, 0 })] Func<TResource, CancellationToken, ValueTask<IAsyncEnumerable<TSource>>> func2, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			using TResource resource = await func(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			await foreach (TSource item in (await func2(resource, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				yield return item;
			}
		}
	}
}
