using System;
using System.Threading.Tasks;

namespace Discord;

internal interface ICustomSticker : ISticker, IStickerItem
{
	ulong? AuthorId { get; }

	IGuild Guild { get; }

	Task ModifyAsync(Action<StickerProperties> func, RequestOptions options = null);

	Task DeleteAsync(RequestOptions options = null);
}
