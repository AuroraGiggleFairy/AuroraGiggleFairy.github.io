using System;

namespace Discord.Interactions;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
internal class GroupAttribute : Attribute
{
	public string Name { get; }

	public string Description { get; }

	public GroupAttribute(string name, string description)
	{
		Name = name;
		Description = description;
	}
}
