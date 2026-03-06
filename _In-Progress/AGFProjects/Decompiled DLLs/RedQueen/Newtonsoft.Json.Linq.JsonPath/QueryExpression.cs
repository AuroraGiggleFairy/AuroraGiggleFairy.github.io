using System.Runtime.CompilerServices;

namespace Newtonsoft.Json.Linq.JsonPath;

[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
internal abstract class QueryExpression
{
	internal QueryOperator Operator;

	public QueryExpression(QueryOperator @operator)
	{
		Operator = @operator;
	}

	public bool IsMatch(JToken root, JToken t)
	{
		return IsMatch(root, t, null);
	}

	public abstract bool IsMatch(JToken root, JToken t, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] JsonSelectSettings settings);
}
