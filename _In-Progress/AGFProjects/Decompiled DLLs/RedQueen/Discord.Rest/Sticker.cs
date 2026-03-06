using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Discord.API;

namespace Discord.Rest;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class Sticker : RestEntity<ulong>, ISticker, IStickerItem
{
	public ulong PackId { get; protected set; }

	public string Name { get; protected set; }

	public string Description { get; protected set; }

	public IReadOnlyCollection<string> Tags { get; protected set; }

	public StickerType Type { get; protected set; }

	public bool? IsAvailable { get; protected set; }

	public int? SortOrder { get; protected set; }

	public StickerFormatType Format { get; protected set; }

	private string DebuggerDisplay => $"{Name} ({base.Id})";

	public string GetStickerUrl()
	{
		return CDN.GetStickerUrl(base.Id, Format);
	}

	internal Sticker(BaseDiscordClient client, ulong id)
		: base(client, id)
	{
	}

	internal static Sticker Create(BaseDiscordClient client, global::Discord.API.Sticker model)
	{
		Sticker sticker = new Sticker(client, model.Id);
		sticker.Update(model);
		return sticker;
	}

	internal void Update(global::Discord.API.Sticker model)
	{
		PackId = model.PackId;
		Name = model.Name;
		Description = model.Description;
		Tags = (model.Tags.IsSpecified ? (from x in model.Tags.Value.Split(',')
			select x.Trim()).ToArray() : Array.Empty<string>());
		Type = model.Type;
		SortOrder = model.SortValue;
		IsAvailable = model.Available;
		Format = model.FormatType;
	}
}
