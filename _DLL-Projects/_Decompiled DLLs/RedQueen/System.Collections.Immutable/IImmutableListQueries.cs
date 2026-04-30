using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Collections.Immutable;

[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(1)]
internal interface IImmutableListQueries<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T> : IReadOnlyList<T>, IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable
{
	ImmutableList<TOutput> ConvertAll<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TOutput>(Func<T, TOutput> converter);

	void ForEach(Action<T> action);

	ImmutableList<T> GetRange(int index, int count);

	void CopyTo(T[] array);

	void CopyTo(T[] array, int arrayIndex);

	void CopyTo(int index, T[] array, int arrayIndex, int count);

	bool Exists(Predicate<T> match);

	[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)]
	T Find(Predicate<T> match);

	ImmutableList<T> FindAll(Predicate<T> match);

	int FindIndex(Predicate<T> match);

	int FindIndex(int startIndex, Predicate<T> match);

	int FindIndex(int startIndex, int count, Predicate<T> match);

	[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)]
	T FindLast(Predicate<T> match);

	int FindLastIndex(Predicate<T> match);

	int FindLastIndex(int startIndex, Predicate<T> match);

	int FindLastIndex(int startIndex, int count, Predicate<T> match);

	bool TrueForAll(Predicate<T> match);

	int BinarySearch(T item);

	int BinarySearch(T item, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] IComparer<T> comparer);

	int BinarySearch(int index, int count, T item, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] IComparer<T> comparer);
}
