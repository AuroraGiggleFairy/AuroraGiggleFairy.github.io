using System.Runtime.CompilerServices;

namespace Newtonsoft.Json.Converters;

[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
internal interface IXmlDocument : IXmlNode
{
	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	IXmlElement DocumentElement
	{
		[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
		get;
	}

	IXmlNode CreateComment([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] string text);

	IXmlNode CreateTextNode([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] string text);

	IXmlNode CreateCDataSection([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] string data);

	IXmlNode CreateWhitespace([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] string text);

	IXmlNode CreateSignificantWhitespace([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] string text);

	IXmlNode CreateXmlDeclaration(string version, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] string encoding, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] string standalone);

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
	[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(1)]
	IXmlNode CreateXmlDocumentType([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(1)] string name, string publicId, string systemId, string internalSubset);

	IXmlNode CreateProcessingInstruction(string target, string data);

	IXmlElement CreateElement(string elementName);

	IXmlElement CreateElement(string qualifiedName, string namespaceUri);

	IXmlNode CreateAttribute(string name, string value);

	IXmlNode CreateAttribute(string qualifiedName, string namespaceUri, string value);
}
