using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.Collections.Immutable;

[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(1)]
[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(0)]
internal class ImmutableSortedDictionaryBuilderDebuggerProxy<TKey, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TValue>
{
	private readonly ImmutableSortedDictionary<TKey, TValue>.Builder _map;

	private KeyValuePair<TKey, TValue>[] _contents;

	[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
	[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 1, 0, 1, 1 })]
	public KeyValuePair<TKey, TValue>[] Contents
	{
		[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 1, 0, 1, 1 })]
		get
		{
			if (_contents == null)
			{
				_contents = _map.ToArray(_map.Count);
			}
			return _contents;
		}
	}

	public ImmutableSortedDictionaryBuilderDebuggerProxy(ImmutableSortedDictionary<TKey, TValue>.Builder map)
	{
		Requires.NotNull(map, "map");
		_map = map;
	}
}
