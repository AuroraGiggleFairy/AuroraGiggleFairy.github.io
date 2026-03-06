using System;
using Discord.API;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Discord.Net.Converters;

internal class MessageComponentConverter : JsonConverter
{
	public static MessageComponentConverter Instance => new MessageComponentConverter();

	public override bool CanRead => true;

	public override bool CanWrite => false;

	public override bool CanConvert(Type objectType)
	{
		return true;
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		serializer.Serialize(writer, value);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		JObject jObject = JObject.Load(reader);
		IMessageComponent messageComponent = null;
		switch ((ComponentType)jObject["type"].Value<int>())
		{
		case ComponentType.ActionRow:
			messageComponent = new Discord.API.ActionRowComponent();
			break;
		case ComponentType.Button:
			messageComponent = new Discord.API.ButtonComponent();
			break;
		case ComponentType.SelectMenu:
			messageComponent = new Discord.API.SelectMenuComponent();
			break;
		case ComponentType.TextInput:
			messageComponent = new Discord.API.TextInputComponent();
			break;
		}
		serializer.Populate(jObject.CreateReader(), messageComponent);
		return messageComponent;
	}
}
