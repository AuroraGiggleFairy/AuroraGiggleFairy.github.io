namespace XMLData.Formatters;

public class MassFormatter : ValueFormatter<float>
{
	public override string FormatValue(float _value)
	{
		return _value.ToCultureInvariantString();
	}
}
