using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord.API;

namespace Discord.Rest;

internal class RestForumChannel : RestGuildChannel, IForumChannel, IGuildChannel, IChannel, ISnowflakeEntity, IEntity<ulong>, IDeletable, IMentionable
{
	public bool IsNsfw { get; private set; }

	public string Topic { get; private set; }

	public ThreadArchiveDuration DefaultAutoArchiveDuration { get; private set; }

	public IReadOnlyCollection<ForumTag> Tags { get; private set; }

	public string Mention => MentionUtils.MentionChannel(base.Id);

	internal RestForumChannel(BaseDiscordClient client, IGuild guild, ulong id)
		: base(client, guild, id)
	{
	}

	internal new static RestStageChannel Create(BaseDiscordClient discord, IGuild guild, Channel model)
	{
		RestStageChannel restStageChannel = new RestStageChannel(discord, guild, model.Id);
		restStageChannel.Update(model);
		return restStageChannel;
	}

	internal override void Update(Channel model)
	{
		base.Update(model);
		IsNsfw = model.Nsfw.GetValueOrDefault(defaultValue: false);
		Topic = model.Topic.GetValueOrDefault();
		DefaultAutoArchiveDuration = model.AutoArchiveDuration.GetValueOrDefault(ThreadArchiveDuration.OneDay);
		Tags = (from x in model.ForumTags.GetValueOrDefault(Array.Empty<ForumTags>())
			select new ForumTag(x.Id, x.Name, x.EmojiId.GetValueOrDefault(null), x.EmojiName.GetValueOrDefault())).ToImmutableArray();
	}

	public Task<RestThreadChannel> CreatePostAsync(string title, ThreadArchiveDuration archiveDuration = ThreadArchiveDuration.OneDay, int? slowmode = null, string text = null, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None)
	{
		return ThreadHelper.CreatePostAsync(this, base.Discord, title, archiveDuration, slowmode, text, embed, options, allowedMentions, components, stickers, embeds, flags);
	}

