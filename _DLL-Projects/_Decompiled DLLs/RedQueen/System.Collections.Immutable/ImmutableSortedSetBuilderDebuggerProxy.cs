using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.Collections.Immutable;

[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(0)]
[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(1)]
internal class ImmutableSortedSetBuilderDebuggerProxy<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>
{
	private readonly ImmutableSortedSet<T>.Builder _set;

	[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
	public T[] Contents => _set.ToArray(_set.Count);

	public ImmutableSortedSetBuilderDebuggerProxy(ImmutableSortedSet<T>.Builder builder)
	{
		Requires.NotNull(builder, "builder");
		_set = builder;
	}
}
