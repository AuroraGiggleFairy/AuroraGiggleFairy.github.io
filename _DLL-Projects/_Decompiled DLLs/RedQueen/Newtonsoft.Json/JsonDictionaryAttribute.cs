using System;
using System.Runtime.CompilerServices;

namespace Newtonsoft.Json;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
internal sealed class JsonDictionaryAttribute : JsonContainerAttribute
{
	public JsonDictionaryAttribute()
	{
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
	public JsonDictionaryAttribute(string id)
		: base(id)
	{
	}
}
