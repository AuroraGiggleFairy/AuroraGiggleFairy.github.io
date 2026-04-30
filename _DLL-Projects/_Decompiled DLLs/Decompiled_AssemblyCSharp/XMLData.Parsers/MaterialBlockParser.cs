namespace XMLData.Parsers;

public static class MaterialBlockParser
{
	public static MaterialBlock Parse(string _value)
	{
		return MaterialBlock.materials[_value];
	}

	public static string Unparse(MaterialBlock _value)
	{
		return _value.id;
	}
}
