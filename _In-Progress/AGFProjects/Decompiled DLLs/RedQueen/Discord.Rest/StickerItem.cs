using System.Threading.Tasks;
using Discord.API;

namespace Discord.Rest;

internal class StickerItem : RestEntity<ulong>, IStickerItem
{
	public string Name { get; }

	public StickerFormatType Format { get; }

	internal StickerItem(BaseDiscordClient client, global::Discord.API.StickerItem model)
		: base(client, model.Id)
	{
		Name = model.Name;
		Format = model.FormatType;
	}

	public async Task<Sticker> ResolveStickerAsync()
	{
		global::Discord.API.Sticker sticker = await base.Discord.ApiClient.GetStickerAsync(base.Id);
		return sticker.GuildId.IsSpecified ? CustomSticker.Create(base.Discord, sticker, sticker.GuildId.Value, sticker.User.IsSpecified ? new ulong?(sticker.User.Value.Id) : ((ulong?)null)) : Sticker.Create(base.Discord, sticker);
	}
}
