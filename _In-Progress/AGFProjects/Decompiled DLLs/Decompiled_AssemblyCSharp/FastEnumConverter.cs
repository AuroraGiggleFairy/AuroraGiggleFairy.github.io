using System;
using Unity.Collections.LowLevel.Unsafe;

public static class FastEnumConverter<TEnum> where TEnum : struct, IConvertible
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly int underlyingSize = UnsafeUtility.SizeOf(Enum.GetUnderlyingType(typeof(TEnum)));

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly long underlyingMask = ((underlyingSize >= 8) ? (-1) : ((1L << underlyingSize * 8) - 1));

	public static int ToInt(TEnum _enum)
	{
		return (int)(UnsafeUtility.EnumToInt(_enum) & underlyingMask);
	}
}
