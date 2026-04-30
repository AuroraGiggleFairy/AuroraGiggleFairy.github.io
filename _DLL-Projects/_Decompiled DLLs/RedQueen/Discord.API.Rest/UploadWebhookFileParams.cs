using System.Collections.Generic;
using System.IO;
using System.Text;
using Discord.Net.Converters;
using Discord.Net.Rest;
using Newtonsoft.Json;

namespace Discord.API.Rest;

internal class UploadWebhookFileParams
{
	private static JsonSerializer _serializer = new JsonSerializer
	{
		ContractResolver = new DiscordContractResolver()
	};

	public FileAttachment[] Files { get; }

	public Optional<string> Content { get; set; }

	public Optional<string> Nonce { get; set; }

	public Optional<bool> IsTTS { get; set; }

	public Optional<string> Username { get; set; }

	public Optional<string> AvatarUrl { get; set; }

	public Optional<Embed[]> Embeds { get; set; }

	public Optional<AllowedMentions> AllowedMentions { get; set; }

	public Optional<ActionRowComponent[]> MessageComponents { get; set; }

	public Optional<MessageFlags> Flags { get; set; }

	public UploadWebhookFileParams(params FileAttachment[] files)
	{
		Files = files;
	}

	public IReadOnlyDictionary<string, object> ToDictionary()
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
		if (Content.IsSpecified)
		{
			dictionary2["content"] = Content.Value;
		}
		if (IsTTS.IsSpecified)
		{
			dictionary2["tts"] = IsTTS.Value;
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
		if (MessageComponents.IsSpecified)
		{
			dictionary2["components"] = MessageComponents.Value;
		}
		if (Embeds.IsSpecified)
		{
			dictionary2["embeds"] = Embeds.Value;
		}
		if (AllowedMentions.IsSpecified)
		{
			dictionary2["allowed_mentions"] = AllowedMentions.Value;
		}
		if (Flags.IsSpecified)
		{
			dictionary2["flags"] = Flags.Value;
		}
		List<object> list = new List<object>();
		for (int i = 0; i != Files.Length; i++)
		{
			FileAttachment fileAttachment = Files[i];
			string text = fileAttachment.FileName ?? "unknown.dat";
			if (fileAttachment.IsSpoiler && !text.StartsWith("SPOILER_"))
			{
				text = text.Insert(0, "SPOILER_");
			}
			dictionary[$"files[{i}]"] = new MultipartFile(fileAttachment.Stream, text);
			long id = i;
			string filename = text;
			string description = fileAttachment.Description;
			list.Add(new
			{
				id = (ulong)id,
				filename = filename,
				description = ((description != null) ? ((Optional<string>)description) : Optional<string>.Unspecified)
			});
		}
		dictionary2["attachments"] = list;
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
