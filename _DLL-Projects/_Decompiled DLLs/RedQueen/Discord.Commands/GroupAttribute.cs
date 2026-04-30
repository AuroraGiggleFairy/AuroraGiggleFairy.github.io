using System;

namespace Discord.Commands;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
internal class GroupAttribute : Attribute
{
	public string Prefix { get; }

	public GroupAttribute()
	{
		Prefix = null;
	}

	public GroupAttribute(string prefix)
	{
		Prefix = prefix;
	}
}
