namespace XMLData.Exceptions;

public class UnexpectedElementException : XmlParserException
{
	public UnexpectedElementException(string _msg, int _line)
		: base(_msg, _line)
	{
	}
}
