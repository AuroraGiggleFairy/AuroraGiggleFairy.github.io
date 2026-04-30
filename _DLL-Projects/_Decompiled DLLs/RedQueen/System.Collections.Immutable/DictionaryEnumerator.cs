using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Collections.Immutable;

[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(0)]
[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(1)]
internal sealed class DictionaryEnumerator<TKey, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TValue> : IDictionaryEnumerator, IEnumerator
{
	private readonly IEnumerator<KeyValuePair<TKey, TValue>> _inner;

	public DictionaryEntry Entry => new DictionaryEntry(_inner.Current.Key, _inner.Current.Value);

	public object Key => _inner.Current.Key;

	[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)]
	public object Value
	{
		[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(2)]
		get
		{
			return _inner.Current.Value;
		}
	}

	public object Current => Entry;

	internal DictionaryEnumerator([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 1, 0, 1, 1 })] IEnumerator<KeyValuePair<TKey, TValue>> inner)
	{
		Requires.NotNull(inner, "inner");
		_inner = inner;
	}

	public bool MoveNext()
	{
		return _inner.MoveNext();
	}

	public void Reset()
	{
		_inner.Reset();
	}
}
