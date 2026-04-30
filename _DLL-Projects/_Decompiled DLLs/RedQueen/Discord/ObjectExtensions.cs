using System;

namespace Discord;

internal static class ObjectExtensions
{
	public static bool IsNumericType(this object o)
	{
		TypeCode typeCode = Type.GetTypeCode(o.GetType());
		if ((uint)(typeCode - 5) <= 10u)
		{
			return true;
		}
		return false;
	}
}
