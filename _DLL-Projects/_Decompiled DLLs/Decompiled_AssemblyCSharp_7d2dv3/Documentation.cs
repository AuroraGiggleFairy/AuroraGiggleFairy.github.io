using System;

[AttributeUsage(AttributeTargets.Field)]
public class Documentation : Attribute
{
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string Text { get; }

	public Documentation(string value)
	{
		Text = value;
	}
}
