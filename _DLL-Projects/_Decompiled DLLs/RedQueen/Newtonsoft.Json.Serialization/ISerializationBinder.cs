using System;
using System.Runtime.CompilerServices;

namespace Newtonsoft.Json.Serialization;

[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
internal interface ISerializationBinder
{
	Type BindToType([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] string assemblyName, string typeName);

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
	void BindToName([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(1)] Type serializedType, out string assemblyName, out string typeName);
}
