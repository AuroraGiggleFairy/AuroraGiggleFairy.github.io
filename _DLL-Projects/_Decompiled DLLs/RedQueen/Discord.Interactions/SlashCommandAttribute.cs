using System;

namespace Discord.Interactions;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
internal class SlashCommandAttribute : Attribute
{
	public string Name { get; }

	public string Description { get; }

	public bool IgnoreGroupNames { get; }

	public RunMode RunMode { get; }

	public SlashCommandAttribute(string name, string description, bool ignoreGroupNames = false, RunMode runMode = RunMode.Default)
	{
		Name = name;
		Description = description;
		IgnoreGroupNames = ignoreGroupNames;
		RunMode = runMode;
	}
}
