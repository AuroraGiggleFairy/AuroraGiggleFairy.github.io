using System.Collections.Generic;
using System.IO;
using System.Text;
using Discord.Net.Converters;
using Discord.Net.Rest;
using Newtonsoft.Json;

namespace Discord.API.Rest;

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
internal class CreateWebhookMessageParams
{
	private static JsonSerializer _serializer = new JsonSerializer
	{
		ContractResolver = new DiscordContractResolver()
	};

	[JsonProperty("content")]
	public Optional<string> Content { get; set; }

	[JsonProperty("nonce")]
	public Optional<string> Nonce { get; set; }

	[JsonProperty("tts")]
	public Optional<bool> IsTTS { get; set; }

	[JsonProperty("embeds")]
	public Optional<Embed[]> Embeds { get; set; }

	[JsonProperty("username")]
	public Optional<string> Username { get; set; }

	[JsonProperty("avatar_url")]
	public Optional<string> AvatarUrl { get; set; }

	[JsonProperty("allowed_mentions")]
	public Optional<AllowedMentions> AllowedMentions { get; set; }

	[JsonProperty("flags")]
	public Optional<MessageFlags> Flags { get; set; }

	[JsonProperty("components")]
	public Optional<ActionRowComponent[]> Components { get; set; }

	[JsonProperty("file")]
	public Optional<MultipartFile> File { get; set; }

	public IReadOnlyDictionary<string, object> ToDictionary()
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (File.IsSpecified)
		{
			dictionary["file"] = File.Value;
		}
		Dictionary<string, object> dictionary2 = new Dictionary<string, object> { ["content"] = Content };
		if (IsTTS.IsSpecified)
		{
			dictionary2["tts"] = IsTTS.Value.ToString();
		}
		if (Nonce.IsSpecified)
		{
			dictionary2["nonce"] = Nonce.Value;
		}
		if (Username.IsSpecified)
		{
			dictionary2["username"] = Username.Value;
		}
		if (AvatarUrl.IsSpecified)
		{
			dictionary2["avatar_url"] = AvatarUrl.Value;
		}
		if (Embeds.IsSpecified)
		{
			dictionary2["embeds"] = Embeds.Value;
		}
		if (AllowedMentions.IsSpecified)
		{
			dictionary2["allowed_mentions"] = AllowedMentions.Value;
		}
		if (Components.IsSpecified)
		{
			dictionary2["components"] = Components.Value;
		}
		StringBuilder stringBuilder = new StringBuilder();
		using (StringWriter textWriter = new StringWriter(stringBuilder))
		{
			using JsonTextWriter jsonWriter = new JsonTextWriter(textWriter);
			_serializer.Serialize(jsonWriter, dictionary2);
		}
		dictionary["payload_json"] = stringBuilder.ToString();
		return dictionary;
	}
}
