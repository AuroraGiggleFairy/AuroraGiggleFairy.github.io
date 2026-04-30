using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml;

namespace Newtonsoft.Json.Converters;

[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
internal class XmlNodeWrapper : IXmlNode
{
	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(1)]
	private readonly XmlNode _node;

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
	private List<IXmlNode> _childNodes;

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
	private List<IXmlNode> _attributes;

	public object WrappedNode => _node;

	public XmlNodeType NodeType => _node.NodeType;

	public virtual string LocalName => _node.LocalName;

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(1)]
	public List<IXmlNode> ChildNodes
	{
		[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
		get
		{
			if (_childNodes == null)
			{
				if (!_node.HasChildNodes)
				{
					_childNodes = XmlNodeConverter.EmptyChildNodes;
				}
				else
				{
					_childNodes = new List<IXmlNode>(_node.ChildNodes.Count);
					foreach (XmlNode childNode in _node.ChildNodes)
					{
						_childNodes.Add(WrapNode(childNode));
					}
				}
			}
			return _childNodes;
		}
	}

	protected virtual bool HasChildNodes => _node.HasChildNodes;

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(1)]
	public List<IXmlNode> Attributes
	{
		[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
		get
		{
			if (_attributes == null)
			{
				if (!HasAttributes)
				{
					_attributes = XmlNodeConverter.EmptyChildNodes;
				}
				else
				{
					_attributes = new List<IXmlNode>(_node.Attributes.Count);
					foreach (XmlAttribute attribute in _node.Attributes)
					{
						_attributes.Add(WrapNode(attribute));
					}
				}
			}
			return _attributes;
		}
	}

	private bool HasAttributes
	{
		get
		{
			if (_node is XmlElement xmlElement)
			{
				return xmlElement.HasAttributes;
			}
			XmlAttributeCollection attributes = _node.Attributes;
			if (attributes == null)
			{
				return false;
			}
			return attributes.Count > 0;
		}
	}

	public IXmlNode ParentNode
	{
		get
		{
			XmlNode xmlNode = ((_node is XmlAttribute xmlAttribute) ? xmlAttribute.OwnerElement : _node.ParentNode);
			if (xmlNode == null)
			{
				return null;
			}
			return WrapNode(xmlNode);
		}
	}

	public string Value
	{
		get
		{
			return _node.Value;
		}
		set
		{
			_node.Value = value;
		}
	}

	public string NamespaceUri => _node.NamespaceURI;

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
	public XmlNodeWrapper(XmlNode node)
	{
		_node = node;
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
	internal static IXmlNode WrapNode(XmlNode node)
	{
		return node.NodeType switch
		{
			XmlNodeType.Element => new XmlElementWrapper((XmlElement)node), 
			XmlNodeType.XmlDeclaration => new XmlDeclarationWrapper((XmlDeclaration)node), 
			XmlNodeType.DocumentType => new XmlDocumentTypeWrapper((XmlDocumentType)node), 
			_ => new XmlNodeWrapper(node), 
		};
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
	public IXmlNode AppendChild(IXmlNode newChild)
	{
		XmlNodeWrapper xmlNodeWrapper = (XmlNodeWrapper)newChild;
		_node.AppendChild(xmlNodeWrapper._node);
		_childNodes = null;
		_attributes = null;
		return newChild;
	}
}
