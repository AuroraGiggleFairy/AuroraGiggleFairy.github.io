namespace Discord.Interactions;

internal sealed class ModalTextInputAttribute : ModalInputAttribute
{
	public override ComponentType ComponentType => ComponentType.TextInput;

	public TextInputStyle Style { get; }

	public string Placeholder { get; }

	public int MinLength { get; }

	public int MaxLength { get; }

	public string InitialValue { get; }

	public ModalTextInputAttribute(string customId, TextInputStyle style = TextInputStyle.Short, string placeholder = null, int minLength = 1, int maxLength = 4000, string initValue = null)
		: base(customId)
	{
		Style = style;
		Placeholder = placeholder;
		MinLength = minLength;
		MaxLength = maxLength;
		InitialValue = initValue;
	}
}
