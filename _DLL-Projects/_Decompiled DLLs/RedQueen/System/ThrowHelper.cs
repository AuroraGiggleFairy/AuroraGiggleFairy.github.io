using System.Runtime.CompilerServices;

namespace System;

internal static class ThrowHelper
{
	internal static void ThrowArgumentNullException(System.ExceptionArgument argument)
	{
		throw GetArgumentNullException(argument);
	}

	internal static void ThrowArgumentOutOfRangeException(System.ExceptionArgument argument)
	{
		throw GetArgumentOutOfRangeException(argument);
	}

	private static ArgumentNullException GetArgumentNullException(System.ExceptionArgument argument)
	{
		return new ArgumentNullException(GetArgumentName(argument));
	}

	private static ArgumentOutOfRangeException GetArgumentOutOfRangeException(System.ExceptionArgument argument)
	{
		return new ArgumentOutOfRangeException(GetArgumentName(argument));
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static string GetArgumentName(System.ExceptionArgument argument)
	{
		return argument.ToString();
	}
}
