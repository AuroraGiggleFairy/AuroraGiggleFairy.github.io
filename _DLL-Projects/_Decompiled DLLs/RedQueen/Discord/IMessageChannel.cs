using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Discord;

internal interface IMessageChannel : IChannel, ISnowflakeEntity, IEntity<ulong>
{
	Task<IUserMessage> SendMessageAsync(string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageReference messageReference = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None);

	Task<IUserMessage> SendFileAsync(string filePath, string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, bool isSpoiler = false, AllowedMentions allowedMentions = null, MessageReference messageReference = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None);

	Task<IUserMessage> SendFileAsync(Stream stream, string filename, string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, bool isSpoiler = false, AllowedMentions allowedMentions = null, MessageReference messageReference = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None);

	Task<IUserMessage> SendFileAsync(FileAttachment attachment, string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageReference messageReference = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None);

	Task<IUserMessage> SendFilesAsync(IEnumerable<FileAttachment> attachments, string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageReference messageReference = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None);

	Task<IMessage> GetMessageAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null);

	IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(int limit = 100, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null);

	IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(ulong fromMessageId, Direction dir, int limit = 100, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null);

	IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(IMessage fromMessage, Direction dir, int limit = 100, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null);

	Task<IReadOnlyCollection<IMessage>> GetPinnedMessagesAsync(RequestOptions options = null);

	Task DeleteMessageAsync(ulong messageId, RequestOptions options = null);

	Task DeleteMessageAsync(IMessage message, RequestOptions options = null);

	Task<IUserMessage> ModifyMessageAsync(ulong messageId, Action<MessageProperties> func, RequestOptions options = null);

	Task TriggerTypingAsync(RequestOptions options = null);

	IDisposable EnterTypingState(RequestOptions options = null);
}
