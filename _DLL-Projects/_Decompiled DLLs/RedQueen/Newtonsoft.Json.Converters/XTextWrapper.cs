using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Newtonsoft.Json.Converters;

[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
internal class XTextWrapper : XObjectWrapper
{
	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(1)]
	private XText Text
	{
		[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
		get
		{
			return (XText)base.WrappedNode;
		}
	}

	public override string Value
	{
		get
		{
			return Text.Value;
		}
		set
		{
			Text.Value = value ?? string.Empty;
		}
	}

	public override IXmlNode ParentNode
	{
		get
		{
			if (Text.Parent == null)
			{
				return null;
			}
			return XContainerWrapper.WrapNode(Text.Parent);
		}
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
	public XTextWrapper(XText text)
		: base(text)
	{
	}
}
