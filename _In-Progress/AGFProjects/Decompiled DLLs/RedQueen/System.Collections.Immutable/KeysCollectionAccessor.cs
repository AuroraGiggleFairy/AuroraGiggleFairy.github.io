using System.Runtime.CompilerServices;

namespace System.Collections.Immutable;

[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(1)]
[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1, 1, 1 })]
internal sealed class KeysCollectionAccessor<TKey, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TValue> : KeysOrValuesCollectionAccessor<TKey, TValue, TKey>
{
	internal KeysCollectionAccessor(IImmutableDictionary<TKey, TValue> dictionary)
		: base(dictionary, dictionary.Keys)
	{
	}

	public override bool Contains(TKey item)
	{
		return base.Dictionary.ContainsKey(item);
	}
}
