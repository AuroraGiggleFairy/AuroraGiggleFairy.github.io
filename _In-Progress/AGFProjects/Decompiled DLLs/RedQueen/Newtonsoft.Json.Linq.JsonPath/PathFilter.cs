using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq.JsonPath;

[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
internal abstract class PathFilter
{
	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
	public abstract IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] JsonSelectSettings settings);

	protected static JToken GetTokenIndex([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(1)] JToken t, JsonSelectSettings settings, int index)
	{
		if (t is JArray jArray)
		{
			if (jArray.Count <= index)
			{
				if (settings != null && settings.ErrorWhenNoMatch)
				{
					throw new JsonException("Index {0} outside the bounds of JArray.".FormatWith(CultureInfo.InvariantCulture, index));
				}
				return null;
			}
			return jArray[index];
		}
		if (t is JConstructor jConstructor)
		{
			if (jConstructor.Count <= index)
			{
				if (settings != null && settings.ErrorWhenNoMatch)
				{
					throw new JsonException("Index {0} outside the bounds of JConstructor.".FormatWith(CultureInfo.InvariantCulture, index));
				}
				return null;
			}
			return jConstructor[index];
		}
		if (settings != null && settings.ErrorWhenNoMatch)
		{
			throw new JsonException("Index {0} not valid on {1}.".FormatWith(CultureInfo.InvariantCulture, index, t.GetType().Name));
		}
		return null;
	}

	protected static JToken GetNextScanValue([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(1)] JToken originalParent, JToken container, JToken value)
	{
		if (container != null && container.HasValues)
		{
			value = container.First;
		}
		else
		{
			while (value != null && value != originalParent && value == value.Parent.Last)
			{
				value = value.Parent;
			}
			if (value == null || value == originalParent)
			{
				return null;
			}
			value = value.Next;
		}
		return value;
	}
}
