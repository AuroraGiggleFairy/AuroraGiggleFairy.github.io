using System;
using System.Dynamic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization;

[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
internal class JsonDynamicContract : JsonContainerContract
{
	private readonly ThreadSafeStore<string, CallSite<Func<CallSite, object, object>>> _callSiteGetters = new ThreadSafeStore<string, CallSite<Func<CallSite, object, object>>>(CreateCallSiteGetter);

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 1, 1, 1, 1, 1, 1, 2, 1 })]
	private readonly ThreadSafeStore<string, CallSite<Func<CallSite, object, object, object>>> _callSiteSetters = new ThreadSafeStore<string, CallSite<Func<CallSite, object, object, object>>>(CreateCallSiteSetter);

	public JsonPropertyCollection Properties { get; }

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1, 1 })]
	[field: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1, 1 })]
	public Func<string, string> PropertyNameResolver
	{
		[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1, 1 })]
		get;
		[param: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1, 1 })]
		set;
	}

	private static CallSite<Func<CallSite, object, object>> CreateCallSiteGetter(string name)
	{
		return CallSite<Func<CallSite, object, object>>.Create(new NoThrowGetBinderMember((GetMemberBinder)DynamicUtils.BinderWrapper.GetMember(name, typeof(DynamicUtils))));
	}

	[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 1, 1, 1, 1, 2, 1 })]
	private static CallSite<Func<CallSite, object, object, object>> CreateCallSiteSetter(string name)
	{
		return CallSite<Func<CallSite, object, object, object>>.Create(new NoThrowSetBinderMember((SetMemberBinder)DynamicUtils.BinderWrapper.SetMember(name, typeof(DynamicUtils))));
	}

	public JsonDynamicContract(Type underlyingType)
		: base(underlyingType)
	{
		ContractType = JsonContractType.Dynamic;
		Properties = new JsonPropertyCollection(base.UnderlyingType);
	}

	internal bool TryGetMember(IDynamicMetaObjectProvider dynamicProvider, string name, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] out object value)
	{
		ValidationUtils.ArgumentNotNull(dynamicProvider, "dynamicProvider");
		CallSite<Func<CallSite, object, object>> callSite = _callSiteGetters.Get(name);
		object obj = callSite.Target(callSite, dynamicProvider);
		if (obj != NoThrowExpressionVisitor.ErrorResult)
		{
			value = obj;
			return true;
		}
		value = null;
		return false;
	}

	internal bool TrySetMember(IDynamicMetaObjectProvider dynamicProvider, string name, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] object value)
	{
		ValidationUtils.ArgumentNotNull(dynamicProvider, "dynamicProvider");
		CallSite<Func<CallSite, object, object, object>> callSite = _callSiteSetters.Get(name);
		return callSite.Target(callSite, dynamicProvider, value) != NoThrowExpressionVisitor.ErrorResult;
	}
}
