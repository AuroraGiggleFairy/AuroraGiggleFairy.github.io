namespace Discord;

internal class AutocompleteOption
{
	public ApplicationCommandOptionType Type { get; }

	public string Name { get; }

	public object Value { get; }

	public bool Focused { get; }

	internal AutocompleteOption(ApplicationCommandOptionType type, string name, object value, bool focused)
	{
		Type = type;
		Name = name;
		Value = value;
		Focused = focused;
	}
}
