namespace XMLData.Exceptions;

public class IncorrectAttributeOccurrenceException : XmlParserException
{
	public IncorrectAttributeOccurrenceException(string _msg, int _line)
		: base(_msg, _line)
	{
	}
}
