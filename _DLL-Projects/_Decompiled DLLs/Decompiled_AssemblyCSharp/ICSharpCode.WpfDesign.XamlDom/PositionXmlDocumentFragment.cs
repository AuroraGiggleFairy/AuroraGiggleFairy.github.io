using System.Xml;

namespace ICSharpCode.WpfDesign.XamlDom;

public class PositionXmlDocumentFragment : XmlDocumentFragment, IXmlLineInfo
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
	public PositionXmlDocumentFragment(XmlDocument doc, IXmlLineInfo lineInfo)
		: base(doc)
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
