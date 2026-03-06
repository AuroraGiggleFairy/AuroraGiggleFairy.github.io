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
internal class SocketDMChannel : SocketChannel, IDMChannel, IMessageChannel, IChannel, ISnowflakeEntity, IEntity<ulong>, IPrivateChannel, ISocketPrivateChannel, ISocketMessageChannel
{
	public SocketUser Recipient { get; }

	public IReadOnlyCollection<SocketMessage> CachedMessages => System.Collections.Immutable.ImmutableArray.Create<SocketMessage>();

	public new IReadOnlyCollection<SocketUser> Users => System.Collections.Immutable.ImmutableArray.Create(base.Discord.CurrentUser, Recipient);

	private string DebuggerDisplay => $"@{Recipient} ({base.Id}, DM)";

	IUser IDMChannel.Recipient => Recipient;

	IReadOnlyCollection<SocketUser> ISocketPrivateChannel.Recipients => System.Collections.Immutable.ImmutableArray.Create(Recipient);

	IReadOnlyCollection<IUser> IPrivateChannel.Recipients => System.Collections.Immutable.ImmutableArray.Create((IUser)Recipient);

	string IChannel.Name => $"@{Recipient}";

	internal SocketDMChannel(DiscordSocketClient discord, ulong id, SocketUser recipient)
		: base(discord, id)
	{
		Recipient = recipient;
	}

	internal static SocketDMChannel Create(DiscordSocketClient discord, ClientState state, Channel model)
	{
		SocketDMChannel socketDMChannel = new SocketDMChannel(discord, model.Id, discord.GetOrCreateTemporaryUser(state, model.Recipients.Value[0]));
		socketDMChannel.Update(state, model);
		return socketDMChannel;
	}

	internal override void Update(ClientState state, Channel model)
	{
		Recipient.Update(state, model.Recipients.Value[0]);
	}

	internal static SocketDMChannel Create(DiscordSocketClient discord, ClientState state, ulong channelId, User recipient)
	{
		SocketDMChannel socketDMChannel = new SocketDMChannel(discord, channelId, discord.GetOrCreateTemporaryUser(state, recipient));
		socketDMChannel.Update(state, recipient);
		return socketDMChannel;
	}

	internal void Update(ClientState state, User recipient)
	{
		Recipient.Update(state, recipient);
	}

	public Task CloseAsync(RequestOptions options = null)
	{
		return ChannelHelper.DeleteAsync(this, base.Discord, options);
	}

	public SocketMessage GetCachedMessage(ulong id)
	{
		return null;
	}

