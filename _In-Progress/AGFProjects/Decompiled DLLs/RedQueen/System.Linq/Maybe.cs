using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Linq;

[_003C943c0d9f_002D9e8d_002D4b94_002D8043_002Dfa75de3db6e3_003EIsReadOnly]
[_003C101b90a7_002Ded16_002D473d_002D9ce2_002D3a947e6817ed_003ENullableContext(1)]
[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(0)]
internal struct Maybe<[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(2)] T>(T value) : IEquatable<Maybe<T>>
{
	public bool HasValue { get; } = true;

	public T Value { get; } = value;

	public bool Equals([_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 0, 1 })] Maybe<T> other)
	{
		if (HasValue == other.HasValue)
		{
			return EqualityComparer<T>.Default.Equals(Value, other.Value);
		}
		return false;
	}

	[_003C101b90a7_002Ded16_002D473d_002D9ce2_002D3a947e6817ed_003ENullableContext(2)]
	public override bool Equals(object other)
	{
		if (other is Maybe<T> other2)
		{
			return Equals(other2);
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (!HasValue)
		{
			return 0;
		}
		return EqualityComparer<T>.Default.GetHashCode(Value);
	}

	public static bool operator ==([_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 0, 1 })] Maybe<T> first, [_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 0, 1 })] Maybe<T> second)
	{
		return first.Equals(second);
	}

	public static bool operator !=([_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 0, 1 })] Maybe<T> first, [_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 0, 1 })] Maybe<T> second)
	{
		return !first.Equals(second);
	}
}
