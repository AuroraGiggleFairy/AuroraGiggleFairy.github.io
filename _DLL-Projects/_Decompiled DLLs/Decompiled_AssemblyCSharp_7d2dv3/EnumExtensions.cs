using System;
using System.Reflection;
using UniLinq;

public static class EnumExtensions
{
	public static string GetDocumentation(this Enum enumValue)
	{
		return enumValue.GetType().GetMember(enumValue.ToString()).FirstOrDefault()?.GetCustomAttribute<Documentation>()?.Text ?? enumValue.ToString();
	}
}
