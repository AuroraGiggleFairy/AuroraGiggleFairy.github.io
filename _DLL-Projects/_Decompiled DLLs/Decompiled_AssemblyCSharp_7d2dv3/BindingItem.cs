public abstract class BindingItem
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string fieldName;

	public readonly string SourceText;

	[PublicizedFrom(EAccessModifier.Protected)]
	public BindingItem(string _sourceText)
	{
		SourceText = _sourceText;
		fieldName = _sourceText.Substring(1, _sourceText.Length - 2);
	}

	public abstract string GetValue();
}
