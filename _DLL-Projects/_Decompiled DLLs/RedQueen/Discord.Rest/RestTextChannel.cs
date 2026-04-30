using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord.API;

namespace Discord.Rest;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class RestTextChannel : RestGuildChannel, IRestMessageChannel, IMessageChannel, IChannel, ISnowflakeEntity, IEntity<ulong>, ITextChannel, IMentionable, INestedChannel, IGuildChannel, IDeletable
{
	public string Topic { get; private set; }

	public virtual int SlowModeInterval { get; private set; }

	public ulong? CategoryId { get; private set; }

	public string Mention => MentionUtils.MentionChannel(base.Id);

	public bool IsNsfw { get; private set; }

	public ThreadArchiveDuration DefaultArchiveDuration { get; private set; }

	private string DebuggerDisplay => $"{base.Name} ({base.Id}, Text)";

	internal RestTextChannel(BaseDiscordClient discord, IGuild guild, ulong id)
		: base(discord, guild, id)
	{
	}

	internal new static RestTextChannel Create(BaseDiscordClient discord, IGuild guild, Channel model)
	{
		RestTextChannel restTextChannel = new RestTextChannel(discord, guild, model.Id);
		restTextChannel.Update(model);
		return restTextChannel;
	}

	internal override void Update(Channel model)
	{
		base.Update(model);
		CategoryId = model.CategoryId;
		Topic = model.Topic.GetValueOrDefault();
		if (model.SlowMode.IsSpecified)
		{
			SlowModeInterval = model.SlowMode.Value;
		}
		IsNsfw = model.Nsfw.GetValueOrDefault();
		if (model.AutoArchiveDuration.IsSpecified)
		{
			DefaultArchiveDuration = model.AutoArchiveDuration.Value;
		}
		else
		{
			DefaultArchiveDuration = ThreadArchiveDuration.OneDay;
		}
	}

	public virtual async Task ModifyAsync(Action<TextChannelProperties> func, RequestOptions options = null)
	{
		Update(await ChannelHelper.ModifyAsync(this, base.Discord, func, options).ConfigureAwait(continueOnCapturedContext: false));
	}

	public Task<RestGuildUser> GetUserAsync(ulong id, RequestOptions options = null)
	{
		return ChannelHelper.GetUserAsync(this, base.Guild, base.Discord, id, options);
	}

	public IAsyncEnumerable<IReadOnlyCollection<RestGuildUser>> GetUsersAsync(RequestOptions options = null)
	{
		return ChannelHelper.GetUsersAsync(this, base.Guild, base.Discord, null, null, options);
	}

	public virtual Task<RestMessage> GetMessageAsync(ulong id, RequestOptions options = null)
	{
		return ChannelHelper.GetMessageAsync(this, base.Discord, id, options);
	}

	public virtual IAsyncEnumerable<IReadOnlyCollection<RestMessage>> GetMessagesAsync(int limit = 100, RequestOptions options = null)
	{
		return ChannelHelper.GetMessagesAsync(this, base.Discord, null, Direction.Before, limit, options);
	}

	public virtual IAsyncEnumerable<IReadOnlyCollection<RestMessage>> GetMessagesAsync(ulong fromMessageId, Direction dir, int limit = 100, RequestOptions options = null)
	{
		return ChannelHelper.GetMessagesAsync(this, base.Discord, fromMessageId, dir, limit, options);
	}

	public virtual IAsyncEnumerable<IReadOnlyCollection<RestMessage>> GetMessagesAsync(IMessage fromMessage, Direction dir, int limit = 100, RequestOptions options = null)
	{
		return ChannelHelper.GetMessagesAsync(this, base.Discord, fromMessage.Id, dir, limit, options);
	}

	public virtual Task<IReadOnlyCollection<RestMessage>> GetPinnedMessagesAsync(RequestOptions options = null)
	{
		return ChannelHelper.GetPinnedMessagesAsync(this, base.Discord, options);
	}

	public virtual Task<RestUserMessage> SendMessageAsync(string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageReference messageReference = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None)
	{
		return ChannelHelper.SendMessageAsync(this, base.Discord, text, isTTS, embed, allowedMentions, messageReference, components, stickers, options, embeds, flags);
	}

