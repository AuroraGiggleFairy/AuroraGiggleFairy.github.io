using System.Collections.Generic;
using System.Collections.Immutable;

namespace Discord;

internal class StickerPack<TSticker> where TSticker : ISticker
{
	public ulong Id { get; }

	public IReadOnlyCollection<TSticker> Stickers { get; }

	public string Name { get; }

	public ulong SkuId { get; }

	public ulong? CoverStickerId { get; }

	public string Description { get; }

	public ulong BannerAssetId { get; }

	internal StickerPack(string name, ulong id, ulong skuid, ulong? coverStickerId, string description, ulong bannerAssetId, IEnumerable<TSticker> stickers)
	{
		Name = name;
		Id = id;
		SkuId = skuid;
		CoverStickerId = coverStickerId;
		Description = description;
		BannerAssetId = bannerAssetId;
		Stickers = stickers.ToImmutableArray();
	}
}
