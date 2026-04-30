using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml;

namespace Newtonsoft.Json.Converters;

[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
internal interface IXmlNode
{
	XmlNodeType NodeType { get; }

	string LocalName { get; }

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(1)]
	List<IXmlNode> ChildNodes
	{
		[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
		get;
	}

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(1)]
	List<IXmlNode> Attributes
	{
		[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
		get;
	}

	IXmlNode ParentNode { get; }

	string Value { get; set; }

	string NamespaceUri { get; }

	object WrappedNode { get; }

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
	IXmlNode AppendChild(IXmlNode newChild);
}
