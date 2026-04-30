using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Collections.Immutable;

[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(1)]
internal interface IImmutableDictionary<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TKey, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TValue> : IReadOnlyDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
{
	IImmutableDictionary<TKey, TValue> Clear();

	IImmutableDictionary<TKey, TValue> Add(TKey key, TValue value);

	IImmutableDictionary<TKey, TValue> AddRange([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 1, 0, 1, 1 })] IEnumerable<KeyValuePair<TKey, TValue>> pairs);

	IImmutableDictionary<TKey, TValue> SetItem(TKey key, TValue value);

	IImmutableDictionary<TKey, TValue> SetItems([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 1, 0, 1, 1 })] IEnumerable<KeyValuePair<TKey, TValue>> items);

	IImmutableDictionary<TKey, TValue> RemoveRange(IEnumerable<TKey> keys);

	IImmutableDictionary<TKey, TValue> Remove(TKey key);

	bool Contains([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1, 1 })] KeyValuePair<TKey, TValue> pair);

	bool TryGetKey(TKey equalKey, out TKey actualKey);
}