	public async Task<IMessage> GetMessageAsync(ulong id, RequestOptions options = null)
	{
		return await ChannelHelper.GetMessageAsync(this, base.Discord, id, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(int limit = 100, RequestOptions options = null)
	{
		return ChannelHelper.GetMessagesAsync(this, base.Discord, null, Direction.Before, limit, options);
	}

	public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(ulong fromMessageId, Direction dir, int limit = 100, RequestOptions options = null)
	{
		return ChannelHelper.GetMessagesAsync(this, base.Discord, fromMessageId, dir, limit, options);
	}

	public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(IMessage fromMessage, Direction dir, int limit = 100, RequestOptions options = null)
	{
		return ChannelHelper.GetMessagesAsync(this, base.Discord, fromMessage.Id, dir, limit, options);
	}

	public IReadOnlyCollection<SocketMessage> GetCachedMessages(int limit = 100)
	{
		return System.Collections.Immutable.ImmutableArray.Create<SocketMessage>();
	}

	public IReadOnlyCollection<SocketMessage> GetCachedMessages(ulong fromMessageId, Direction dir, int limit = 100)
	{
		return System.Collections.Immutable.ImmutableArray.Create<SocketMessage>();
	}

	public IReadOnlyCollection<SocketMessage> GetCachedMessages(IMessage fromMessage, Direction dir, int limit = 100)
	{
		return System.Collections.Immutable.ImmutableArray.Create<SocketMessage>();
	}

	public Task<IReadOnlyCollection<RestMessage>> GetPinnedMessagesAsync(RequestOptions options = null)
	{
		return ChannelHelper.GetPinnedMessagesAsync(this, base.Discord, options);
	}

	public Task<RestUserMessage> SendMessageAsync(string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageReference messageReference = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None)
	{
		return ChannelHelper.SendMessageAsync(this, base.Discord, text, isTTS, embed, allowedMentions, messageReference, components, stickers, options, embeds, flags);
	}

	public Task<RestUserMessage> SendFileAsync(string filePath, string text, bool isTTS = false, Embed embed = null, RequestOptions options = null, bool isSpoiler = false, AllowedMentions allowedMentions = null, MessageReference messageReference = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None)
	{
		return ChannelHelper.SendFileAsync(this, base.Discord, filePath, text, isTTS, embed, allowedMentions, messageReference, components, stickers, options, isSpoiler, embeds, flags);
	}

	public Task<RestUserMessage> SendFileAsync(Stream stream, string filename, string text, bool isTTS = false, Embed embed = null, RequestOptions options = null, bool isSpoiler = false, AllowedMentions allowedMentions = null, MessageReference messageReference = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None)
	{
		return ChannelHelper.SendFileAsync(this, base.Discord, stream, filename, text, isTTS, embed, allowedMentions, messageReference, components, stickers, options, isSpoiler, embeds, flags);
	}

	public Task<RestUserMessage> SendFileAsync(FileAttachment attachment, string text, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageReference messageReference = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None)
	{
		return ChannelHelper.SendFileAsync(this, base.Discord, attachment, text, isTTS, embed, allowedMentions, messageReference, components, stickers, options, embeds, flags);
	}

	public Task<RestUserMessage> SendFilesAsync(IEnumerable<FileAttachment> attachments, string text, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageReference messageReference = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None)
	{
		return ChannelHelper.SendFilesAsync(this, base.Discord, attachments, text, isTTS, embed, allowedMentions, messageReference, components, stickers, options, embeds, flags);
	}

	public Task DeleteMessageAsync(ulong messageId, RequestOptions options = null)
	{
		return ChannelHelper.DeleteMessageAsync(this, messageId, base.Discord, options);
	}

	public Task DeleteMessageAsync(IMessage message, RequestOptions options = null)
	{
		return ChannelHelper.DeleteMessageAsync(this, message.Id, base.Discord, options);
	}

	public async Task<IUserMessage> ModifyMessageAsync(ulong messageId, Action<MessageProperties> func, RequestOptions options = null)
	{
		return await ChannelHelper.ModifyMessageAsync(this, messageId, func, base.Discord, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public Task TriggerTypingAsync(RequestOptions options = null)
	{
		return ChannelHelper.TriggerTypingAsync(this, base.Discord, options);
	}

	public IDisposable EnterTypingState(RequestOptions options = null)
	{
		return ChannelHelper.EnterTypingState(this, base.Discord, options);
	}

	internal void AddMessage(SocketMessage msg)
	{
	}

	internal SocketMessage RemoveMessage(ulong id)
	{
		return null;
	}

	public new SocketUser GetUser(ulong id)
	{
		if (id == Recipient.Id)
		{
			return Recipient;
		}
		if (id == base.Discord.CurrentUser.Id)
		{
			return base.Discord.CurrentUser;
		}
		return null;
	}

	public override string ToString()
	{
		return $"@{Recipient}";
	}

	internal new SocketDMChannel Clone()
	{
		return MemberwiseClone() as SocketDMChannel;
	}

	internal override IReadOnlyCollection<SocketUser> GetUsersInternal()
	{
		return Users;
	}

	internal override SocketUser GetUserInternal(ulong id)
	{
		return GetUser(id);
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
		if (mode != CacheMode.CacheOnly)
		{
			return GetMessagesAsync(limit, options);
		}
		return null;
	}

	IAsyncEnumerable<IReadOnlyCollection<IMessage>> IMessageChannel.GetMessagesAsync(ulong fromMessageId, Direction dir, int limit, CacheMode mode, RequestOptions options)
	{
		if (mode != CacheMode.CacheOnly)
		{
			return GetMessagesAsync(fromMessageId, dir, limit, options);
		}
		return null;
	}

	IAsyncEnumerable<IReadOnlyCollection<IMessage>> IMessageChannel.GetMessagesAsync(IMessage fromMessage, Direction dir, int limit, CacheMode mode, RequestOptions options)
	{
		if (mode != CacheMode.CacheOnly)
		{
			return GetMessagesAsync(fromMessage.Id, dir, limit, options);
		}
		return null;
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

	Task<IUser> IChannel.GetUserAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		return Task.FromResult((IUser)GetUser(id));
	}

	IAsyncEnumerable<IReadOnlyCollection<IUser>> IChannel.GetUsersAsync(CacheMode mode, RequestOptions options)
	{
		return System.Collections.Immutable.ImmutableArray.Create((IReadOnlyCollection<IUser>)Users).ToAsyncEnumerable();
	}
}
