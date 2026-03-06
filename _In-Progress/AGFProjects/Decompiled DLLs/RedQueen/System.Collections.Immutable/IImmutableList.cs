using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Collections.Immutable;

[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(1)]
internal interface IImmutableList<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T> : IReadOnlyList<T>, IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable
{
	IImmutableList<T> Clear();

	int IndexOf(T item, int index, int count, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<T> equalityComparer);

	int LastIndexOf(T item, int index, int count, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<T> equalityComparer);

	IImmutableList<T> Add(T value);

	IImmutableList<T> AddRange(IEnumerable<T> items);

	IImmutableList<T> Insert(int index, T element);

	IImmutableList<T> InsertRange(int index, IEnumerable<T> items);

	IImmutableList<T> Remove(T value, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<T> equalityComparer);

	IImmutableList<T> RemoveAll(Predicate<T> match);

	IImmutableList<T> RemoveRange(IEnumerable<T> items, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<T> equalityComparer);

	IImmutableList<T> RemoveRange(int index, int count);

	IImmutableList<T> RemoveAt(int index);

	IImmutableList<T> SetItem(int index, T value);

	IImmutableList<T> Replace(T oldValue, T newValue, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<T> equalityComparer);
}
