using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Newtonsoft.Json.Linq.JsonPath;

[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
internal class CompositeExpression : QueryExpression
{
	public List<QueryExpression> Expressions { get; set; }

	public CompositeExpression(QueryOperator @operator)
		: base(@operator)
	{
		Expressions = new List<QueryExpression>();
	}

	public override bool IsMatch(JToken root, JToken t, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] JsonSelectSettings settings)
	{
		switch (Operator)
		{
		case QueryOperator.And:
			foreach (QueryExpression expression in Expressions)
			{
				if (!expression.IsMatch(root, t, settings))
				{
					return false;
				}
			}
			return true;
		case QueryOperator.Or:
			foreach (QueryExpression expression2 in Expressions)
			{
				if (expression2.IsMatch(root, t, settings))
				{
					return true;
				}
			}
			return false;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}
}
