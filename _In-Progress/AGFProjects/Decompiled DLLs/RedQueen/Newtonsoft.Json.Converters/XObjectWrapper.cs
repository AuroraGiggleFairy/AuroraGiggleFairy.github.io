using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Linq;

namespace Newtonsoft.Json.Converters;

[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
internal class XObjectWrapper : IXmlNode
{
	private readonly XObject _xmlObject;

	public object WrappedNode => _xmlObject;

	public virtual XmlNodeType NodeType => _xmlObject?.NodeType ?? XmlNodeType.None;

	public virtual string LocalName => null;

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(1)]
	public virtual List<IXmlNode> ChildNodes
	{
		[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
		get
		{
			return XmlNodeConverter.EmptyChildNodes;
		}
	}

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(1)]
	public virtual List<IXmlNode> Attributes
	{
		[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
		get
		{
			return XmlNodeConverter.EmptyChildNodes;
		}
	}

	public virtual IXmlNode ParentNode => null;

	public virtual string Value
	{
		get
		{
			return null;
		}
		set
		{
			throw new InvalidOperationException();
		}
	}

	public virtual string NamespaceUri => null;

	public XObjectWrapper(XObject xmlObject)
	{
		_xmlObject = xmlObject;
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
	public virtual IXmlNode AppendChild(IXmlNode newChild)
	{
		throw new InvalidOperationException();
	}
}
