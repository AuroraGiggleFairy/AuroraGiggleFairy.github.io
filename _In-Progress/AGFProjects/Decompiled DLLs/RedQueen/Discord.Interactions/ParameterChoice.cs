namespace Discord.Interactions;

internal class ParameterChoice
{
	public string Name { get; }

	public object Value { get; }

	internal ParameterChoice(string name, object value)
	{
		Name = name;
		Value = value;
	}
}
