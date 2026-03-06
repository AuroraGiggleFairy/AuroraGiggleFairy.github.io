namespace XMLData.Parsers;

public static class intParser
{
	public static int Parse(string _value)
	{
		return int.Parse(_value);
	}

	public static string Unparse(int _value)
	{
		return _value.ToString();
	}
}
