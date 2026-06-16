namespace XMLData.Exceptions;

public class RedefinedElementException : XmlParserException
{
	public RedefinedElementException(string _msg, int _line)
		: base(_msg, _line)
	{
	}
}
