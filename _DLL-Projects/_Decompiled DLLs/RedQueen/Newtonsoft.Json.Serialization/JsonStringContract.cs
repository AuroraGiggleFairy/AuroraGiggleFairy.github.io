using System;
using System.Runtime.CompilerServices;

namespace Newtonsoft.Json.Serialization;

internal class JsonStringContract : JsonPrimitiveContract
{
	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
	public JsonStringContract(Type underlyingType)
		: base(underlyingType)
	{
		ContractType = JsonContractType.String;
	}
}
