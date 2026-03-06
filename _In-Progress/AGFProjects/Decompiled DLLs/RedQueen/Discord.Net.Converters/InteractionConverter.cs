using System;
using Discord.API;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Discord.Net.Converters;

internal class InteractionConverter : JsonConverter
{
	public static InteractionConverter Instance => new InteractionConverter();

	public override bool CanRead => true;

	public override bool CanWrite => false;

	public override bool CanConvert(Type objectType)
	{
		return true;
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		if (reader.TokenType == JsonToken.Null)
		{
			return null;
		}
		JObject jObject = JObject.Load(reader);
		Interaction interaction = new Interaction();
		JToken value = jObject.GetValue("data", StringComparison.OrdinalIgnoreCase);
		value?.Parent.Remove();
		using (JsonReader reader2 = jObject.CreateReader())
		{
			serializer.Populate(reader2, interaction);
		}
		if (value != null)
		{
			switch (interaction.Type)
			{
			case InteractionType.ApplicationCommand:
			{
				ApplicationCommandInteractionData applicationCommandInteractionData = new ApplicationCommandInteractionData();
				serializer.Populate(value.CreateReader(), applicationCommandInteractionData);
				interaction.Data = applicationCommandInteractionData;
				break;
			}
			case InteractionType.MessageComponent:
			{
				MessageComponentInteractionData messageComponentInteractionData = new MessageComponentInteractionData();
				serializer.Populate(value.CreateReader(), messageComponentInteractionData);
				interaction.Data = messageComponentInteractionData;
				break;
			}
			case InteractionType.ApplicationCommandAutocomplete:
			{
				AutocompleteInteractionData autocompleteInteractionData = new AutocompleteInteractionData();
				serializer.Populate(value.CreateReader(), autocompleteInteractionData);
				interaction.Data = autocompleteInteractionData;
				break;
			}
			case InteractionType.ModalSubmit:
			{
				ModalInteractionData modalInteractionData = new ModalInteractionData();
				serializer.Populate(value.CreateReader(), modalInteractionData);
				interaction.Data = modalInteractionData;
				break;
			}
			}
		}
		else
		{
			interaction.Data = Optional<IDiscordInteractionData>.Unspecified;
		}
		return interaction;
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		throw new NotImplementedException();
	}
}
