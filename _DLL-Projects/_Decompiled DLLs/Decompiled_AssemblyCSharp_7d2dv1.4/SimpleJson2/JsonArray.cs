using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;

namespace SimpleJson2;

[GeneratedCode("simple-json", "1.0.0")]
[EditorBrowsable(EditorBrowsableState.Never)]
public class JsonArray : List<object>
{
	public JsonArray()
	{
	}

	public JsonArray(int capacity)
		: base(capacity)
	{
	}

	public override string ToString()
	{
		return SimpleJson2.SerializeObject(this) ?? string.Empty;
	}
}
