namespace Discord.Interactions.Builders;

internal class TextInputComponentBuilder : InputComponentBuilder<TextInputComponentInfo, TextInputComponentBuilder>
{
	protected override TextInputComponentBuilder Instance => this;

	public TextInputStyle Style { get; set; }

	public string Placeholder { get; set; }

	public int MinLength { get; set; }

	public int MaxLength { get; set; }

	public string InitialValue { get; set; }

	public TextInputComponentBuilder(ModalBuilder modal)
		: base(modal)
	{
	}

	public TextInputComponentBuilder WithStyle(TextInputStyle style)
	{
		Style = style;
		return this;
	}

	public TextInputComponentBuilder WithPlaceholder(string placeholder)
	{
		Placeholder = placeholder;
		return this;
	}

	public TextInputComponentBuilder WithMinLength(int minLength)
	{
		MinLength = minLength;
		return this;
	}

	public TextInputComponentBuilder WithMaxLength(int maxLength)
	{
		MaxLength = maxLength;
		return this;
	}

	public TextInputComponentBuilder WithInitialValue(string value)
	{
		InitialValue = value;
		return this;
	}

	internal override TextInputComponentInfo Build(ModalInfo modal)
	{
		return new TextInputComponentInfo(this, modal);
	}
}
