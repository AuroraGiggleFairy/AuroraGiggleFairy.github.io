using System;

namespace XMLData.Exceptions;

public class InvalidValueException : XmlParserException
{
	public InvalidValueException(string _msg, int _line)
		: base(_msg, _line)
	{
	}

	public InvalidValueException(string _msg, int _line, Exception _innerException)
		: base(_msg, _line, _innerException)
	{
	}
}
