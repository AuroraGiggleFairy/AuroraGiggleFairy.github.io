using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Discord.Rest;

internal interface IRestMessageChannel : IMessageChannel, IChannel, ISnowflakeEntity, IEntity<ulong>
{
	new Task<RestUserMessage> SendMessageAsync(string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageReference messageReference = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None);

	new Task<RestUserMessage> SendFileAsync(string filePath, string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, bool isSpoiler = false, AllowedMentions allowedMentions = null, MessageReference messageReference = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None);

	new Task<RestUserMessage> SendFileAsync(Stream stream, string filename, string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, bool isSpoiler = false, AllowedMentions allowedMentions = null, MessageReference messageReference = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None);

	Task<RestMessage> GetMessageAsync(ulong id, RequestOptions options = null);

	IAsyncEnumerable<IReadOnlyCollection<RestMessage>> GetMessagesAsync(int limit = 100, RequestOptions options = null);

	IAsyncEnumerable<IReadOnlyCollection<RestMessage>> GetMessagesAsync(ulong fromMessageId, Direction dir, int limit = 100, RequestOptions options = null);

	IAsyncEnumerable<IReadOnlyCollection<RestMessage>> GetMessagesAsync(IMessage fromMessage, Direction dir, int limit = 100, RequestOptions options = null);

	new Task<IReadOnlyCollection<RestMessage>> GetPinnedMessagesAsync(RequestOptions options = null);
}
