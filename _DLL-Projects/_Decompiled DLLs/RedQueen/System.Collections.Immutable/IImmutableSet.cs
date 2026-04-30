using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Collections.Immutable;

[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(1)]
internal interface IImmutableSet<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T> : IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable
{
	IImmutableSet<T> Clear();

	bool Contains(T value);

	IImmutableSet<T> Add(T value);

	IImmutableSet<T> Remove(T value);

	bool TryGetValue(T equalValue, out T actualValue);

	IImmutableSet<T> Intersect(IEnumerable<T> other);

	IImmutableSet<T> Except(IEnumerable<T> other);

	IImmutableSet<T> SymmetricExcept(IEnumerable<T> other);

	IImmutableSet<T> Union(IEnumerable<T> other);

	bool SetEquals(IEnumerable<T> other);

	bool IsProperSubsetOf(IEnumerable<T> other);

	bool IsProperSupersetOf(IEnumerable<T> other);

	bool IsSubsetOf(IEnumerable<T> other);

	bool IsSupersetOf(IEnumerable<T> other);

	bool Overlaps(IEnumerable<T> other);
}
