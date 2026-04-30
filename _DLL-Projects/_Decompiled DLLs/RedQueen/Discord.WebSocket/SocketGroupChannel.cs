using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord.API;
using Discord.Audio;
using Discord.Rest;

namespace Discord.WebSocket;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class SocketGroupChannel : SocketChannel, IGroupChannel, IMessageChannel, IChannel, ISnowflakeEntity, IEntity<ulong>, IPrivateChannel, IAudioChannel, ISocketPrivateChannel, ISocketMessageChannel, ISocketAudioChannel
{
	private readonly MessageCache _messages;

	private readonly ConcurrentDictionary<ulong, SocketVoiceState> _voiceStates;

	private string _iconId;

	private ConcurrentDictionary<ulong, SocketGroupUser> _users;

	public string Name { get; private set; }

	public string RTCRegion { get; private set; }

	public IReadOnlyCollection<SocketMessage> CachedMessages => (IReadOnlyCollection<SocketMessage>)(_messages?.Messages ?? ((object)System.Collections.Immutable.ImmutableArray.Create<SocketMessage>()));

	public new IReadOnlyCollection<SocketGroupUser> Users => _users.ToReadOnlyCollection();

	public IReadOnlyCollection<SocketGroupUser> Recipients => (from x in _users
		select x.Value into x
		where x.Id != base.Discord.CurrentUser.Id
		select x).ToReadOnlyCollection(() => _users.Count - 1);

	private string DebuggerDisplay => $"{Name} ({base.Id}, Group)";

	IReadOnlyCollection<SocketUser> ISocketPrivateChannel.Recipients => Recipients;

	IReadOnlyCollection<IUser> IPrivateChannel.Recipients => Recipients;

	internal SocketGroupChannel(DiscordSocketClient discord, ulong id)
		: base(discord, id)
	{
		if (base.Discord.MessageCacheSize > 0)
		{
			_messages = new MessageCache(base.Discord);
		}
		_voiceStates = new ConcurrentDictionary<ulong, SocketVoiceState>(ConcurrentHashSet.DefaultConcurrencyLevel, 5);
		_users = new ConcurrentDictionary<ulong, SocketGroupUser>(ConcurrentHashSet.DefaultConcurrencyLevel, 5);
	}

	internal static SocketGroupChannel Create(DiscordSocketClient discord, ClientState state, Channel model)
	{
		SocketGroupChannel socketGroupChannel = new SocketGroupChannel(discord, model.Id);
		socketGroupChannel.Update(state, model);
		return socketGroupChannel;
	}

	internal override void Update(ClientState state, Channel model)
	{
		if (model.Name.IsSpecified)
		{
			Name = model.Name.Value;
		}
		if (model.Icon.IsSpecified)
		{
			_iconId = model.Icon.Value;
		}
		if (model.Recipients.IsSpecified)
		{
			UpdateUsers(state, model.Recipients.Value);
		}
		RTCRegion = model.RTCRegion.GetValueOrDefault(null);
	}

	private void UpdateUsers(ClientState state, User[] models)
	{
		ConcurrentDictionary<ulong, SocketGroupUser> concurrentDictionary = new ConcurrentDictionary<ulong, SocketGroupUser>(ConcurrentHashSet.DefaultConcurrencyLevel, (int)((double)models.Length * 1.05));
		for (int i = 0; i < models.Length; i++)
		{
			concurrentDictionary[models[i].Id] = SocketGroupUser.Create(this, state, models[i]);
		}
		_users = concurrentDictionary;
	}

	public Task LeaveAsync(RequestOptions options = null)
	{
		return ChannelHelper.DeleteAsync(this, base.Discord, options);
	}

	public Task<IAudioClient> ConnectAsync()
	{
		throw new NotSupportedException("Voice is not yet supported for group channels.");
	}

	public SocketMessage GetCachedMessage(ulong id)
	{
		return _messages?.Get(id);
	}

