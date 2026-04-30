using System.Dynamic;
using System.Runtime.CompilerServices;

namespace Newtonsoft.Json.Utilities;

[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
internal class NoThrowGetBinderMember : GetMemberBinder
{
	private readonly GetMemberBinder _innerBinder;

	public NoThrowGetBinderMember(GetMemberBinder innerBinder)
		: base(innerBinder.Name, innerBinder.IgnoreCase)
	{
		_innerBinder = innerBinder;
	}

	public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] DynamicMetaObject errorSuggestion)
	{
		DynamicMetaObject dynamicMetaObject = _innerBinder.Bind(target, CollectionUtils.ArrayEmpty<DynamicMetaObject>());
		return new DynamicMetaObject(new NoThrowExpressionVisitor().Visit(dynamicMetaObject.Expression), dynamicMetaObject.Restrictions);
	}
}
