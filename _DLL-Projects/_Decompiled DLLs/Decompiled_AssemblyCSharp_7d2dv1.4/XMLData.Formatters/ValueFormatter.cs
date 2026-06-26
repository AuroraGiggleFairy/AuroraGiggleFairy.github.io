namespace XMLData.Formatters;

public abstract class ValueFormatter<TValue>
{
	public abstract string FormatValue(TValue _value);

	[PublicizedFrom(EAccessModifier.Protected)]
	public ValueFormatter()
	{
	}
}
