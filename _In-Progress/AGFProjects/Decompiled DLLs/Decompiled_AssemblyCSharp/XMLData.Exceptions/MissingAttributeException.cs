namespace XMLData.Exceptions;

public class MissingAttributeException : XmlParserException
{
	public MissingAttributeException(string _msg, int _line)
		: base(_msg, _line)
	{
	}
}
