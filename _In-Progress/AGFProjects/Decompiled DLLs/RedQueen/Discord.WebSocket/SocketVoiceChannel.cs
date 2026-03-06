using System;
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
internal class SocketVoiceChannel : SocketTextChannel, IVoiceChannel, IMessageChannel, IChannel, ISnowflakeEntity, IEntity<ulong>, INestedChannel, IGuildChannel, IDeletable, IAudioChannel, IMentionable, ISocketAudioChannel
{
	public virtual bool IsTextInVoice => true;

	public int Bitrate { get; private set; }

	public int? UserLimit { get; private set; }

	public string RTCRegion { get; private set; }

	public IReadOnlyCollection<SocketGuildUser> ConnectedUsers => base.Guild.Users.Where((SocketGuildUser x) => x.VoiceChannel?.Id == base.Id).ToImmutableArray();

	private string DebuggerDisplay => $"{base.Name} ({base.Id}, Voice)";

	internal SocketVoiceChannel(DiscordSocketClient discord, ulong id, SocketGuild guild)
		: base(discord, id, guild)
	{
	}

	internal new static SocketVoiceChannel Create(SocketGuild guild, ClientState state, Channel model)
	{
		SocketVoiceChannel socketVoiceChannel = new SocketVoiceChannel(guild?.Discord, model.Id, guild);
		socketVoiceChannel.Update(state, model);
		return socketVoiceChannel;
	}

	internal override void Update(ClientState state, Channel model)
	{
		base.Update(state, model);
		Bitrate = model.Bitrate.GetValueOrDefault(64000);
		UserLimit = ((model.UserLimit.GetValueOrDefault() != 0) ? new int?(model.UserLimit.Value) : ((int?)null));
		RTCRegion = model.RTCRegion.GetValueOrDefault(null);
	}

	public Task ModifyAsync(Action<VoiceChannelProperties> func, RequestOptions options = null)
	{
		return ChannelHelper.ModifyAsync(this, base.Discord, func, options);
	}

