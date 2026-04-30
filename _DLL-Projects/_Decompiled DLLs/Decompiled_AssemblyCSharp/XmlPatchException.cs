using System;
using System.Xml;
using System.Xml.Linq;

public class XmlPatchException : Exception
{
	public XmlPatchException(XElement _patchElement, string _patchMethodName, string _message)
		: this(buildMessage(_patchElement, _patchMethodName, _message))
	{
	}

	public XmlPatchException(XElement _patchElement, string _patchMethodName, string _message, Exception _innerException)
		: this(buildMessage(_patchElement, _patchMethodName, _message), _innerException)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string buildMessage(XElement _patchElement, string _patchMethodName, string _message)
	{
		return $"XML.{_patchMethodName} ({_patchElement.GetXPath()}, line {((IXmlLineInfo)_patchElement).LineNumber} at pos {((IXmlLineInfo)_patchElement).LinePosition}): {_message}";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XmlPatchException(string _message)
		: base(_message)
	{
	}

	public XmlPatchException(string _message, Exception _innerException)
		: base(_message, _innerException)
	{
	}
}
