using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Linq;

namespace Newtonsoft.Json.Converters;

[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
internal class XDeclarationWrapper : XObjectWrapper, IXmlDeclaration, IXmlNode
{
	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(1)]
	[field: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(1)]
	internal XDeclaration Declaration
	{
		[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
		get;
	}

	public override XmlNodeType NodeType => XmlNodeType.XmlDeclaration;

	public string Version => Declaration.Version;

	public string Encoding
	{
		get
		{
			return Declaration.Encoding;
		}
		set
		{
			Declaration.Encoding = value;
		}
	}

	public string Standalone
	{
		get
		{
			return Declaration.Standalone;
		}
		set
		{
			Declaration.Standalone = value;
		}
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
	public XDeclarationWrapper(XDeclaration declaration)
		: base(null)
	{
		Declaration = declaration;
	}
}
