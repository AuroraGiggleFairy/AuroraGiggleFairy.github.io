using System;

namespace Discord.Interactions;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
internal class MinLengthAttribute : Attribute
{
	public int Length { get; }

	public MinLengthAttribute(int length)
	{
		Length = length;
	}
}
