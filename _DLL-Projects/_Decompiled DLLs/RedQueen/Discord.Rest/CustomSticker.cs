using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord.API;

namespace Discord.Rest;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class CustomSticker : Sticker, ICustomSticker, ISticker, IStickerItem
{
	public ulong? AuthorId { get; private set; }

	public RestGuild Guild { get; private set; }

	private ulong GuildId { get; set; }

	private string DebuggerDisplay
	{
		get
		{
			if (Guild == null)
			{
				return $"{base.Name} ({base.Id})";
			}
			return $"{base.Name} in {Guild.Name} ({base.Id})";
		}
	}

	IGuild ICustomSticker.Guild => Guild;

	internal CustomSticker(BaseDiscordClient client, ulong id, RestGuild guild, ulong? authorId = null)
		: base(client, id)
	{
		AuthorId = authorId;
		Guild = guild;
	}

	internal CustomSticker(BaseDiscordClient client, ulong id, ulong guildId, ulong? authorId = null)
		: base(client, id)
	{
		AuthorId = authorId;
		GuildId = guildId;
	}

	internal static CustomSticker Create(BaseDiscordClient client, global::Discord.API.Sticker model, RestGuild guild, ulong? authorId = null)
	{
		CustomSticker customSticker = new CustomSticker(client, model.Id, guild, authorId);
		customSticker.Update(model);
		return customSticker;
	}

	internal static CustomSticker Create(BaseDiscordClient client, global::Discord.API.Sticker model, ulong guildId, ulong? authorId = null)
	{
		CustomSticker customSticker = new CustomSticker(client, model.Id, guildId, authorId);
		customSticker.Update(model);
		return customSticker;
	}

	public Task DeleteAsync(RequestOptions options = null)
	{
		return GuildHelper.DeleteStickerAsync(base.Discord, GuildId, this, options);
	}

	public async Task ModifyAsync(Action<StickerProperties> func, RequestOptions options = null)
	{
		Update(await GuildHelper.ModifyStickerAsync(base.Discord, GuildId, this, func, options));
	}
}
