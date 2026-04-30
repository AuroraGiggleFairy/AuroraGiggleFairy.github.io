using System;

namespace Discord.Interactions;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true, Inherited = true)]
internal class ChoiceAttribute : Attribute
{
	public string Name { get; }

	public SlashCommandChoiceType Type { get; }

	public object Value { get; }

	private ChoiceAttribute(string name)
	{
		Name = name;
	}

	public ChoiceAttribute(string name, string value)
		: this(name)
	{
		Type = SlashCommandChoiceType.String;
		Value = value;
	}

	public ChoiceAttribute(string name, int value)
		: this(name)
	{
		Type = SlashCommandChoiceType.Integer;
		Value = value;
	}

	public ChoiceAttribute(string name, double value)
		: this(name)
	{
		Type = SlashCommandChoiceType.Number;
		Value = value;
	}
}
