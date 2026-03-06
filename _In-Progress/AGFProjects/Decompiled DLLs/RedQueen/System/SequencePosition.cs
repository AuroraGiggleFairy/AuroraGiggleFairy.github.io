using System.ComponentModel;
using System.Numerics.Hashing;
using System.Runtime.CompilerServices;

namespace System;

[_003Ca49c99bc_002D5074_002D4086_002Dac07_002Dfb6cba902e04_003EIsReadOnly]
internal struct SequencePosition(object @object, int integer) : IEquatable<SequencePosition>
{
	private readonly object _object = @object;

	private readonly int _integer = integer;

	[EditorBrowsable(EditorBrowsableState.Never)]
	public object GetObject()
	{
		return _object;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public int GetInteger()
	{
		return _integer;
	}

	public bool Equals(SequencePosition other)
	{
		if (_integer == other._integer)
		{
			return object.Equals(_object, other._object);
		}
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override bool Equals(object obj)
	{
		if (obj is SequencePosition other)
		{
			return Equals(other);
		}
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override int GetHashCode()
	{
		return _003C7b849468_002Dff53_002D41a4_002D939b_002Db0f040936437_003EHashHelpers.Combine(_object?.GetHashCode() ?? 0, _integer);
	}
}
