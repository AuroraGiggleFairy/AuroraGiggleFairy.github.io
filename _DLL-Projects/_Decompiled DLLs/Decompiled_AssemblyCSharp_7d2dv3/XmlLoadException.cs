using System;
using System.Xml;
using System.Xml.Linq;

public class XmlLoadException : Exception
{
	public XmlLoadException(string _xmlName, XElement _element, string _message)
		: this(buildMessage(_element, _xmlName, _message))
	{
	}

	public XmlLoadException(string _xmlName, XElement _element, string _message, Exception _innerException)
		: this(buildMessage(_element, _xmlName, _message), _innerException)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string buildMessage(XElement _element, string _xmlName, string _message)
	{
		return $"Error loading {_xmlName}: {_message} (line {((IXmlLineInfo)_element).LineNumber} at pos {((IXmlLineInfo)_element).LinePosition})";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XmlLoadException(string _message)
		: base(_message)
	{
	}

	public XmlLoadException(string _message, Exception _innerException)
		: base(_message, _innerException)
	{
	}
}