	public async Task<RestThreadChannel> CreatePostWithFileAsync(string title, string filePath, ThreadArchiveDuration archiveDuration = ThreadArchiveDuration.OneDay, int? slowmode = null, string text = null, Embed embed = null, RequestOptions options = null, bool isSpoiler = false, AllowedMentions allowedMentions = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None)
	{
		using FileAttachment file = new FileAttachment(filePath, null, null, isSpoiler);
		return await ThreadHelper.CreatePostAsync(this, base.Discord, title, new FileAttachment[1] { file }, archiveDuration, slowmode, text, embed, options, allowedMentions, components, stickers, embeds, flags).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<RestThreadChannel> CreatePostWithFileAsync(string title, Stream stream, string filename, ThreadArchiveDuration archiveDuration = ThreadArchiveDuration.OneDay, int? slowmode = null, string text = null, Embed embed = null, RequestOptions options = null, bool isSpoiler = false, AllowedMentions allowedMentions = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None)
	{
		using FileAttachment file = new FileAttachment(stream, filename, null, isSpoiler);
		return await ThreadHelper.CreatePostAsync(this, base.Discord, title, new FileAttachment[1] { file }, archiveDuration, slowmode, text, embed, options, allowedMentions, components, stickers, embeds, flags).ConfigureAwait(continueOnCapturedContext: false);
	}

	public Task<RestThreadChannel> CreatePostWithFileAsync(string title, FileAttachment attachment, ThreadArchiveDuration archiveDuration = ThreadArchiveDuration.OneDay, int? slowmode = null, string text = null, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None)
	{
		return ThreadHelper.CreatePostAsync(this, base.Discord, title, new FileAttachment[1] { attachment }, archiveDuration, slowmode, text, embed, options, allowedMentions, components, stickers, embeds, flags);
	}

	public Task<RestThreadChannel> CreatePostWithFilesAsync(string title, IEnumerable<FileAttachment> attachments, ThreadArchiveDuration archiveDuration = ThreadArchiveDuration.OneDay, int? slowmode = null, string text = null, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None)
	{
		return ThreadHelper.CreatePostAsync(this, base.Discord, title, attachments, archiveDuration, slowmode, text, embed, options, allowedMentions, components, stickers, embeds, flags);
	}

	public Task<IReadOnlyCollection<RestThreadChannel>> GetActiveThreadsAsync(RequestOptions options = null)
	{
		return ThreadHelper.GetActiveThreadsAsync(base.Guild, base.Discord, options);
	}

	public Task<IReadOnlyCollection<RestThreadChannel>> GetJoinedPrivateArchivedThreadsAsync(int? limit = null, DateTimeOffset? before = null, RequestOptions options = null)
	{
		return ThreadHelper.GetJoinedPrivateArchivedThreadsAsync(this, base.Discord, limit, before, options);
	}

	public Task<IReadOnlyCollection<RestThreadChannel>> GetPrivateArchivedThreadsAsync(int? limit = null, DateTimeOffset? before = null, RequestOptions options = null)
	{
		return ThreadHelper.GetPrivateArchivedThreadsAsync(this, base.Discord, limit, before, options);
	}

	public Task<IReadOnlyCollection<RestThreadChannel>> GetPublicArchivedThreadsAsync(int? limit = null, DateTimeOffset? before = null, RequestOptions options = null)
	{
		return ThreadHelper.GetPublicArchivedThreadsAsync(this, base.Discord, limit, before, options);
	}

	async Task<IReadOnlyCollection<IThreadChannel>> IForumChannel.GetActiveThreadsAsync(RequestOptions options)
	{
		return await GetActiveThreadsAsync(options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IReadOnlyCollection<IThreadChannel>> IForumChannel.GetPublicArchivedThreadsAsync(int? limit, DateTimeOffset? before, RequestOptions options)
	{
		return await GetPublicArchivedThreadsAsync(limit, before, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IReadOnlyCollection<IThreadChannel>> IForumChannel.GetPrivateArchivedThreadsAsync(int? limit, DateTimeOffset? before, RequestOptions options)
	{
		return await GetPrivateArchivedThreadsAsync(limit, before, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IReadOnlyCollection<IThreadChannel>> IForumChannel.GetJoinedPrivateArchivedThreadsAsync(int? limit, DateTimeOffset? before, RequestOptions options)
	{
		return await GetJoinedPrivateArchivedThreadsAsync(limit, before, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IThreadChannel> IForumChannel.CreatePostAsync(string title, ThreadArchiveDuration archiveDuration, int? slowmode, string text, Embed embed, RequestOptions options, AllowedMentions allowedMentions, MessageComponent components, ISticker[] stickers, Embed[] embeds, MessageFlags flags)
	{
		return await CreatePostAsync(title, archiveDuration, slowmode, text, embed, options, allowedMentions, components, stickers, embeds, flags).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IThreadChannel> IForumChannel.CreatePostWithFileAsync(string title, string filePath, ThreadArchiveDuration archiveDuration, int? slowmode, string text, Embed embed, RequestOptions options, bool isSpoiler, AllowedMentions allowedMentions, MessageComponent components, ISticker[] stickers, Embed[] embeds, MessageFlags flags)
	{
		return await CreatePostWithFileAsync(title, filePath, archiveDuration, slowmode, text, embed, options, isSpoiler, allowedMentions, components, stickers, embeds, flags).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IThreadChannel> IForumChannel.CreatePostWithFileAsync(string title, Stream stream, string filename, ThreadArchiveDuration archiveDuration, int? slowmode, string text, Embed embed, RequestOptions options, bool isSpoiler, AllowedMentions allowedMentions, MessageComponent components, ISticker[] stickers, Embed[] embeds, MessageFlags flags)
	{
		return await CreatePostWithFileAsync(title, stream, filename, archiveDuration, slowmode, text, embed, options, isSpoiler, allowedMentions, components, stickers, embeds, flags).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IThreadChannel> IForumChannel.CreatePostWithFileAsync(string title, FileAttachment attachment, ThreadArchiveDuration archiveDuration, int? slowmode, string text, Embed embed, RequestOptions options, AllowedMentions allowedMentions, MessageComponent components, ISticker[] stickers, Embed[] embeds, MessageFlags flags)
	{
		return await CreatePostWithFileAsync(title, attachment, archiveDuration, slowmode, text, embed, options, allowedMentions, components, stickers, embeds, flags).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IThreadChannel> IForumChannel.CreatePostWithFilesAsync(string title, IEnumerable<FileAttachment> attachments, ThreadArchiveDuration archiveDuration, int? slowmode, string text, Embed embed, RequestOptions options, AllowedMentions allowedMentions, MessageComponent components, ISticker[] stickers, Embed[] embeds, MessageFlags flags)
	{
		return await CreatePostWithFilesAsync(title, attachments, archiveDuration, slowmode, text, embed, options, allowedMentions, components, stickers, embeds, flags);
	}
}
