using System.Xml;

namespace ICSharpCode.WpfDesign.XamlDom;

public class PositionXmlImplementation : XmlImplementation
{
	public override XmlDocument CreateDocument()
	{
		return new PositionXmlDocument();
	}
}
