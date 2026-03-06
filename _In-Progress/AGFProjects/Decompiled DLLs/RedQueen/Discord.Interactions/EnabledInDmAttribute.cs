using System;

namespace Discord.Interactions;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
internal class EnabledInDmAttribute : Attribute
{
	public bool IsEnabled { get; }

	public EnabledInDmAttribute(bool isEnabled)
	{
		IsEnabled = isEnabled;
	}
}
