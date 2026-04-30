using System.Globalization;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization;

[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
internal class DefaultReferenceResolver : IReferenceResolver
{
	private int _referenceCount;

	private BidirectionalDictionary<string, object> GetMappings(object context)
	{
		JsonSerializerInternalBase jsonSerializerInternalBase = context as JsonSerializerInternalBase;
		if (jsonSerializerInternalBase == null)
		{
			if (!(context is JsonSerializerProxy jsonSerializerProxy))
			{
				throw new JsonException("The DefaultReferenceResolver can only be used internally.");
			}
			jsonSerializerInternalBase = jsonSerializerProxy.GetInternalSerializer();
		}
		return jsonSerializerInternalBase.DefaultReferenceMappings;
	}

	public object ResolveReference(object context, string reference)
	{
		GetMappings(context).TryGetByFirst(reference, out var second);
		return second;
	}

	public string GetReference(object context, object value)
	{
		BidirectionalDictionary<string, object> mappings = GetMappings(context);
		if (!mappings.TryGetBySecond(value, out var first))
		{
			_referenceCount++;
			first = _referenceCount.ToString(CultureInfo.InvariantCulture);
			mappings.Set(first, value);
		}
		return first;
	}

	public void AddReference(object context, string reference, object value)
	{
		GetMappings(context).Set(reference, value);
	}

	public bool IsReferenced(object context, object value)
	{
		string first;
		return GetMappings(context).TryGetBySecond(value, out first);
	}
}
