using System;
using System.Runtime.CompilerServices;

namespace Newtonsoft.Json.Converters;

internal abstract class DateTimeConverterBase : JsonConverter
{
	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
	public override bool CanConvert(Type objectType)
	{
		if (objectType == typeof(DateTime) || objectType == typeof(DateTime?))
		{
			return true;
		}
		if (objectType == typeof(DateTimeOffset) || objectType == typeof(DateTimeOffset?))
		{
			return true;
		}
		return false;
	}
}
