using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization;

[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
internal class JsonPrimitiveContract : JsonContract
{
	private static readonly Dictionary<Type, ReadType> ReadTypeMap = new Dictionary<Type, ReadType>
	{
		[typeof(byte[])] = ReadType.ReadAsBytes,
		[typeof(byte)] = ReadType.ReadAsInt32,
		[typeof(short)] = ReadType.ReadAsInt32,
		[typeof(int)] = ReadType.ReadAsInt32,
		[typeof(decimal)] = ReadType.ReadAsDecimal,
		[typeof(bool)] = ReadType.ReadAsBoolean,
		[typeof(string)] = ReadType.ReadAsString,
		[typeof(DateTime)] = ReadType.ReadAsDateTime,
		[typeof(DateTimeOffset)] = ReadType.ReadAsDateTimeOffset,
		[typeof(float)] = ReadType.ReadAsDouble,
		[typeof(double)] = ReadType.ReadAsDouble,
		[typeof(long)] = ReadType.ReadAsInt64
	};

	internal PrimitiveTypeCode TypeCode { get; set; }

	public JsonPrimitiveContract(Type underlyingType)
		: base(underlyingType)
	{
		ContractType = JsonContractType.Primitive;
		TypeCode = ConvertUtils.GetTypeCode(underlyingType);
		IsReadOnlyOrFixedSize = true;
		if (ReadTypeMap.TryGetValue(NonNullableUnderlyingType, out var value))
		{
			InternalReadType = value;
		}
	}
}
