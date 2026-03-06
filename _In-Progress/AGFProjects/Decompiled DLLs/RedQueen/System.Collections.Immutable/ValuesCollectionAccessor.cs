using System.Runtime.CompilerServices;

namespace System.Collections.Immutable;

[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1, 1, 1 })]
[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(1)]
internal sealed class ValuesCollectionAccessor<TKey, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TValue> : KeysOrValuesCollectionAccessor<TKey, TValue, TValue>
{
	internal ValuesCollectionAccessor(IImmutableDictionary<TKey, TValue> dictionary)
		: base(dictionary, dictionary.Values)
	{
	}

	public override bool Contains(TValue item)
	{
		if (base.Dictionary is ImmutableSortedDictionary<TKey, TValue> immutableSortedDictionary)
		{
			return immutableSortedDictionary.ContainsValue(item);
		}
		if (base.Dictionary is IImmutableDictionaryInternal<TKey, TValue> immutableDictionaryInternal)
		{
			return immutableDictionaryInternal.ContainsValue(item);
		}
		throw new NotSupportedException();
	}
}
