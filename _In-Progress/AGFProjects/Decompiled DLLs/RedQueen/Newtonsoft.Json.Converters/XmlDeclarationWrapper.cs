using System.Runtime.CompilerServices;
using System.Xml;

namespace Newtonsoft.Json.Converters;

[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
internal class XmlDeclarationWrapper : XmlNodeWrapper, IXmlDeclaration, IXmlNode
{
	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(1)]
	private readonly XmlDeclaration _declaration;

	public string Version => _declaration.Version;

	public string Encoding
	{
		get
		{
			return _declaration.Encoding;
		}
		set
		{
			_declaration.Encoding = value;
		}
	}

	public string Standalone
	{
		get
		{
			return _declaration.Standalone;
		}
		set
		{
			_declaration.Standalone = value;
		}
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
	public XmlDeclarationWrapper(XmlDeclaration declaration)
		: base(declaration)
	{
		_declaration = declaration;
	}
}
