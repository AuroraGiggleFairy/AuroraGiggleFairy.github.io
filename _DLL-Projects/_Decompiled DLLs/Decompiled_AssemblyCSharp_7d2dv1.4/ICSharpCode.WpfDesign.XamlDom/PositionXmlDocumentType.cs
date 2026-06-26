using System.Xml;

namespace ICSharpCode.WpfDesign.XamlDom;

public class PositionXmlDocumentType : XmlDocumentType, IXmlLineInfo
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
	public PositionXmlDocumentType(string name, string publicId, string systemId, string internalSubset, XmlDocument doc, IXmlLineInfo lineInfo)
		: base(name, publicId, systemId, internalSubset, doc)
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
