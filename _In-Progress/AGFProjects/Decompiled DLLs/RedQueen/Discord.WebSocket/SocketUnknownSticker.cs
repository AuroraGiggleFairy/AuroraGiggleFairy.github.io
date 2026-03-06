using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord.API;

namespace Discord.WebSocket;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class SocketUnknownSticker : SocketSticker
{
	public override IReadOnlyCollection<string> Tags => null;

	public override string Description => null;

	public override ulong PackId => 0uL;

	public override bool? IsAvailable => null;

	public override int? SortOrder => null;

	public new StickerType? Type => null;

	private new string DebuggerDisplay => $"{base.Name} ({base.Id})";

	internal SocketUnknownSticker(DiscordSocketClient client, ulong id)
		: base(client, id)
	{
	}

	internal static SocketUnknownSticker Create(DiscordSocketClient client, StickerItem model)
	{
		SocketUnknownSticker socketUnknownSticker = new SocketUnknownSticker(client, model.Id);
		socketUnknownSticker.Update(model);
		return socketUnknownSticker;
	}

	internal void Update(StickerItem model)
	{
		base.Name = model.Name;
		base.Format = model.FormatType;
	}

	public Task<SocketSticker> ResolveAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
	{
		return base.Discord.GetStickerAsync(base.Id, mode, options);
	}
}
