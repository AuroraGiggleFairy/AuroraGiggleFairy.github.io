using System.Collections.Generic;
using System.IO;
using System.Text;
using Discord.Net.Converters;
using Discord.Net.Rest;
using Newtonsoft.Json;

namespace Discord.API.Rest;

internal class CreateMultipartPostAsync
{
	private static JsonSerializer _serializer = new JsonSerializer
	{
		ContractResolver = new DiscordContractResolver()
	};

	public FileAttachment[] Files { get; }

	public string Title { get; set; }

	public ThreadArchiveDuration ArchiveDuration { get; set; }

	public Optional<int?> Slowmode { get; set; }

	public Optional<string> Content { get; set; }

	public Optional<Embed[]> Embeds { get; set; }

	public Optional<AllowedMentions> AllowedMentions { get; set; }

	public Optional<ActionRowComponent[]> MessageComponent { get; set; }

	public Optional<MessageFlags?> Flags { get; set; }

	public Optional<ulong[]> Stickers { get; set; }

	public CreateMultipartPostAsync(params FileAttachment[] attachments)
	{
		Files = attachments;
	}

	public IReadOnlyDictionary<string, object> ToDictionary()
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
		Dictionary<string, object> dictionary3 = new Dictionary<string, object>();
		dictionary2["name"] = Title;
		dictionary2["auto_archive_duration"] = ArchiveDuration;
		if (Slowmode.IsSpecified)
		{
			dictionary2["rate_limit_per_user"] = Slowmode.Value;
		}
		if (Content.IsSpecified)
		{
			dictionary3["content"] = Content.Value;
		}
		if (Embeds.IsSpecified)
		{
			dictionary3["embeds"] = Embeds.Value;
		}
		if (AllowedMentions.IsSpecified)
		{
			dictionary3["allowed_mentions"] = AllowedMentions.Value;
		}
		if (MessageComponent.IsSpecified)
		{
			dictionary3["components"] = MessageComponent.Value;
		}
		if (Stickers.IsSpecified)
		{
			dictionary3["sticker_ids"] = Stickers.Value;
		}
		if (Flags.IsSpecified)
		{
			dictionary3["flags"] = Flags.Value;
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
		dictionary3["attachments"] = list;
		dictionary2["message"] = dictionary3;
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
