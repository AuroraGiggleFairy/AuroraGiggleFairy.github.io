using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace System.Collections.Immutable;

[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(0)]
[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(1)]
internal class ImmutableEnumerableDebuggerProxy<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>
{
	private readonly IEnumerable<T> _enumerable;

	private T[] _cachedContents;

	[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
	public T[] Contents => _cachedContents ?? (_cachedContents = _enumerable.ToArray());

	public ImmutableEnumerableDebuggerProxy(IEnumerable<T> enumerable)
	{
		Requires.NotNull(enumerable, "enumerable");
		_enumerable = enumerable;
	}
}
