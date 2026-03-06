using System;

namespace Discord.Interactions;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
internal class MaxLengthAttribute : Attribute
{
	public int Length { get; }

	public MaxLengthAttribute(int length)
	{
		Length = length;
	}
}
