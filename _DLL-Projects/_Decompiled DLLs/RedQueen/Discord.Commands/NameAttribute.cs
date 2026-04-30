using System;

namespace Discord.Commands;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
internal class NameAttribute : Attribute
{
	public string Text { get; }

	public NameAttribute(string text)
	{
		Text = text;
	}
}
