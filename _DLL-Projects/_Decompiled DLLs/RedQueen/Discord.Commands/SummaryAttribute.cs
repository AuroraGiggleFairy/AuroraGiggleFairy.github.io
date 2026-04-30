using System;

namespace Discord.Commands;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
internal class SummaryAttribute : Attribute
{
	public string Text { get; }

	public SummaryAttribute(string text)
	{
		Text = text;
	}
}
