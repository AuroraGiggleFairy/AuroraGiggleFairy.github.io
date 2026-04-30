namespace Discord;

internal class TextInputComponent : IMessageComponent
{
	public ComponentType Type => ComponentType.TextInput;

	public string CustomId { get; }

	public string Label { get; }

	public string Placeholder { get; }

	public int? MinLength { get; }

	public int? MaxLength { get; }

	public TextInputStyle Style { get; }

	public bool? Required { get; }

	public string Value { get; }

	internal TextInputComponent(string customId, string label, string placeholder, int? minLength, int? maxLength, TextInputStyle style, bool? required, string value)
	{
		CustomId = customId;
		Label = label;
		Placeholder = placeholder;
		MinLength = minLength;
		MaxLength = maxLength;
		Style = style;
		Required = required;
		Value = value;
	}
}
