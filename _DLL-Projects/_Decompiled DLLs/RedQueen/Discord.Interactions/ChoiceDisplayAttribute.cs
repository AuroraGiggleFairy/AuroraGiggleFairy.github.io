using System;

namespace Discord.Interactions;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
internal class ChoiceDisplayAttribute : Attribute
{
	public string Name { get; }

	public ChoiceDisplayAttribute(string name)
	{
		Name = name;
	}
}
