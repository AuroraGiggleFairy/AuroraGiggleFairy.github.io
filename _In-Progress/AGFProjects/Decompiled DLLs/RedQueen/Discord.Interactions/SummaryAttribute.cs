using System;

namespace Discord.Interactions;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
internal class SummaryAttribute : Attribute
{
	public string Name { get; }

	public string Description { get; }

	public SummaryAttribute(string name = null, string description = null)
	{
		Name = name;
		Description = description;
	}
}
