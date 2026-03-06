using System.Runtime.CompilerServices;

namespace System;

[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(0)]
[_003C101b90a7_002Ded16_002D473d_002D9ce2_002D3a947e6817ed_003ENullableContext(1)]
internal static class Error
{
	public static Exception ArgumentNull(string paramName)
	{
		return new ArgumentNullException(paramName);
	}

	public static Exception ArgumentOutOfRange(string paramName)
	{
		return new ArgumentOutOfRangeException(paramName);
	}

	public static Exception NoElements()
	{
		return new InvalidOperationException(Strings.NO_ELEMENTS);
	}

	public static Exception MoreThanOneElement()
	{
		return new InvalidOperationException(Strings.MORE_THAN_ONE_ELEMENT);
	}

	public static Exception NotSupported()
	{
		return new NotSupportedException(Strings.NOT_SUPPORTED);
	}
}
