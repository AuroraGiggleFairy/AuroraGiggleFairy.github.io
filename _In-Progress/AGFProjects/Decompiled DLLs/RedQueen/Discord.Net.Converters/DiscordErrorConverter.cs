using System;
using System.Collections.Generic;
using System.Linq;
using Discord.API;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Discord.Net.Converters;

internal class DiscordErrorConverter : JsonConverter
{
	public static DiscordErrorConverter Instance => new DiscordErrorConverter();

	public override bool CanRead => true;

	public override bool CanWrite => false;

	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(DiscordError);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		JObject jObject = JObject.Load(reader);
		Discord.API.DiscordError discordError = new Discord.API.DiscordError();
		JToken value = jObject.GetValue("errors", StringComparison.OrdinalIgnoreCase);
		value?.Parent.Remove();
		using (JsonReader reader2 = jObject.CreateReader())
		{
			serializer.Populate(reader2, discordError);
		}
		if (value != null)
		{
			JsonReader reader3 = value.CreateReader();
			List<ErrorDetails> list = ReadErrors(reader3);
			discordError.Errors = list.ToArray();
		}
		return discordError;
	}

	private List<ErrorDetails> ReadErrors(JsonReader reader, string path = "")
	{
		List<ErrorDetails> list = new List<ErrorDetails>();
		foreach (JProperty item in JObject.Load(reader).Properties())
		{
			int result;
			if (item.Name == "_errors" && path == "")
			{
				list.Add(new ErrorDetails
				{
					Name = Optional<string>.Unspecified,
					Errors = item.Value.ToObject<Discord.API.Error[]>()
				});
			}
			else if (item.Name == "_errors")
			{
				list.Add(new ErrorDetails
				{
					Name = path,
					Errors = item.Value.ToObject<Discord.API.Error[]>()
				});
			}
			else if (int.TryParse(item.Name, out result))
			{
				JsonReader reader2 = item.Value.CreateReader();
				list.AddRange(ReadErrors(reader2, path + $"[{result}]"));
			}
			else
			{
				JsonReader reader3 = item.Value.CreateReader();
				list.AddRange(ReadErrors(reader3, path + ((path != "") ? "." : "") + item.Name[0].ToString().ToUpper() + new string(item.Name.Skip(1).ToArray())));
			}
		}
		return list;
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		throw new NotImplementedException();
	}
}
