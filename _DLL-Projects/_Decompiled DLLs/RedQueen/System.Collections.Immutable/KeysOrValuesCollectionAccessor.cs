using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.Collections.Immutable;

[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(0)]
[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(1)]
internal abstract class KeysOrValuesCollectionAccessor<TKey, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TValue, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T> : ICollection<T>, IEnumerable<T>, IEnumerable, ICollection
{
	private readonly IImmutableDictionary<TKey, TValue> _dictionary;

	private readonly IEnumerable<T> _keysOrValues;

	public bool IsReadOnly => true;

	public int Count => _dictionary.Count;

	protected IImmutableDictionary<TKey, TValue> Dictionary => _dictionary;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	bool ICollection.IsSynchronized => true;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	object ICollection.SyncRoot => this;

	protected KeysOrValuesCollectionAccessor(IImmutableDictionary<TKey, TValue> dictionary, IEnumerable<T> keysOrValues)
	{
		Requires.NotNull(dictionary, "dictionary");
		Requires.NotNull(keysOrValues, "keysOrValues");
		_dictionary = dictionary;
		_keysOrValues = keysOrValues;
	}

	public void Add(T item)
	{
		throw new NotSupportedException();
	}

	public void Clear()
	{
		throw new NotSupportedException();
	}

	public abstract bool Contains(T item);

	public void CopyTo(T[] array, int arrayIndex)
	{
		Requires.NotNull(array, "array");
		Requires.Range(arrayIndex >= 0, "arrayIndex");
		Requires.Range(array.Length >= arrayIndex + Count, "arrayIndex");
		using IEnumerator<T> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			T current = enumerator.Current;
			array[arrayIndex++] = current;
		}
	}

	public bool Remove(T item)
	{
		throw new NotSupportedException();
	}

	public IEnumerator<T> GetEnumerator()
	{
		return _keysOrValues.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	void ICollection.CopyTo(Array array, int arrayIndex)
	{
		Requires.NotNull(array, "array");
		Requires.Range(arrayIndex >= 0, "arrayIndex");
		Requires.Range(array.Length >= arrayIndex + Count, "arrayIndex");
		using IEnumerator<T> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			T current = enumerator.Current;
			array.SetValue(current, arrayIndex++);
		}
	}
}
