using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.CompilerServices;

namespace Newtonsoft.Json.Utilities;

[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
internal class DynamicProxy<[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] T>
{
	public virtual IEnumerable<string> GetDynamicMemberNames(T instance)
	{
		return CollectionUtils.ArrayEmpty<string>();
	}

	public virtual bool TryBinaryOperation(T instance, BinaryOperationBinder binder, object arg, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] out object result)
	{
		result = null;
		return false;
	}

	public virtual bool TryConvert(T instance, ConvertBinder binder, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] out object result)
	{
		result = null;
		return false;
	}

	public virtual bool TryCreateInstance(T instance, CreateInstanceBinder binder, object[] args, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] out object result)
	{
		result = null;
		return false;
	}

	public virtual bool TryDeleteIndex(T instance, DeleteIndexBinder binder, object[] indexes)
	{
		return false;
	}

	public virtual bool TryDeleteMember(T instance, DeleteMemberBinder binder)
	{
		return false;
	}

	public virtual bool TryGetIndex(T instance, GetIndexBinder binder, object[] indexes, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] out object result)
	{
		result = null;
		return false;
	}

	public virtual bool TryGetMember(T instance, GetMemberBinder binder, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] out object result)
	{
		result = null;
		return false;
	}

	public virtual bool TryInvoke(T instance, InvokeBinder binder, object[] args, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] out object result)
	{
		result = null;
		return false;
	}

	public virtual bool TryInvokeMember(T instance, InvokeMemberBinder binder, object[] args, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] out object result)
	{
		result = null;
		return false;
	}

	public virtual bool TrySetIndex(T instance, SetIndexBinder binder, object[] indexes, object value)
	{
		return false;
	}

	public virtual bool TrySetMember(T instance, SetMemberBinder binder, object value)
	{
		return false;
	}

	public virtual bool TryUnaryOperation(T instance, UnaryOperationBinder binder, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] out object result)
	{
		result = null;
		return false;
	}
}
