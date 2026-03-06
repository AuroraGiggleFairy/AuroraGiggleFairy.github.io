namespace Discord;

internal class ButtonComponent : IMessageComponent
{
	public ComponentType Type => ComponentType.Button;

	public ButtonStyle Style { get; }

	public string Label { get; }

	public IEmote Emote { get; }

	public string CustomId { get; }

	public string Url { get; }

	public bool IsDisabled { get; }

	public ButtonBuilder ToBuilder()
	{
		return new ButtonBuilder(Label, CustomId, Style, Url, Emote, IsDisabled);
	}

	internal ButtonComponent(ButtonStyle style, string label, IEmote emote, string customId, string url, bool isDisabled)
	{
		Style = style;
		Label = label;
		Emote = emote;
		CustomId = customId;
		Url = url;
		IsDisabled = isDisabled;
	}
}
