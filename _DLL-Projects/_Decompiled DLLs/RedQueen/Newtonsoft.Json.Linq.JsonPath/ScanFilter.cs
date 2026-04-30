using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Newtonsoft.Json.Linq.JsonPath;

[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
internal class ScanFilter : PathFilter
{
	internal string Name;

	public ScanFilter(string name)
	{
		Name = name;
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
	public override IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] JsonSelectSettings settings)
	{
		foreach (JToken c in current)
		{
			if (Name == null)
			{
				yield return c;
			}
			JToken value = c;
			while (true)
			{
				JContainer container = value as JContainer;
				value = PathFilter.GetNextScanValue(c, container, value);
				if (value == null)
				{
					break;
				}
				if (value is JProperty jProperty)
				{
					if (jProperty.Name == Name)
					{
						yield return jProperty.Value;
					}
				}
				else if (Name == null)
				{
					yield return value;
				}
			}
		}
	}
}
