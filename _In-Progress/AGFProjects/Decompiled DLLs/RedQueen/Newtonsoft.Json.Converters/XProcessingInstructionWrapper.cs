using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Newtonsoft.Json.Converters;

[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
internal class XProcessingInstructionWrapper : XObjectWrapper
{
	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(1)]
	private XProcessingInstruction ProcessingInstruction
	{
		[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
		get
		{
			return (XProcessingInstruction)base.WrappedNode;
		}
	}

	public override string LocalName => ProcessingInstruction.Target;

	public override string Value
	{
		get
		{
			return ProcessingInstruction.Data;
		}
		set
		{
			ProcessingInstruction.Data = value ?? string.Empty;
		}
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
	public XProcessingInstructionWrapper(XProcessingInstruction processingInstruction)
		: base(processingInstruction)
	{
	}
}
