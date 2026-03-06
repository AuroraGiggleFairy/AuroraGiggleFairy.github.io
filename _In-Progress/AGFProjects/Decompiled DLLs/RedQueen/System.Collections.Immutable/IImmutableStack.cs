using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Collections.Immutable;

[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(1)]
internal interface IImmutableStack<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T> : IEnumerable<T>, IEnumerable
{
	bool IsEmpty { get; }

	IImmutableStack<T> Clear();

	IImmutableStack<T> Push(T value);

	IImmutableStack<T> Pop();

	T Peek();
}
