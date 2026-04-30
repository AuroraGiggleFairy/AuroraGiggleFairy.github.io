using System;
using Discord.Net.Converters;
using Newtonsoft.Json;

namespace Discord.Rest;

internal static class StringExtensions
{
	private static Lazy<JsonSerializerSettings> _settings = new Lazy<JsonSerializerSettings>(() => new JsonSerializerSettings
	{
		ContractResolver = new DiscordContractResolver(),
		Converters = { (JsonConverter)new EmbedTypeConverter() }
	});

	public static string ToJsonString(this EmbedBuilder builder, Formatting formatting = Formatting.Indented)
	{
		return builder.Build().ToJsonString(formatting);
	}

	public static string ToJsonString(this Embed embed, Formatting formatting = Formatting.Indented)
	{
		return JsonConvert.SerializeObject(embed.ToModel(), formatting, _settings.Value);
	}
}
