using System.Runtime.CompilerServices;
using System.Xml;

namespace Newtonsoft.Json.Converters;

[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
internal class XmlDocumentWrapper : XmlNodeWrapper, IXmlDocument, IXmlNode
{
	private readonly XmlDocument _document;

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	public IXmlElement DocumentElement
	{
		[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
		get
		{
			if (_document.DocumentElement == null)
			{
				return null;
			}
			return new XmlElementWrapper(_document.DocumentElement);
		}
	}

	public XmlDocumentWrapper(XmlDocument document)
		: base(document)
	{
		_document = document;
	}

	public IXmlNode CreateComment([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] string data)
	{
		return new XmlNodeWrapper(_document.CreateComment(data));
	}

	public IXmlNode CreateTextNode([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] string text)
	{
		return new XmlNodeWrapper(_document.CreateTextNode(text));
	}

	public IXmlNode CreateCDataSection([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] string data)
	{
		return new XmlNodeWrapper(_document.CreateCDataSection(data));
	}

	public IXmlNode CreateWhitespace([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] string text)
	{
		return new XmlNodeWrapper(_document.CreateWhitespace(text));
	}

	public IXmlNode CreateSignificantWhitespace([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] string text)
	{
		return new XmlNodeWrapper(_document.CreateSignificantWhitespace(text));
	}

	public IXmlNode CreateXmlDeclaration(string version, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] string encoding, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] string standalone)
	{
		return new XmlDeclarationWrapper(_document.CreateXmlDeclaration(version, encoding, standalone));
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
	[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(1)]
	public IXmlNode CreateXmlDocumentType([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(1)] string name, string publicId, string systemId, string internalSubset)
	{
		return new XmlDocumentTypeWrapper(_document.CreateDocumentType(name, publicId, systemId, null));
	}

	public IXmlNode CreateProcessingInstruction(string target, string data)
	{
		return new XmlNodeWrapper(_document.CreateProcessingInstruction(target, data));
	}

	public IXmlElement CreateElement(string elementName)
	{
		return new XmlElementWrapper(_document.CreateElement(elementName));
	}

	public IXmlElement CreateElement(string qualifiedName, string namespaceUri)
	{
		return new XmlElementWrapper(_document.CreateElement(qualifiedName, namespaceUri));
	}

	public IXmlNode CreateAttribute(string name, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] string value)
	{
		return new XmlNodeWrapper(_document.CreateAttribute(name))
		{
			Value = value
		};
	}

	public IXmlNode CreateAttribute(string qualifiedName, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] string namespaceUri, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] string value)
	{
		return new XmlNodeWrapper(_document.CreateAttribute(qualifiedName, namespaceUri))
		{
			Value = value
		};
	}
}
