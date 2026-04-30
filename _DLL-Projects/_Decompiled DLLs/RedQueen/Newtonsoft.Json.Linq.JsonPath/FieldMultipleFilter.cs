using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq.JsonPath;

[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
internal class FieldMultipleFilter : PathFilter
{
	internal List<string> Names;

	public FieldMultipleFilter(List<string> names)
	{
		Names = names;
	}

	public override IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] JsonSelectSettings settings)
	{
		foreach (JToken item in current)
		{
			if (item is JObject o)
			{
				foreach (string name in Names)
				{
					JToken jToken = o[name];
					if (jToken != null)
					{
						yield return jToken;
					}
					if (settings?.ErrorWhenNoMatch ?? false)
					{
						throw new JsonException("Property '{0}' does not exist on JObject.".FormatWith(CultureInfo.InvariantCulture, name));
					}
				}
			}
			else if (settings?.ErrorWhenNoMatch ?? false)
			{
				throw new JsonException("Properties {0} not valid on {1}.".FormatWith(CultureInfo.InvariantCulture, string.Join(", ", Names.Select([_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(0)] (string n) => "'" + n + "'")), item.GetType().Name));
			}
		}
	}
}
