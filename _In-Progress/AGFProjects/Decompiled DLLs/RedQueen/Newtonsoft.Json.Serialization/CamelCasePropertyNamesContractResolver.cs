using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization;

[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
internal class CamelCasePropertyNamesContractResolver : DefaultContractResolver
{
	private static readonly object TypeContractCacheLock = new object();

	private static readonly DefaultJsonNameTable NameTable = new DefaultJsonNameTable();

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 0, 1, 1, 1 })]
	private static Dictionary<StructMultiKey<Type, Type>, JsonContract> _contractCache;

	public CamelCasePropertyNamesContractResolver()
	{
		base.NamingStrategy = new CamelCaseNamingStrategy
		{
			ProcessDictionaryKeys = true,
			OverrideSpecifiedNames = true
		};
	}

	public override JsonContract ResolveContract(Type type)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		StructMultiKey<Type, Type> key = new StructMultiKey<Type, Type>(GetType(), type);
		Dictionary<StructMultiKey<Type, Type>, JsonContract> contractCache = _contractCache;
		if (contractCache == null || !contractCache.TryGetValue(key, out var value))
		{
			value = CreateContract(type);
			lock (TypeContractCacheLock)
			{
				contractCache = _contractCache;
				Dictionary<StructMultiKey<Type, Type>, JsonContract> obj = ((contractCache != null) ? new Dictionary<StructMultiKey<Type, Type>, JsonContract>(contractCache) : new Dictionary<StructMultiKey<Type, Type>, JsonContract>());
				obj[key] = value;
				_contractCache = obj;
			}
		}
		return value;
	}

	internal override DefaultJsonNameTable GetNameTable()
	{
		return NameTable;
	}
}
