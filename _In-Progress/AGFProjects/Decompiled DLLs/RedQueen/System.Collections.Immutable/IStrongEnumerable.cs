using System.Runtime.CompilerServices;

namespace System.Collections.Immutable;

internal interface IStrongEnumerable<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] out T, TEnumerator> where TEnumerator : struct, IStrongEnumerator<T>
{
	TEnumerator GetEnumerator();
}
