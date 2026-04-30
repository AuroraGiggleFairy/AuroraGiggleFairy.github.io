using System;

namespace Discord.Interactions;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
internal class ComponentInteractionAttribute : Attribute
{
	public string CustomId { get; }

	public bool IgnoreGroupNames { get; }

	public RunMode RunMode { get; }

	public ComponentInteractionAttribute(string customId, bool ignoreGroupNames = false, RunMode runMode = RunMode.Default)
	{
		CustomId = customId;
		IgnoreGroupNames = ignoreGroupNames;
		RunMode = runMode;
	}
}
