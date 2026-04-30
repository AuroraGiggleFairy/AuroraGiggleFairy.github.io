using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq.JsonPath;

internal class ArrayIndexFilter : PathFilter
{
	public int? Index { get; set; }

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
	public override IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] JsonSelectSettings settings)
	{
		foreach (JToken item in current)
		{
			if (Index.HasValue)
			{
				JToken tokenIndex = PathFilter.GetTokenIndex(item, settings, Index.GetValueOrDefault());
				if (tokenIndex != null)
				{
					yield return tokenIndex;
				}
			}
			else if (item is JArray || item is JConstructor)
			{
				foreach (JToken item2 in (IEnumerable<JToken>)item)
				{
					yield return item2;
				}
			}
			else if (settings?.ErrorWhenNoMatch ?? false)
			{
				throw new JsonException("Index * not valid on {0}.".FormatWith(CultureInfo.InvariantCulture, item.GetType().Name));
			}
		}
	}
}
