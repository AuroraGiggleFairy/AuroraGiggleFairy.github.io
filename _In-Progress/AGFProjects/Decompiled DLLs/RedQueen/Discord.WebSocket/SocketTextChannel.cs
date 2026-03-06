using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord.API;
using Discord.Rest;

namespace Discord.WebSocket;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class SocketTextChannel : SocketGuildChannel, ITextChannel, IMessageChannel, IChannel, ISnowflakeEntity, IEntity<ulong>, IMentionable, INestedChannel, IGuildChannel, IDeletable, ISocketMessageChannel
{
	private readonly MessageCache _messages;

	private bool _nsfw;

	public string Topic { get; private set; }

	public virtual int SlowModeInterval { get; private set; }

	public ulong? CategoryId { get; private set; }

	public ICategoryChannel Category
	{
		get
		{
			if (!CategoryId.HasValue)
			{
				return null;
			}
			return base.Guild.GetChannel(CategoryId.Value) as ICategoryChannel;
		}
	}

	public bool IsNsfw => _nsfw;

	public ThreadArchiveDuration DefaultArchiveDuration { get; private set; }

	public string Mention => MentionUtils.MentionChannel(base.Id);

	public IReadOnlyCollection<SocketMessage> CachedMessages => (IReadOnlyCollection<SocketMessage>)(_messages?.Messages ?? ((object)System.Collections.Immutable.ImmutableArray.Create<SocketMessage>()));

	public override IReadOnlyCollection<SocketGuildUser> Users => base.Guild.Users.Where((SocketGuildUser x) => Permissions.GetValue(Permissions.ResolveChannel(base.Guild, x, this, Permissions.ResolveGuild(base.Guild, x)), ChannelPermission.ViewChannel)).ToImmutableArray();

	public IReadOnlyCollection<SocketThreadChannel> Threads => base.Guild.ThreadChannels.Where((SocketThreadChannel x) => x.ParentChannel.Id == base.Id).ToImmutableArray();

	private string DebuggerDisplay => $"{base.Name} ({base.Id}, Text)";

	public virtual Task SyncPermissionsAsync(RequestOptions options = null)
	{
		return ChannelHelper.SyncPermissionsAsync(this, base.Discord, options);
	}

	internal SocketTextChannel(DiscordSocketClient discord, ulong id, SocketGuild guild)
		: base(discord, id, guild)
	{
		DiscordSocketClient discord2 = base.Discord;
		if (discord2 != null && discord2.MessageCacheSize > 0)
		{
			_messages = new MessageCache(base.Discord);
		}
	}

	internal new static SocketTextChannel Create(SocketGuild guild, ClientState state, Channel model)
	{
		SocketTextChannel socketTextChannel = new SocketTextChannel(guild?.Discord, model.Id, guild);
		socketTextChannel.Update(state, model);
		return socketTextChannel;
	}

	internal override void Update(ClientState state, Channel model)
	{
		base.Update(state, model);
		CategoryId = model.CategoryId;
		Topic = model.Topic.GetValueOrDefault();
		SlowModeInterval = model.SlowMode.GetValueOrDefault();
		_nsfw = model.Nsfw.GetValueOrDefault();
		if (model.AutoArchiveDuration.IsSpecified)
		{
			DefaultArchiveDuration = model.AutoArchiveDuration.Value;
		}
		else
		{
			DefaultArchiveDuration = ThreadArchiveDuration.OneDay;
		}
	}

	public virtual Task ModifyAsync(Action<TextChannelProperties> func, RequestOptions options = null)
	{
		return ChannelHelper.ModifyAsync(this, base.Discord, func, options);
	}

