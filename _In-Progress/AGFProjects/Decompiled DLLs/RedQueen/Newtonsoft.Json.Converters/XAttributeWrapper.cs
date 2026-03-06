using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Newtonsoft.Json.Converters;

[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
internal class XAttributeWrapper : XObjectWrapper
{
	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(1)]
	private XAttribute Attribute
	{
		[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
		get
		{
			return (XAttribute)base.WrappedNode;
		}
	}

	public override string Value
	{
		get
		{
			return Attribute.Value;
		}
		set
		{
			Attribute.Value = value ?? string.Empty;
		}
	}

	public override string LocalName => Attribute.Name.LocalName;

	public override string NamespaceUri => Attribute.Name.NamespaceName;

	public override IXmlNode ParentNode
	{
		get
		{
			if (Attribute.Parent == null)
			{
				return null;
			}
			return XContainerWrapper.WrapNode(Attribute.Parent);
		}
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
	public XAttributeWrapper(XAttribute attribute)
		: base(attribute)
	{
	}
}
