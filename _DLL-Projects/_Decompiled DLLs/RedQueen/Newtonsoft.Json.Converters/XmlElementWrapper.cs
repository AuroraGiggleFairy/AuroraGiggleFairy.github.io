using System.Runtime.CompilerServices;
using System.Xml;

namespace Newtonsoft.Json.Converters;

[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
internal class XmlElementWrapper : XmlNodeWrapper, IXmlElement, IXmlNode
{
	private readonly XmlElement _element;

	public bool IsEmpty => _element.IsEmpty;

	public XmlElementWrapper(XmlElement element)
		: base(element)
	{
		_element = element;
	}

	public void SetAttributeNode(IXmlNode attribute)
	{
		XmlNodeWrapper xmlNodeWrapper = (XmlNodeWrapper)attribute;
		_element.SetAttributeNode((XmlAttribute)xmlNodeWrapper.WrappedNode);
	}

	[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	public string GetPrefixOfNamespace(string namespaceUri)
	{
		return _element.GetPrefixOfNamespace(namespaceUri);
	}
}
