using System;

namespace Discord.Commands;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
internal class RemarksAttribute : Attribute
{
	public string Text { get; }

	public RemarksAttribute(string text)
	{
		Text = text;
	}
}