	public virtual Task<RestUserMessage> SendFileAsync(string filePath, string text, bool isTTS = false, Embed embed = null, RequestOptions options = null, bool isSpoiler = false, AllowedMentions allowedMentions = null, MessageReference messageReference = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None)
	{
		return ChannelHelper.SendFileAsync(this, base.Discord, filePath, text, isTTS, embed, allowedMentions, messageReference, components, stickers, options, isSpoiler, embeds, flags);
	}

	public virtual Task<RestUserMessage> SendFileAsync(Stream stream, string filename, string text, bool isTTS = false, Embed embed = null, RequestOptions options = null, bool isSpoiler = false, AllowedMentions allowedMentions = null, MessageReference messageReference = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None)
	{
		return ChannelHelper.SendFileAsync(this, base.Discord, stream, filename, text, isTTS, embed, allowedMentions, messageReference, components, stickers, options, isSpoiler, embeds, flags);
	}

	public virtual Task<RestUserMessage> SendFileAsync(FileAttachment attachment, string text, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageReference messageReference = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None)
	{
		return ChannelHelper.SendFileAsync(this, base.Discord, attachment, text, isTTS, embed, allowedMentions, messageReference, components, stickers, options, embeds, flags);
	}

	public virtual Task<RestUserMessage> SendFilesAsync(IEnumerable<FileAttachment> attachments, string text, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageReference messageReference = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None)
	{
		return ChannelHelper.SendFilesAsync(this, base.Discord, attachments, text, isTTS, embed, allowedMentions, messageReference, components, stickers, options, embeds, flags);
	}

	public virtual Task DeleteMessageAsync(ulong messageId, RequestOptions options = null)
	{
		return ChannelHelper.DeleteMessageAsync(this, messageId, base.Discord, options);
	}

	public virtual Task DeleteMessageAsync(IMessage message, RequestOptions options = null)
	{
		return ChannelHelper.DeleteMessageAsync(this, message.Id, base.Discord, options);
	}

	public virtual Task DeleteMessagesAsync(IEnumerable<IMessage> messages, RequestOptions options = null)
	{
		return ChannelHelper.DeleteMessagesAsync(this, base.Discord, messages.Select((IMessage x) => x.Id), options);
	}

	public virtual Task DeleteMessagesAsync(IEnumerable<ulong> messageIds, RequestOptions options = null)
	{
		return ChannelHelper.DeleteMessagesAsync(this, base.Discord, messageIds, options);
	}

