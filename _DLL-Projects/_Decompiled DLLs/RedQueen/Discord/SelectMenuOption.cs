namespace Discord;

internal class SelectMenuOption
{
	public string Label { get; }

	public string Value { get; }

	public string Description { get; }

	public IEmote Emote { get; }

	public bool? IsDefault { get; }

	internal SelectMenuOption(string label, string value, string description, IEmote emote, bool? defaultValue)
	{
		Label = label;
		Value = value;
		Description = description;
		Emote = emote;
		IsDefault = defaultValue;
	}
}