	public async Task<IAudioClient> ConnectAsync(bool selfDeaf = false, bool selfMute = false, bool external = false)
	{
		return await base.Guild.ConnectAudioAsync(base.Id, selfDeaf, selfMute, external).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task DisconnectAsync()
	{
		await base.Guild.DisconnectAudioAsync();
	}

	public async Task ModifyAsync(Action<AudioChannelProperties> func, RequestOptions options = null)
	{
		await base.Guild.ModifyAudioAsync(base.Id, func, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public override SocketGuildUser GetUser(ulong id)
	{
		SocketGuildUser user = base.Guild.GetUser(id);
		if (user?.VoiceChannel?.Id == base.Id)
		{
			return user;
		}
		return null;
	}

	public override Task<SocketThreadChannel> CreateThreadAsync(string name, ThreadType type = ThreadType.PublicThread, ThreadArchiveDuration autoArchiveDuration = ThreadArchiveDuration.OneDay, IMessage message = null, bool? invitable = null, int? slowmode = null, RequestOptions options = null)
	{
		throw new InvalidOperationException("Voice channels cannot contain threads.");
	}

	public override Task ModifyAsync(Action<TextChannelProperties> func, RequestOptions options = null)
	{
		throw new InvalidOperationException("Cannot modify text channel properties for voice channels.");
	}

	public override Task<IMessage> GetMessageAsync(ulong id, RequestOptions options = null)
	{
		if (!IsTextInVoice)
		{
			throw new NotSupportedException("This function is only supported in Text-In-Voice channels");
		}
		return base.GetMessageAsync(id, options);
	}

	public override Task DeleteMessageAsync(IMessage message, RequestOptions options = null)
	{
		if (!IsTextInVoice)
		{
			throw new NotSupportedException("This function is only supported in Text-In-Voice channels");
		}
		return base.DeleteMessageAsync(message, options);
	}

	public override Task DeleteMessageAsync(ulong messageId, RequestOptions options = null)
	{
		if (!IsTextInVoice)
		{
			throw new NotSupportedException("This function is only supported in Text-In-Voice channels");
		}
		return base.DeleteMessageAsync(messageId, options);
	}

	public override Task DeleteMessagesAsync(IEnumerable<IMessage> messages, RequestOptions options = null)
	{
		if (!IsTextInVoice)
		{
			throw new NotSupportedException("This function is only supported in Text-In-Voice channels");
		}
		return base.DeleteMessagesAsync(messages, options);
	}

	public override Task DeleteMessagesAsync(IEnumerable<ulong> messageIds, RequestOptions options = null)
	{
		if (!IsTextInVoice)
		{
			throw new NotSupportedException("This function is only supported in Text-In-Voice channels");
		}
		return base.DeleteMessagesAsync(messageIds, options);
	}

	public override IDisposable EnterTypingState(RequestOptions options = null)
	{
		if (!IsTextInVoice)
		{
			throw new NotSupportedException("This function is only supported in Text-In-Voice channels");
		}
		return base.EnterTypingState(options);
	}

	public override SocketMessage GetCachedMessage(ulong id)
	{
		if (!IsTextInVoice)
		{
			throw new NotSupportedException("This function is only supported in Text-In-Voice channels");
		}
		return base.GetCachedMessage(id);
	}

	public override IReadOnlyCollection<SocketMessage> GetCachedMessages(IMessage fromMessage, Direction dir, int limit = 100)
	{
		if (!IsTextInVoice)
		{
			throw new NotSupportedException("This function is only supported in Text-In-Voice channels");
		}
		return base.GetCachedMessages(fromMessage, dir, limit);
	}

	public override IReadOnlyCollection<SocketMessage> GetCachedMessages(int limit = 100)
	{
		if (!IsTextInVoice)
		{
			throw new NotSupportedException("This function is only supported in Text-In-Voice channels");
		}
		return base.GetCachedMessages(limit);
	}

	public override IReadOnlyCollection<SocketMessage> GetCachedMessages(ulong fromMessageId, Direction dir, int limit = 100)
	{
		if (!IsTextInVoice)
		{
			throw new NotSupportedException("This function is only supported in Text-In-Voice channels");
		}
		return base.GetCachedMessages(fromMessageId, dir, limit);
	}

	public override IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(IMessage fromMessage, Direction dir, int limit = 100, RequestOptions options = null)
	{
		if (!IsTextInVoice)
		{
			throw new NotSupportedException("This function is only supported in Text-In-Voice channels");
		}
		return base.GetMessagesAsync(fromMessage, dir, limit, options);
	}

	public override IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(int limit = 100, RequestOptions options = null)
	{
		if (!IsTextInVoice)
		{
			throw new NotSupportedException("This function is only supported in Text-In-Voice channels");
		}
		return base.GetMessagesAsync(limit, options);
	}

	public override IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(ulong fromMessageId, Direction dir, int limit = 100, RequestOptions options = null)
	{
		if (!IsTextInVoice)
		{
			throw new NotSupportedException("This function is only supported in Text-In-Voice channels");
		}
		return base.GetMessagesAsync(fromMessageId, dir, limit, options);
	}

	public override Task<IReadOnlyCollection<RestMessage>> GetPinnedMessagesAsync(RequestOptions options = null)
	{
		if (!IsTextInVoice)
		{
			throw new NotSupportedException("This function is only supported in Text-In-Voice channels");
		}
		return base.GetPinnedMessagesAsync(options);
	}

	public override Task<RestWebhook> GetWebhookAsync(ulong id, RequestOptions options = null)
	{
		if (!IsTextInVoice)
		{
			throw new NotSupportedException("This function is only supported in Text-In-Voice channels");
		}
		return base.GetWebhookAsync(id, options);
	}

	public override Task<IReadOnlyCollection<RestWebhook>> GetWebhooksAsync(RequestOptions options = null)
	{
		if (!IsTextInVoice)
		{
			throw new NotSupportedException("This function is only supported in Text-In-Voice channels");
		}
		return base.GetWebhooksAsync(options);
	}

	public override Task<RestWebhook> CreateWebhookAsync(string name, Stream avatar = null, RequestOptions options = null)
	{
		if (!IsTextInVoice)
		{
			throw new NotSupportedException("This function is only supported in Text-In-Voice channels");
		}
		return base.CreateWebhookAsync(name, avatar, options);
	}

	public override Task<IUserMessage> ModifyMessageAsync(ulong messageId, Action<MessageProperties> func, RequestOptions options = null)
	{
		if (!IsTextInVoice)
		{
			throw new NotSupportedException("This function is only supported in Text-In-Voice channels");
		}
		return base.ModifyMessageAsync(messageId, func, options);
	}

	public override Task<RestUserMessage> SendFileAsync(FileAttachment attachment, string text, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageReference messageReference = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None)
	{
		if (!IsTextInVoice)
		{
			throw new NotSupportedException("This function is only supported in Text-In-Voice channels");
		}
		return base.SendFileAsync(attachment, text, isTTS, embed, options, allowedMentions, messageReference, components, stickers, embeds, flags);
	}

	public override Task<RestUserMessage> SendFileAsync(Stream stream, string filename, string text, bool isTTS = false, Embed embed = null, RequestOptions options = null, bool isSpoiler = false, AllowedMentions allowedMentions = null, MessageReference messageReference = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None)
	{
		if (!IsTextInVoice)
		{
			throw new NotSupportedException("This function is only supported in Text-In-Voice channels");
		}
		return base.SendFileAsync(stream, filename, text, isTTS, embed, options, isSpoiler, allowedMentions, messageReference, components, stickers, embeds, flags);
	}

	public override Task<RestUserMessage> SendFileAsync(string filePath, string text, bool isTTS = false, Embed embed = null, RequestOptions options = null, bool isSpoiler = false, AllowedMentions allowedMentions = null, MessageReference messageReference = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None)
	{
		if (!IsTextInVoice)
		{
			throw new NotSupportedException("This function is only supported in Text-In-Voice channels");
		}
		return base.SendFileAsync(filePath, text, isTTS, embed, options, isSpoiler, allowedMentions, messageReference, components, stickers, embeds, flags);
	}

	public override Task<RestUserMessage> SendFilesAsync(IEnumerable<FileAttachment> attachments, string text, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageReference messageReference = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None)
	{
		if (!IsTextInVoice)
		{
			throw new NotSupportedException("This function is only supported in Text-In-Voice channels");
		}
		return base.SendFilesAsync(attachments, text, isTTS, embed, options, allowedMentions, messageReference, components, stickers, embeds, flags);
	}

	public override Task<RestUserMessage> SendMessageAsync(string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageReference messageReference = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None)
	{
		if (!IsTextInVoice)
		{
			throw new NotSupportedException("This function is only supported in Text-In-Voice channels");
		}
		return base.SendMessageAsync(text, isTTS, embed, options, allowedMentions, messageReference, components, stickers, embeds, flags);
	}

	public override Task TriggerTypingAsync(RequestOptions options = null)
	{
		if (!IsTextInVoice)
		{
			throw new NotSupportedException("This function is only supported in Text-In-Voice channels");
		}
		return base.TriggerTypingAsync(options);
	}

	internal new SocketVoiceChannel Clone()
	{
		return MemberwiseClone() as SocketVoiceChannel;
	}

	Task<IGuildUser> IGuildChannel.GetUserAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		return Task.FromResult((IGuildUser)GetUser(id));
	}

	IAsyncEnumerable<IReadOnlyCollection<IGuildUser>> IGuildChannel.GetUsersAsync(CacheMode mode, RequestOptions options)
	{
		return System.Collections.Immutable.ImmutableArray.Create((IReadOnlyCollection<IGuildUser>)Users).ToAsyncEnumerable();
	}

	Task<ICategoryChannel> INestedChannel.GetCategoryAsync(CacheMode mode, RequestOptions options)
	{
		return Task.FromResult(base.Category);
	}
}
