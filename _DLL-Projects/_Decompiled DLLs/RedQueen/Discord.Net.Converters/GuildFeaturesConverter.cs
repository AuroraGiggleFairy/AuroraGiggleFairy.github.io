using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Discord.Net.Converters;

internal class GuildFeaturesConverter : JsonConverter
{
	public static GuildFeaturesConverter Instance => new GuildFeaturesConverter();

	public override bool CanWrite => false;

	public override bool CanRead => true;

	public override bool CanConvert(Type objectType)
	{
		return true;
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		string[] array = JToken.Load(reader).ToObject<string[]>();
		GuildFeature guildFeature = GuildFeature.None;
		List<string> list = new List<string>();
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (Enum.TryParse<GuildFeature>(string.Concat(text.Split('_')), ignoreCase: true, out var result))
			{
				guildFeature |= result;
			}
			else
			{
				list.Add(text);
			}
		}
		return new GuildFeatures(guildFeature, list.ToArray());
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		throw new NotImplementedException();
	}
}
