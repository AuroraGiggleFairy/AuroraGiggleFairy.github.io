using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord.API;
using Discord.Audio;

namespace Discord.Rest;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class RestVoiceChannel : RestTextChannel, IVoiceChannel, IMessageChannel, IChannel, ISnowflakeEntity, IEntity<ulong>, INestedChannel, IGuildChannel, IDeletable, IAudioChannel, IMentionable, IRestAudioChannel
{
	public virtual bool IsTextInVoice => base.Guild.Features.HasTextInVoice;

	public int Bitrate { get; private set; }

	public int? UserLimit { get; private set; }

	public string RTCRegion { get; private set; }

	private string DebuggerDisplay => $"{base.Name} ({base.Id}, Voice)";

	internal RestVoiceChannel(BaseDiscordClient discord, IGuild guild, ulong id)
		: base(discord, guild, id)
	{
	}

	internal new static RestVoiceChannel Create(BaseDiscordClient discord, IGuild guild, Channel model)
	{
		RestVoiceChannel restVoiceChannel = new RestVoiceChannel(discord, guild, model.Id);
		restVoiceChannel.Update(model);
		return restVoiceChannel;
	}

	internal override void Update(Channel model)
	{
		base.Update(model);
		if (model.Bitrate.IsSpecified)
		{
			Bitrate = model.Bitrate.Value;
		}
		if (model.UserLimit.IsSpecified)
		{
			UserLimit = ((model.UserLimit.Value != 0) ? new int?(model.UserLimit.Value) : ((int?)null));
		}
		RTCRegion = model.RTCRegion.GetValueOrDefault(null);
	}

	public async Task ModifyAsync(Action<VoiceChannelProperties> func, RequestOptions options = null)
	{
		Update(await ChannelHelper.ModifyAsync(this, base.Discord, func, options).ConfigureAwait(continueOnCapturedContext: false));
	}

	public override Task ModifyAsync(Action<TextChannelProperties> func, RequestOptions options = null)
	{
		throw new InvalidOperationException("Cannot modify text channel properties of a voice channel");
	}

	public override Task<RestThreadChannel> CreateThreadAsync(string name, ThreadType type = ThreadType.PublicThread, ThreadArchiveDuration autoArchiveDuration = ThreadArchiveDuration.OneDay, IMessage message = null, bool? invitable = null, int? slowmode = null, RequestOptions options = null)
	{
		throw new InvalidOperationException("Cannot create a thread within a voice channel");
	}

	public override Task<RestMessage> GetMessageAsync(ulong id, RequestOptions options = null)
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

	public override IAsyncEnumerable<IReadOnlyCollection<RestMessage>> GetMessagesAsync(IMessage fromMessage, Direction dir, int limit = 100, RequestOptions options = null)
	{
		if (!IsTextInVoice)
		{
			throw new NotSupportedException("This function is only supported in Text-In-Voice channels");
		}
		return base.GetMessagesAsync(fromMessage, dir, limit, options);
	}

	public override IAsyncEnumerable<IReadOnlyCollection<RestMessage>> GetMessagesAsync(int limit = 100, RequestOptions options = null)
	{
		if (!IsTextInVoice)
		{
			throw new NotSupportedException("This function is only supported in Text-In-Voice channels");
		}
		return base.GetMessagesAsync(limit, options);
	}

	public override IAsyncEnumerable<IReadOnlyCollection<RestMessage>> GetMessagesAsync(ulong fromMessageId, Direction dir, int limit = 100, RequestOptions options = null)
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

	Task<IGuildUser> IGuildChannel.GetUserAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		return Task.FromResult<IGuildUser>(null);
	}

	IAsyncEnumerable<IReadOnlyCollection<IGuildUser>> IGuildChannel.GetUsersAsync(CacheMode mode, RequestOptions options)
	{
		return AsyncEnumerable.Empty<IReadOnlyCollection<IGuildUser>>();
	}

	async Task<ICategoryChannel> INestedChannel.GetCategoryAsync(CacheMode mode, RequestOptions options)
	{
		if (base.CategoryId.HasValue && mode == CacheMode.AllowDownload)
		{
			return (await base.Guild.GetChannelAsync(base.CategoryId.Value, mode, options).ConfigureAwait(continueOnCapturedContext: false)) as ICategoryChannel;
		}
		return null;
	}
}
