using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.Collections.Immutable;

[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(0)]
[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(2)]
internal static class Requires
{
	[DebuggerStepThrough]
	[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(1)]
	public static void NotNull<T>([ValidatedNotNull] T value, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] string parameterName) where T : class
	{
		if (value == null)
		{
			FailArgumentNullException(parameterName);
		}
	}

	[DebuggerStepThrough]
	[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(1)]
	public static T NotNullPassthrough<T>([ValidatedNotNull] T value, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] string parameterName) where T : class
	{
		NotNull(value, parameterName);
		return value;
	}

	[DebuggerStepThrough]
	public static void NotNullAllowStructs<T>([ValidatedNotNull][_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(1)] T value, string parameterName)
	{
		if (value == null)
		{
			FailArgumentNullException(parameterName);
		}
	}

	[DebuggerStepThrough]
	private static void FailArgumentNullException(string parameterName)
	{
		throw new ArgumentNullException(parameterName);
	}

	[DebuggerStepThrough]
	public static void Range(bool condition, string parameterName, string message = null)
	{
		if (!condition)
		{
			FailRange(parameterName, message);
		}
	}

	[DebuggerStepThrough]
	public static void FailRange(string parameterName, string message = null)
	{
		if (string.IsNullOrEmpty(message))
		{
			throw new ArgumentOutOfRangeException(parameterName);
		}
		throw new ArgumentOutOfRangeException(parameterName, message);
	}

	[DebuggerStepThrough]
	public static void Argument(bool condition, string parameterName, string message)
	{
		if (!condition)
		{
			throw new ArgumentException(message, parameterName);
		}
	}

	[DebuggerStepThrough]
	public static void Argument(bool condition)
	{
		if (!condition)
		{
			throw new ArgumentException();
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(1)]
	[DebuggerStepThrough]
	public static void FailObjectDisposed<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TDisposed>(TDisposed disposed)
	{
		throw new ObjectDisposedException(disposed.GetType().FullName);
	}
}