	public async Task<IMessage> GetMessageAsync(ulong id, RequestOptions options = null)
	{
		IMessage message = _messages?.Get(id);
		if (message == null)
		{
			message = await ChannelHelper.GetMessageAsync(this, base.Discord, id, options).ConfigureAwait(continueOnCapturedContext: false);
		}
		return message;
	}

	public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(int limit = 100, RequestOptions options = null)
	{
		return SocketChannelHelper.GetMessagesAsync(this, base.Discord, _messages, null, Direction.Before, limit, CacheMode.AllowDownload, options);
	}

	public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(ulong fromMessageId, Direction dir, int limit = 100, RequestOptions options = null)
	{
		return SocketChannelHelper.GetMessagesAsync(this, base.Discord, _messages, fromMessageId, dir, limit, CacheMode.AllowDownload, options);
	}

	public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(IMessage fromMessage, Direction dir, int limit = 100, RequestOptions options = null)
	{
		return SocketChannelHelper.GetMessagesAsync(this, base.Discord, _messages, fromMessage.Id, dir, limit, CacheMode.AllowDownload, options);
	}

	public IReadOnlyCollection<SocketMessage> GetCachedMessages(int limit = 100)
	{
		return SocketChannelHelper.GetCachedMessages(this, base.Discord, _messages, null, Direction.Before, limit);
	}

	public IReadOnlyCollection<SocketMessage> GetCachedMessages(ulong fromMessageId, Direction dir, int limit = 100)
	{
		return SocketChannelHelper.GetCachedMessages(this, base.Discord, _messages, fromMessageId, dir, limit);
	}

	public IReadOnlyCollection<SocketMessage> GetCachedMessages(IMessage fromMessage, Direction dir, int limit = 100)
	{
		return SocketChannelHelper.GetCachedMessages(this, base.Discord, _messages, fromMessage.Id, dir, limit);
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
		_messages?.Add(msg);
	}

	internal SocketMessage RemoveMessage(ulong id)
	{
		return _messages?.Remove(id);
	}

	public new SocketGroupUser GetUser(ulong id)
	{
		if (_users.TryGetValue(id, out var value))
		{
			return value;
		}
		return null;
	}

	internal SocketGroupUser GetOrAddUser(User model)
	{
		if (_users.TryGetValue(model.Id, out var value))
		{
			return value;
		}
		SocketGroupUser socketGroupUser = SocketGroupUser.Create(this, base.Discord.State, model);
		socketGroupUser.GlobalUser.AddRef();
		_users[socketGroupUser.Id] = socketGroupUser;
		return socketGroupUser;
	}

	internal SocketGroupUser RemoveUser(ulong id)
	{
		if (_users.TryRemove(id, out var value))
		{
			value.GlobalUser.RemoveRef(base.Discord);
			return value;
		}
		return null;
	}

	internal SocketVoiceState AddOrUpdateVoiceState(ClientState state, VoiceState model)
	{
		SocketVoiceState socketVoiceState = SocketVoiceState.Create(state.GetChannel(model.ChannelId.Value) as SocketVoiceChannel, model);
		_voiceStates[model.UserId] = socketVoiceState;
		return socketVoiceState;
	}

	internal SocketVoiceState? GetVoiceState(ulong id)
	{
		if (_voiceStates.TryGetValue(id, out var value))
		{
			return value;
		}
		return null;
	}

	internal SocketVoiceState? RemoveVoiceState(ulong id)
	{
		if (_voiceStates.TryRemove(id, out var value))
		{
			return value;
		}
		return null;
	}

	public override string ToString()
	{
		return Name;
	}

	internal new SocketGroupChannel Clone()
	{
		return MemberwiseClone() as SocketGroupChannel;
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

	Task<IAudioClient> IAudioChannel.ConnectAsync(bool selfDeaf, bool selfMute, bool external)
	{
		throw new NotSupportedException();
	}

	Task IAudioChannel.DisconnectAsync()
	{
		throw new NotSupportedException();
	}

	Task IAudioChannel.ModifyAsync(Action<AudioChannelProperties> func, RequestOptions options)
	{
		throw new NotSupportedException();
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
