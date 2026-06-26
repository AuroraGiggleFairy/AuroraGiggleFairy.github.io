using System.Xml;

namespace ICSharpCode.WpfDesign.XamlDom;

public class PositionXmlAttribute : XmlAttribute, IXmlLineInfo
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int lineNumber;

	[PublicizedFrom(EAccessModifier.Private)]
	public int linePosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasLineInfo;

	public int LineNumber => lineNumber;

	public int LinePosition => linePosition;

	[PublicizedFrom(EAccessModifier.Internal)]
	public PositionXmlAttribute(string prefix, string localName, string namespaceURI, XmlDocument doc, IXmlLineInfo lineInfo)
		: base(prefix, localName, namespaceURI, doc)
	{
		if (lineInfo != null)
		{
			lineNumber = lineInfo.LineNumber;
			linePosition = lineInfo.LinePosition;
			hasLineInfo = true;
		}
	}

	public bool HasLineInfo()
	{
		return hasLineInfo;
	}
}
