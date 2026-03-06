using System;

namespace Discord.Interactions;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
internal class RequiredInputAttribute : Attribute
{
	public bool IsRequired { get; }

	public RequiredInputAttribute(bool isRequired = true)
	{
		IsRequired = isRequired;
	}
}
