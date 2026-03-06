using System;

namespace Discord.Interactions;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
internal class InputLabelAttribute : Attribute
{
	public string Label { get; }

	public InputLabelAttribute(string label)
	{
		Label = label;
	}
}
