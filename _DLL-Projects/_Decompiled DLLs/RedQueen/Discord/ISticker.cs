using System.Collections.Generic;

namespace Discord;

internal interface ISticker : IStickerItem
{
	new ulong Id { get; }

	ulong PackId { get; }

	new string Name { get; }

	string Description { get; }

	IReadOnlyCollection<string> Tags { get; }

	StickerType Type { get; }

	new StickerFormatType Format { get; }

	bool? IsAvailable { get; }

	int? SortOrder { get; }

	string GetStickerUrl();
}