	public virtual async Task<SocketThreadChannel> CreateThreadAsync(string name, ThreadType type = ThreadType.PublicThread, ThreadArchiveDuration autoArchiveDuration = ThreadArchiveDuration.OneDay, IMessage message = null, bool? invitable = null, int? slowmode = null, RequestOptions options = null)
	{
		Channel model = await ThreadHelper.CreateThreadAsync(base.Discord, this, name, type, autoArchiveDuration, message, invitable, slowmode, options);
		SocketThreadChannel thread = (SocketThreadChannel)base.Guild.AddOrUpdateChannel(base.Discord.State, model);
		if (base.Discord.AlwaysDownloadUsers && base.Discord.HasGatewayIntent(GatewayIntents.GuildMembers))
		{
			await thread.DownloadUsersAsync();
		}
		return thread;
	}

	public virtual SocketMessage GetCachedMessage(ulong id)
	{
		return _messages?.Get(id);
	}

	public virtual async Task<IMessage> GetMessageAsync(ulong id, RequestOptions options = null)
	{
		IMessage message = _messages?.Get(id);
		if (message == null)
		{
			message = await ChannelHelper.GetMessageAsync(this, base.Discord, id, options).ConfigureAwait(continueOnCapturedContext: false);
		}
		return message;
	}

	public virtual IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(int limit = 100, RequestOptions options = null)
	{
		return SocketChannelHelper.GetMessagesAsync(this, base.Discord, _messages, null, Direction.Before, limit, CacheMode.AllowDownload, options);
	}

	public virtual IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(ulong fromMessageId, Direction dir, int limit = 100, RequestOptions options = null)
	{
		return SocketChannelHelper.GetMessagesAsync(this, base.Discord, _messages, fromMessageId, dir, limit, CacheMode.AllowDownload, options);
	}

	public virtual IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(IMessage fromMessage, Direction dir, int limit = 100, RequestOptions options = null)
	{
		return SocketChannelHelper.GetMessagesAsync(this, base.Discord, _messages, fromMessage.Id, dir, limit, CacheMode.AllowDownload, options);
	}

	public virtual IReadOnlyCollection<SocketMessage> GetCachedMessages(int limit = 100)
	{
		return SocketChannelHelper.GetCachedMessages(this, base.Discord, _messages, null, Direction.Before, limit);
	}

	public virtual IReadOnlyCollection<SocketMessage> GetCachedMessages(ulong fromMessageId, Direction dir, int limit = 100)
	{
		return SocketChannelHelper.GetCachedMessages(this, base.Discord, _messages, fromMessageId, dir, limit);
	}

