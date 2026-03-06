using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Discord.API;

namespace Discord.WebSocket;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class SocketSticker : SocketEntity<ulong>, ISticker, IStickerItem
{
	public virtual ulong PackId { get; private set; }

	public string Name { get; protected set; }

	public virtual string Description { get; private set; }

	public virtual IReadOnlyCollection<string> Tags { get; private set; }

	public virtual StickerType Type { get; private set; }

	public StickerFormatType Format { get; protected set; }

	public virtual bool? IsAvailable { get; protected set; }

	public virtual int? SortOrder { get; private set; }

	internal string DebuggerDisplay => $"{Name} ({base.Id})";

	public string GetStickerUrl()
	{
		return CDN.GetStickerUrl(base.Id, Format);
	}

	internal SocketSticker(DiscordSocketClient client, ulong id)
		: base(client, id)
	{
	}

	internal static SocketSticker Create(DiscordSocketClient client, Sticker model)
	{
		SocketSticker obj = (model.GuildId.IsSpecified ? new SocketCustomSticker(client, model.Id, client.GetGuild(model.GuildId.Value), model.User.IsSpecified ? new ulong?(model.User.Value.Id) : ((ulong?)null)) : new SocketSticker(client, model.Id));
		obj.Update(model);
		return obj;
	}

	internal virtual void Update(Sticker model)
	{
		Name = model.Name;
		Description = model.Description;
		PackId = model.PackId;
		IsAvailable = model.Available;
		Format = model.FormatType;
		Type = model.Type;
		SortOrder = model.SortValue;
		Tags = (model.Tags.IsSpecified ? (from x in model.Tags.Value.Split(',')
			select x.Trim()).ToImmutableArray() : System.Collections.Immutable.ImmutableArray.Create<string>());
	}

	public override bool Equals(object obj)
	{
		if (obj is Sticker sticker)
		{
			if (sticker.Name == Name && sticker.Description == Description && sticker.FormatType == Format && sticker.Id == base.Id && sticker.PackId == PackId && sticker.Type == Type && sticker.SortValue == SortOrder && sticker.Available == IsAvailable)
			{
				if (sticker.Tags.IsSpecified)
				{
					return sticker.Tags.Value == string.Join(", ", Tags);
				}
				return true;
			}
			return false;
		}
		return base.Equals(obj);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