	public virtual async Task<IUserMessage> ModifyMessageAsync(ulong messageId, Action<MessageProperties> func, RequestOptions options = null)
	{
		return await ChannelHelper.ModifyMessageAsync(this, messageId, func, base.Discord, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public virtual Task TriggerTypingAsync(RequestOptions options = null)
	{
		return ChannelHelper.TriggerTypingAsync(this, base.Discord, options);
	}

	public virtual IDisposable EnterTypingState(RequestOptions options = null)
	{
		return ChannelHelper.EnterTypingState(this, base.Discord, options);
	}

	public virtual Task<RestWebhook> CreateWebhookAsync(string name, Stream avatar = null, RequestOptions options = null)
	{
		return ChannelHelper.CreateWebhookAsync(this, base.Discord, name, avatar, options);
	}

	public virtual Task<RestWebhook> GetWebhookAsync(ulong id, RequestOptions options = null)
	{
		return ChannelHelper.GetWebhookAsync(this, base.Discord, id, options);
	}

	public virtual Task<IReadOnlyCollection<RestWebhook>> GetWebhooksAsync(RequestOptions options = null)
	{
		return ChannelHelper.GetWebhooksAsync(this, base.Discord, options);
	}

	public virtual async Task<RestThreadChannel> CreateThreadAsync(string name, ThreadType type = ThreadType.PublicThread, ThreadArchiveDuration autoArchiveDuration = ThreadArchiveDuration.OneDay, IMessage message = null, bool? invitable = null, int? slowmode = null, RequestOptions options = null)
	{
		Channel model = await ThreadHelper.CreateThreadAsync(base.Discord, this, name, type, autoArchiveDuration, message, invitable, slowmode, options);
		return RestThreadChannel.Create(base.Discord, base.Guild, model);
	}

	public virtual Task<ICategoryChannel> GetCategoryAsync(RequestOptions options = null)
	{
		return ChannelHelper.GetCategoryAsync(this, base.Discord, options);
	}

	public Task SyncPermissionsAsync(RequestOptions options = null)
	{
		return ChannelHelper.SyncPermissionsAsync(this, base.Discord, options);
	}

	public virtual async Task<IInviteMetadata> CreateInviteAsync(int? maxAge = 86400, int? maxUses = null, bool isTemporary = false, bool isUnique = false, RequestOptions options = null)
	{
		return await ChannelHelper.CreateInviteAsync(this, base.Discord, maxAge, maxUses, isTemporary, isUnique, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public virtual async Task<IInviteMetadata> CreateInviteToApplicationAsync(ulong applicationId, int? maxAge = 86400, int? maxUses = null, bool isTemporary = false, bool isUnique = false, RequestOptions options = null)
	{
		return await ChannelHelper.CreateInviteToApplicationAsync(this, base.Discord, maxAge, maxUses, isTemporary, isUnique, applicationId, options);
	}

	public virtual async Task<IInviteMetadata> CreateInviteToApplicationAsync(DefaultApplications application, int? maxAge = 86400, int? maxUses = null, bool isTemporary = false, bool isUnique = false, RequestOptions options = null)
	{
		return await ChannelHelper.CreateInviteToApplicationAsync(this, base.Discord, maxAge, maxUses, isTemporary, isUnique, (ulong)application, options);
	}

	public virtual Task<IInviteMetadata> CreateInviteToStreamAsync(IUser user, int? maxAge, int? maxUses = null, bool isTemporary = false, bool isUnique = false, RequestOptions options = null)
	{
		throw new NotImplementedException();
	}

	public virtual async Task<IReadOnlyCollection<IInviteMetadata>> GetInvitesAsync(RequestOptions options = null)
	{
		return await ChannelHelper.GetInvitesAsync(this, base.Discord, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IWebhook> ITextChannel.CreateWebhookAsync(string name, Stream avatar, RequestOptions options)
	{
		return await CreateWebhookAsync(name, avatar, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IWebhook> ITextChannel.GetWebhookAsync(ulong id, RequestOptions options)
	{
		return await GetWebhookAsync(id, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IReadOnlyCollection<IWebhook>> ITextChannel.GetWebhooksAsync(RequestOptions options)
	{
		return await GetWebhooksAsync(options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IThreadChannel> ITextChannel.CreateThreadAsync(string name, ThreadType type, ThreadArchiveDuration autoArchiveDuration, IMessage message, bool? invitable, int? slowmode, RequestOptions options)
	{
		return await CreateThreadAsync(name, type, autoArchiveDuration, message, invitable, slowmode, options);
	}

	async Task<IMessage> IMessageChannel.GetMessageAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		if (mode == CacheMode.AllowDownload)
		{
			return await GetMessageAsync(id, options).ConfigureAwait(continueOnCapturedContext: false);
		}
		return null;
	}

	IAsyncEnumerable<IReadOnlyCollection<IMessage>> IMessageChannel.GetMessagesAsync(int limit, CacheMode mode, RequestOptions options)
	{
		if (mode == CacheMode.AllowDownload)
		{
			return GetMessagesAsync(limit, options);
		}
		return AsyncEnumerable.Empty<IReadOnlyCollection<IMessage>>();
	}

	IAsyncEnumerable<IReadOnlyCollection<IMessage>> IMessageChannel.GetMessagesAsync(ulong fromMessageId, Direction dir, int limit, CacheMode mode, RequestOptions options)
	{
		if (mode == CacheMode.AllowDownload)
		{
			return GetMessagesAsync(fromMessageId, dir, limit, options);
		}
		return AsyncEnumerable.Empty<IReadOnlyCollection<IMessage>>();
	}

	IAsyncEnumerable<IReadOnlyCollection<IMessage>> IMessageChannel.GetMessagesAsync(IMessage fromMessage, Direction dir, int limit, CacheMode mode, RequestOptions options)
	{
		if (mode == CacheMode.AllowDownload)
		{
			return GetMessagesAsync(fromMessage, dir, limit, options);
		}
		return AsyncEnumerable.Empty<IReadOnlyCollection<IMessage>>();
	}

	async Task<IReadOnlyCollection<IMessage>> IMessageChannel.GetPinnedMessagesAsync(RequestOptions options)
	{
		return await GetPinnedMessagesAsync(options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IUserMessage> IMessageChannel.SendFileAsync(string filePath, string text, bool isTTS, Embed embed, RequestOptions options, bool isSpoiler, AllowedMentions allowedMentions, MessageReference messageReference, MessageComponent components, ISticker[] stickers, Embed[] embeds, MessageFlags flags)
	{
		return await SendFileAsync(filePath, text, isTTS, embed, options, isSpoiler, allowedMentions, messageReference, components, stickers, embeds, flags).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IUserMessage> IMessageChannel.SendFileAsync(Stream stream, string filename, string text, bool isTTS, Embed embed, RequestOptions options, bool isSpoiler, AllowedMentions allowedMentions, MessageReference messageReference, MessageComponent components, ISticker[] stickers, Embed[] embeds, MessageFlags flags)
	{
		return await SendFileAsync(stream, filename, text, isTTS, embed, options, isSpoiler, allowedMentions, messageReference, components, stickers, embeds, flags).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IUserMessage> IMessageChannel.SendFileAsync(FileAttachment attachment, string text, bool isTTS, Embed embed, RequestOptions options, AllowedMentions allowedMentions, MessageReference messageReference, MessageComponent components, ISticker[] stickers, Embed[] embeds, MessageFlags flags)
	{
		return await SendFileAsync(attachment, text, isTTS, embed, options, allowedMentions, messageReference, components, stickers, embeds, flags).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IUserMessage> IMessageChannel.SendFilesAsync(IEnumerable<FileAttachment> attachments, string text, bool isTTS, Embed embed, RequestOptions options, AllowedMentions allowedMentions, MessageReference messageReference, MessageComponent components, ISticker[] stickers, Embed[] embeds, MessageFlags flags)
	{
		return await SendFilesAsync(attachments, text, isTTS, embed, options, allowedMentions, messageReference, components, stickers, embeds, flags).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IUserMessage> IMessageChannel.SendMessageAsync(string text, bool isTTS, Embed embed, RequestOptions options, AllowedMentions allowedMentions, MessageReference messageReference, MessageComponent components, ISticker[] stickers, Embed[] embeds, MessageFlags flags)
	{
		return await SendMessageAsync(text, isTTS, embed, options, allowedMentions, messageReference, components, stickers, embeds, flags).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IGuildUser> IGuildChannel.GetUserAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		if (mode == CacheMode.AllowDownload)
		{
			return await GetUserAsync(id, options).ConfigureAwait(continueOnCapturedContext: false);
		}
		return null;
	}

	IAsyncEnumerable<IReadOnlyCollection<IGuildUser>> IGuildChannel.GetUsersAsync(CacheMode mode, RequestOptions options)
	{
		if (mode != CacheMode.AllowDownload)
		{
			return AsyncEnumerable.Empty<IReadOnlyCollection<IGuildUser>>();
		}
		return GetUsersAsync(options);
	}

	async Task<IUser> IChannel.GetUserAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		if (mode == CacheMode.AllowDownload)
		{
			return await GetUserAsync(id, options).ConfigureAwait(continueOnCapturedContext: false);
		}
		return null;
	}

	IAsyncEnumerable<IReadOnlyCollection<IUser>> IChannel.GetUsersAsync(CacheMode mode, RequestOptions options)
	{
		if (mode == CacheMode.AllowDownload)
		{
			return GetUsersAsync(options);
		}
		return AsyncEnumerable.Empty<IReadOnlyCollection<IGuildUser>>();
	}

	async Task<ICategoryChannel> INestedChannel.GetCategoryAsync(CacheMode mode, RequestOptions options)
	{
		if (CategoryId.HasValue && mode == CacheMode.AllowDownload)
		{
			return (await base.Guild.GetChannelAsync(CategoryId.Value, mode, options).ConfigureAwait(continueOnCapturedContext: false)) as ICategoryChannel;
		}
		return null;
	}
}
