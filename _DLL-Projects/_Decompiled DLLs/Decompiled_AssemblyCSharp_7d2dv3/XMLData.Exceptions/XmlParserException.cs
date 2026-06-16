using System;

namespace XMLData.Exceptions;

public class XmlParserException : Exception
{
	[field: PublicizedFrom(EAccessModifier.Private)]
	public int Line
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public override string Message => $"{base.Message} (line {Line})";

	public XmlParserException(string _msg, int _line)
		: base(_msg)
	{
		Line = _line;
	}

	public XmlParserException(string _msg, int _line, Exception _innerException)
		: base(_msg, _innerException)
	{
		Line = _line;
	}

	public override string ToString()
	{
		return Message;
	}
}
