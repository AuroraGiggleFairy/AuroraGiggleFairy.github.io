using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord.API;
using Discord.Audio;

namespace Discord.Rest;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class RestGroupChannel : RestChannel, IGroupChannel, IMessageChannel, IChannel, ISnowflakeEntity, IEntity<ulong>, IPrivateChannel, IAudioChannel, IRestPrivateChannel, IRestMessageChannel, IRestAudioChannel
{
	private string _iconId;

	private ImmutableDictionary<ulong, RestGroupUser> _users;

	public string Name { get; private set; }

	public string RTCRegion { get; private set; }

	public IReadOnlyCollection<RestGroupUser> Users => _users.ToReadOnlyCollection();

	public IReadOnlyCollection<RestGroupUser> Recipients => (from x in _users
		select x.Value into x
		where x.Id != base.Discord.CurrentUser.Id
		select x).ToReadOnlyCollection(() => _users.Count - 1);

	private string DebuggerDisplay => $"{Name} ({base.Id}, Group)";

	IReadOnlyCollection<RestUser> IRestPrivateChannel.Recipients => Recipients;

	IReadOnlyCollection<IUser> IPrivateChannel.Recipients => Recipients;

	internal RestGroupChannel(BaseDiscordClient discord, ulong id)
		: base(discord, id)
	{
	}

	internal new static RestGroupChannel Create(BaseDiscordClient discord, Channel model)
	{
		RestGroupChannel restGroupChannel = new RestGroupChannel(discord, model.Id);
		restGroupChannel.Update(model);
		return restGroupChannel;
	}

	internal override void Update(Channel model)
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
			UpdateUsers(model.Recipients.Value);
		}
		RTCRegion = model.RTCRegion.GetValueOrDefault(null);
	}

	internal void UpdateUsers(User[] models)
	{
		ImmutableDictionary<ulong, RestGroupUser>.Builder builder = ImmutableDictionary.CreateBuilder<ulong, RestGroupUser>();
		for (int i = 0; i < models.Length; i++)
		{
			builder[models[i].Id] = RestGroupUser.Create(base.Discord, models[i]);
		}
		_users = builder.ToImmutable();
	}

	public override async Task UpdateAsync(RequestOptions options = null)
	{
		Update(await base.Discord.ApiClient.GetChannelAsync(base.Id, options).ConfigureAwait(continueOnCapturedContext: false));
	}

	public Task LeaveAsync(RequestOptions options = null)
	{
		return ChannelHelper.DeleteAsync(this, base.Discord, options);
	}

	public RestUser GetUser(ulong id)
	{
		if (_users.TryGetValue(id, out var value))
		{
			return value;
		}
		return null;
	}

	public Task<RestMessage> GetMessageAsync(ulong id, RequestOptions options = null)
	{
		return ChannelHelper.GetMessageAsync(this, base.Discord, id, options);
	}

	public IAsyncEnumerable<IReadOnlyCollection<RestMessage>> GetMessagesAsync(int limit = 100, RequestOptions options = null)
	{
		return ChannelHelper.GetMessagesAsync(this, base.Discord, null, Direction.Before, limit, options);
	}

	public IAsyncEnumerable<IReadOnlyCollection<RestMessage>> GetMessagesAsync(ulong fromMessageId, Direction dir, int limit = 100, RequestOptions options = null)
	{
		return ChannelHelper.GetMessagesAsync(this, base.Discord, fromMessageId, dir, limit, options);
	}

	public IAsyncEnumerable<IReadOnlyCollection<RestMessage>> GetMessagesAsync(IMessage fromMessage, Direction dir, int limit = 100, RequestOptions options = null)
	{
		return ChannelHelper.GetMessagesAsync(this, base.Discord, fromMessage.Id, dir, limit, options);
	}

	public Task<IReadOnlyCollection<RestMessage>> GetPinnedMessagesAsync(RequestOptions options = null)
	{
		return ChannelHelper.GetPinnedMessagesAsync(this, base.Discord, options);
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

	public Task TriggerTypingAsync(RequestOptions options = null)
	{
		return ChannelHelper.TriggerTypingAsync(this, base.Discord, options);
	}

	public IDisposable EnterTypingState(RequestOptions options = null)
	{
		return ChannelHelper.EnterTypingState(this, base.Discord, options);
	}

	public override string ToString()
	{
		return Name;
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
