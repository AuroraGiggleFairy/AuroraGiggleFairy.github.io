using System.Collections.Generic;
using Newtonsoft.Json;

namespace Discord.API;

internal class NitroStickerPacks
{
	[JsonProperty("sticker_packs")]
	public List<StickerPack> StickerPacks { get; set; }
}
