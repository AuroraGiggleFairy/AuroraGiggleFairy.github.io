using Discord.Interactions.Builders;

namespace Discord.Interactions;

internal class TextInputComponentInfo : InputComponentInfo
{
	public TextInputStyle Style { get; }

	public string Placeholder { get; }

	public int MinLength { get; }

	public int MaxLength { get; }

	public string InitialValue { get; }

	internal TextInputComponentInfo(TextInputComponentBuilder builder, ModalInfo modal)
		: base(builder, modal)
	{
		Style = builder.Style;
		Placeholder = builder.Placeholder;
		MinLength = builder.MinLength;
		MaxLength = builder.MaxLength;
		InitialValue = builder.InitialValue;
	}
}
