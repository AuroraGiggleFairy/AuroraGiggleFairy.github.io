using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord.API;
using Discord.Rest;

namespace Discord.WebSocket;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class SocketCustomSticker : SocketSticker, ICustomSticker, ISticker, IStickerItem
{
	public SocketGuildUser Author
	{
		get
		{
			if (!AuthorId.HasValue)
			{
				return null;
			}
			return Guild.GetUser(AuthorId.Value);
		}
	}

	public SocketGuild Guild { get; }

	public ulong? AuthorId { get; set; }

	private new string DebuggerDisplay
	{
		get
		{
			if (Guild != null)
			{
				return $"{base.Name} in {Guild.Name} ({base.Id})";
			}
			return base.DebuggerDisplay;
		}
	}

	ulong? ICustomSticker.AuthorId => AuthorId;

	IGuild ICustomSticker.Guild => Guild;

	internal SocketCustomSticker(DiscordSocketClient client, ulong id, SocketGuild guild, ulong? authorId = null)
		: base(client, id)
	{
		Guild = guild;
		AuthorId = authorId;
	}

	internal static SocketCustomSticker Create(DiscordSocketClient client, global::Discord.API.Sticker model, SocketGuild guild, ulong? authorId = null)
	{
		SocketCustomSticker socketCustomSticker = new SocketCustomSticker(client, model.Id, guild, authorId);
		socketCustomSticker.Update(model);
		return socketCustomSticker;
	}

	public async Task ModifyAsync(Action<StickerProperties> func, RequestOptions options = null)
	{
		if (!Guild.CurrentUser.GuildPermissions.Has(GuildPermission.ManageEmojisAndStickers))
		{
			throw new InvalidOperationException("Missing permission ManageEmojisAndStickers");
		}
		Update(await GuildHelper.ModifyStickerAsync(base.Discord, Guild.Id, this, func, options));
	}

	public async Task DeleteAsync(RequestOptions options = null)
	{
		await GuildHelper.DeleteStickerAsync(base.Discord, Guild.Id, this, options);
		Guild.RemoveSticker(base.Id);
	}

	internal SocketCustomSticker Clone()
	{
		return MemberwiseClone() as SocketCustomSticker;
	}
}
