using System;

namespace Discord.Interactions;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
internal sealed class ModalInteractionAttribute : Attribute
{
	public string CustomId { get; }

	public bool IgnoreGroupNames { get; }

	public RunMode RunMode { get; }

	public ModalInteractionAttribute(string customId, bool ignoreGroupNames = false, RunMode runMode = RunMode.Default)
	{
		CustomId = customId;
		IgnoreGroupNames = ignoreGroupNames;
		RunMode = runMode;
	}
}