	public virtual IReadOnlyCollection<SocketMessage> GetCachedMessages(IMessage fromMessage, Direction dir, int limit = 100)
	{
		return SocketChannelHelper.GetCachedMessages(this, base.Discord, _messages, fromMessage.Id, dir, limit);
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

	public virtual Task DeleteMessageAsync(ulong messageId, RequestOptions options = null)
	{
		return ChannelHelper.DeleteMessageAsync(this, messageId, base.Discord, options);
	}

	public virtual Task DeleteMessageAsync(IMessage message, RequestOptions options = null)
	{
		return ChannelHelper.DeleteMessageAsync(this, message.Id, base.Discord, options);
	}

	public virtual Task TriggerTypingAsync(RequestOptions options = null)
	{
		return ChannelHelper.TriggerTypingAsync(this, base.Discord, options);
	}

	public virtual IDisposable EnterTypingState(RequestOptions options = null)
	{
		return ChannelHelper.EnterTypingState(this, base.Discord, options);
	}

	internal void AddMessage(SocketMessage msg)
	{
		_messages?.Add(msg);
	}

	internal SocketMessage RemoveMessage(ulong id)
	{
		return _messages?.Remove(id);
	}

	public override SocketGuildUser GetUser(ulong id)
	{
		SocketGuildUser user = base.Guild.GetUser(id);
		if (user != null)
		{
			ulong guildPermissions = Permissions.ResolveGuild(base.Guild, user);
			if (Permissions.GetValue(Permissions.ResolveChannel(base.Guild, user, this, guildPermissions), ChannelPermission.ViewChannel))
			{
				return user;
			}
		}
		return null;
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

	public virtual async Task<IInviteMetadata> CreateInviteAsync(int? maxAge = 86400, int? maxUses = null, bool isTemporary = false, bool isUnique = false, RequestOptions options = null)
	{
		return await ChannelHelper.CreateInviteAsync(this, base.Discord, maxAge, maxUses, isTemporary, isUnique, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public virtual async Task<IInviteMetadata> CreateInviteToApplicationAsync(ulong applicationId, int? maxAge, int? maxUses = null, bool isTemporary = false, bool isUnique = false, RequestOptions options = null)
	{
		return await ChannelHelper.CreateInviteToApplicationAsync(this, base.Discord, maxAge, maxUses, isTemporary, isUnique, applicationId, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public virtual async Task<IInviteMetadata> CreateInviteToApplicationAsync(DefaultApplications application, int? maxAge = 86400, int? maxUses = null, bool isTemporary = false, bool isUnique = false, RequestOptions options = null)
	{
		return await ChannelHelper.CreateInviteToApplicationAsync(this, base.Discord, maxAge, maxUses, isTemporary, isUnique, (ulong)application, options);
	}

	public virtual async Task<IInviteMetadata> CreateInviteToStreamAsync(IUser user, int? maxAge, int? maxUses = null, bool isTemporary = false, bool isUnique = false, RequestOptions options = null)
	{
		return await ChannelHelper.CreateInviteToStreamAsync(this, base.Discord, maxAge, maxUses, isTemporary, isUnique, user, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public virtual async Task<IReadOnlyCollection<IInviteMetadata>> GetInvitesAsync(RequestOptions options = null)
	{
		return await ChannelHelper.GetInvitesAsync(this, base.Discord, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	internal new SocketTextChannel Clone()
	{
		return MemberwiseClone() as SocketTextChannel;
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

	async Task<IGuildUser> IGuildChannel.GetUserAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		SocketGuildUser user = GetUser(id);
		if (user != null || mode == CacheMode.CacheOnly)
		{
			return user;
		}
		return await ChannelHelper.GetUserAsync(this, base.Guild, base.Discord, id, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	IAsyncEnumerable<IReadOnlyCollection<IGuildUser>> IGuildChannel.GetUsersAsync(CacheMode mode, RequestOptions options)
	{
		if (mode != CacheMode.AllowDownload)
		{
			return System.Collections.Immutable.ImmutableArray.Create((IReadOnlyCollection<IGuildUser>)Users).ToAsyncEnumerable();
		}
		return ChannelHelper.GetUsersAsync(this, base.Guild, base.Discord, null, null, options);
	}

	async Task<IMessage> IMessageChannel.GetMessageAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		if (mode == CacheMode.AllowDownload)
		{
			return await GetMessageAsync(id, options).ConfigureAwait(continueOnCapturedContext: false);
		}
		return GetCachedMessage(id);
	}

	IAsyncEnumerable<IReadOnlyCollection<IMessage>> IMessageChannel.GetMessagesAsync(int limit, CacheMode mode, RequestOptions options)
	{
		return SocketChannelHelper.GetMessagesAsync(this, base.Discord, _messages, null, Direction.Before, limit, mode, options);
	}

	IAsyncEnumerable<IReadOnlyCollection<IMessage>> IMessageChannel.GetMessagesAsync(ulong fromMessageId, Direction dir, int limit, CacheMode mode, RequestOptions options)
	{
		return SocketChannelHelper.GetMessagesAsync(this, base.Discord, _messages, fromMessageId, dir, limit, mode, options);
	}

	IAsyncEnumerable<IReadOnlyCollection<IMessage>> IMessageChannel.GetMessagesAsync(IMessage fromMessage, Direction dir, int limit, CacheMode mode, RequestOptions options)
	{
		return SocketChannelHelper.GetMessagesAsync(this, base.Discord, _messages, fromMessage.Id, dir, limit, mode, options);
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

	Task<ICategoryChannel> INestedChannel.GetCategoryAsync(CacheMode mode, RequestOptions options)
	{
		return Task.FromResult(Category);
	}
}
