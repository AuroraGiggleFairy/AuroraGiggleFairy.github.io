using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Converters;

[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
internal class XElementWrapper : XContainerWrapper, IXmlElement, IXmlNode
{
	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
	private List<IXmlNode> _attributes;

	private XElement Element => (XElement)base.WrappedNode;

	public override List<IXmlNode> Attributes
	{
		get
		{
			if (_attributes == null)
			{
				if (!Element.HasAttributes && !HasImplicitNamespaceAttribute(NamespaceUri))
				{
					_attributes = XmlNodeConverter.EmptyChildNodes;
				}
				else
				{
					_attributes = new List<IXmlNode>();
					foreach (XAttribute item in Element.Attributes())
					{
						_attributes.Add(new XAttributeWrapper(item));
					}
					string namespaceUri = NamespaceUri;
					if (HasImplicitNamespaceAttribute(namespaceUri))
					{
						_attributes.Insert(0, new XAttributeWrapper(new XAttribute("xmlns", namespaceUri)));
					}
				}
			}
			return _attributes;
		}
	}

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	public override string Value
	{
		[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
		get
		{
			return Element.Value;
		}
		[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
		set
		{
			Element.Value = value ?? string.Empty;
		}
	}

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	public override string LocalName
	{
		[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
		get
		{
			return Element.Name.LocalName;
		}
	}

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	public override string NamespaceUri
	{
		[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
		get
		{
			return Element.Name.NamespaceName;
		}
	}

	public bool IsEmpty => Element.IsEmpty;

	public XElementWrapper(XElement element)
		: base(element)
	{
	}

	public void SetAttributeNode(IXmlNode attribute)
	{
		XObjectWrapper xObjectWrapper = (XObjectWrapper)attribute;
		Element.Add(xObjectWrapper.WrappedNode);
		_attributes = null;
	}

	private bool HasImplicitNamespaceAttribute(string namespaceUri)
	{
		if (!StringUtils.IsNullOrEmpty(namespaceUri) && namespaceUri != ParentNode?.NamespaceUri && StringUtils.IsNullOrEmpty(GetPrefixOfNamespace(namespaceUri)))
		{
			bool flag = false;
			if (Element.HasAttributes)
			{
				foreach (XAttribute item in Element.Attributes())
				{
					if (item.Name.LocalName == "xmlns" && StringUtils.IsNullOrEmpty(item.Name.NamespaceName) && item.Value == namespaceUri)
					{
						flag = true;
					}
				}
			}
			if (!flag)
			{
				return true;
			}
		}
		return false;
	}

	public override IXmlNode AppendChild(IXmlNode newChild)
	{
		IXmlNode result = base.AppendChild(newChild);
		_attributes = null;
		return result;
	}

	[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	public string GetPrefixOfNamespace(string namespaceUri)
	{
		return Element.GetPrefixOfNamespace(namespaceUri);
	}
}
