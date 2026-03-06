using System.Runtime.CompilerServices;

namespace System;

[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(0)]
[_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(1)]
internal static class _003Cd31d44e9_002D1cba_002D4eed_002D8583_002D1c78684977d9_003EError
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
		return new InvalidOperationException(_003Cf131b85d_002Dced5_002D441d_002Da7a2_002D18e9fe82341a_003EStrings.NO_ELEMENTS);
	}

	public static Exception MoreThanOneElement()
	{
		return new InvalidOperationException(_003Cf131b85d_002Dced5_002D441d_002Da7a2_002D18e9fe82341a_003EStrings.MORE_THAN_ONE_ELEMENT);
	}

	public static Exception NotSupported()
	{
		return new NotSupportedException(_003Cf131b85d_002Dced5_002D441d_002Da7a2_002D18e9fe82341a_003EStrings.NOT_SUPPORTED);
	}
}
