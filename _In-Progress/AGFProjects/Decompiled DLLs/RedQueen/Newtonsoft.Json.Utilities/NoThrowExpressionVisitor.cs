using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Newtonsoft.Json.Utilities;

[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
internal class NoThrowExpressionVisitor : ExpressionVisitor
{
	internal static readonly object ErrorResult = new object();

	protected override Expression VisitConditional(ConditionalExpression node)
	{
		if (node.IfFalse.NodeType == ExpressionType.Throw)
		{
			return Expression.Condition(node.Test, node.IfTrue, Expression.Constant(ErrorResult));
		}
		return base.VisitConditional(node);
	}
}
