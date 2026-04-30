using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Discord;

internal interface IForumChannel : IGuildChannel, IChannel, ISnowflakeEntity, IEntity<ulong>, IDeletable, IMentionable
{
	bool IsNsfw { get; }

	string Topic { get; }

	ThreadArchiveDuration DefaultAutoArchiveDuration { get; }

	IReadOnlyCollection<ForumTag> Tags { get; }

	Task<IThreadChannel> CreatePostAsync(string title, ThreadArchiveDuration archiveDuration = ThreadArchiveDuration.OneDay, int? slowmode = null, string text = null, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None);

	Task<IThreadChannel> CreatePostWithFileAsync(string title, string filePath, ThreadArchiveDuration archiveDuration = ThreadArchiveDuration.OneDay, int? slowmode = null, string text = null, Embed embed = null, RequestOptions options = null, bool isSpoiler = false, AllowedMentions allowedMentions = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None);

	Task<IThreadChannel> CreatePostWithFileAsync(string title, Stream stream, string filename, ThreadArchiveDuration archiveDuration = ThreadArchiveDuration.OneDay, int? slowmode = null, string text = null, Embed embed = null, RequestOptions options = null, bool isSpoiler = false, AllowedMentions allowedMentions = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None);

	Task<IThreadChannel> CreatePostWithFileAsync(string title, FileAttachment attachment, ThreadArchiveDuration archiveDuration = ThreadArchiveDuration.OneDay, int? slowmode = null, string text = null, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None);

	Task<IThreadChannel> CreatePostWithFilesAsync(string title, IEnumerable<FileAttachment> attachments, ThreadArchiveDuration archiveDuration = ThreadArchiveDuration.OneDay, int? slowmode = null, string text = null, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None);

	Task<IReadOnlyCollection<IThreadChannel>> GetActiveThreadsAsync(RequestOptions options = null);

	Task<IReadOnlyCollection<IThreadChannel>> GetPublicArchivedThreadsAsync(int? limit = null, DateTimeOffset? before = null, RequestOptions options = null);

	Task<IReadOnlyCollection<IThreadChannel>> GetPrivateArchivedThreadsAsync(int? limit = null, DateTimeOffset? before = null, RequestOptions options = null);

	Task<IReadOnlyCollection<IThreadChannel>> GetJoinedPrivateArchivedThreadsAsync(int? limit = null, DateTimeOffset? before = null, RequestOptions options = null);
}
