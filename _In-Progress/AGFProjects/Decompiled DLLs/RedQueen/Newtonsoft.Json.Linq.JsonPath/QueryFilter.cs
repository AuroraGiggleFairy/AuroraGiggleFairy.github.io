using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Newtonsoft.Json.Linq.JsonPath;

[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
internal class QueryFilter : PathFilter
{
	internal QueryExpression Expression;

	public QueryFilter(QueryExpression expression)
	{
		Expression = expression;
	}

	public override IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] JsonSelectSettings settings)
	{
		foreach (JToken item in current)
		{
			foreach (JToken item2 in (IEnumerable<JToken>)item)
			{
				if (Expression.IsMatch(root, item2, settings))
				{
					yield return item2;
				}
			}
		}
	}
}
