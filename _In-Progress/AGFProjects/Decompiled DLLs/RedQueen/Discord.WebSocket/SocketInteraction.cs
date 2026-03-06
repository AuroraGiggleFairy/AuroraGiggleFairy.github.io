using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Discord.API;
using Discord.Net;
using Discord.Rest;

namespace Discord.WebSocket;

internal abstract class SocketInteraction : SocketEntity<ulong>, IDiscordInteraction, ISnowflakeEntity, IEntity<ulong>
{
	public ISocketMessageChannel Channel { get; private set; }

	public ulong? ChannelId { get; private set; }

	public SocketUser User { get; private set; }

	public InteractionType Type { get; private set; }

	public string Token { get; private set; }

	public IDiscordInteractionData Data { get; private set; }

	public string UserLocale { get; private set; }

	public string GuildLocale { get; private set; }

	public int Version { get; private set; }

	public DateTimeOffset CreatedAt { get; private set; }

	public abstract bool HasResponded { get; internal set; }

	public bool IsValidToken => InteractionHelper.CanRespondOrFollowup(this);

	public bool IsDMInteraction { get; private set; }

	public ulong? GuildId { get; private set; }

	public ulong ApplicationId { get; private set; }

	IUser IDiscordInteraction.User => User;

	internal SocketInteraction(DiscordSocketClient client, ulong id, ISocketMessageChannel channel, SocketUser user)
		: base(client, id)
	{
		Channel = channel;
		User = user;
		CreatedAt = (client.UseInteractionSnowflakeDate ? SnowflakeUtils.FromSnowflake(base.Id) : ((DateTimeOffset)DateTime.UtcNow));
	}

	internal static SocketInteraction Create(DiscordSocketClient client, Interaction model, ISocketMessageChannel channel, SocketUser user)
	{
		if (model.Type == InteractionType.ApplicationCommand)
		{
			ApplicationCommandInteractionData applicationCommandInteractionData = (model.Data.IsSpecified ? ((ApplicationCommandInteractionData)model.Data.Value) : null);
			if (applicationCommandInteractionData == null)
			{
				return null;
			}
			return applicationCommandInteractionData.Type switch
			{
				ApplicationCommandType.Slash => SocketSlashCommand.Create(client, model, channel, user), 
				ApplicationCommandType.Message => SocketMessageCommand.Create(client, model, channel, user), 
				ApplicationCommandType.User => SocketUserCommand.Create(client, model, channel, user), 
				_ => null, 
			};
		}
		if (model.Type == InteractionType.MessageComponent)
		{
			return SocketMessageComponent.Create(client, model, channel, user);
		}
		if (model.Type == InteractionType.ApplicationCommandAutocomplete)
		{
			return SocketAutocompleteInteraction.Create(client, model, channel, user);
		}
		if (model.Type == InteractionType.ModalSubmit)
		{
			return SocketModal.Create(client, model, channel, user);
		}
		return null;
	}

	internal virtual void Update(Interaction model)
	{
		ChannelId = (model.ChannelId.IsSpecified ? new ulong?(model.ChannelId.Value) : ((ulong?)null));
		GuildId = (model.GuildId.IsSpecified ? new ulong?(model.GuildId.Value) : ((ulong?)null));
		IsDMInteraction = !GuildId.HasValue;
		ApplicationId = model.ApplicationId;
		Data = (model.Data.IsSpecified ? model.Data.Value : null);
		Token = model.Token;
		Version = model.Version;
		Type = model.Type;
		UserLocale = (model.UserLocale.IsSpecified ? model.UserLocale.Value : null);
		GuildLocale = (model.GuildLocale.IsSpecified ? model.GuildLocale.Value : null);
	}

	public abstract Task RespondAsync(string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null);

	public async Task RespondWithFileAsync(Stream fileStream, string fileName, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
	{
		using FileAttachment file = new FileAttachment(fileStream, fileName);
		await RespondWithFileAsync(file, text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task RespondWithFileAsync(string filePath, string fileName = null, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
	{
		using FileAttachment file = new FileAttachment(filePath, fileName);
		await RespondWithFileAsync(file, text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public Task RespondWithFileAsync(FileAttachment attachment, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
	{
		return RespondWithFilesAsync(new FileAttachment[1] { attachment }, text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options);
	}

	public abstract Task RespondWithFilesAsync(IEnumerable<FileAttachment> attachments, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null);

	public abstract Task<RestFollowupMessage> FollowupAsync(string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null);

	public async Task<RestFollowupMessage> FollowupWithFileAsync(Stream fileStream, string fileName, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
	{
		using FileAttachment file = new FileAttachment(fileStream, fileName);
		return await FollowupWithFileAsync(file, text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<RestFollowupMessage> FollowupWithFileAsync(string filePath, string fileName = null, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
	{
		using FileAttachment file = new FileAttachment(filePath, fileName);
		return await FollowupWithFileAsync(file, text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public Task<RestFollowupMessage> FollowupWithFileAsync(FileAttachment attachment, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
	{
		return FollowupWithFilesAsync(new FileAttachment[1] { attachment }, text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options);
	}

	public abstract Task<RestFollowupMessage> FollowupWithFilesAsync(IEnumerable<FileAttachment> attachments, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null);

	public Task<RestInteractionMessage> GetOriginalResponseAsync(RequestOptions options = null)
	{
		return InteractionHelper.GetOriginalResponseAsync(base.Discord, Channel, this, options);
	}

	public async Task<RestInteractionMessage> ModifyOriginalResponseAsync(Action<MessageProperties> func, RequestOptions options = null)
	{
		Message model = await InteractionHelper.ModifyInteractionResponseAsync(base.Discord, Token, func, options);
		return RestInteractionMessage.Create(base.Discord, model, Token, Channel);
	}

	public Task DeleteOriginalResponseAsync(RequestOptions options = null)
	{
		return InteractionHelper.DeleteInteractionResponseAsync(base.Discord, this, options);
	}

	public abstract Task DeferAsync(bool ephemeral = false, RequestOptions options = null);

	public abstract Task RespondWithModalAsync(Modal modal, RequestOptions options = null);

	public async ValueTask<IMessageChannel> GetChannelAsync(RequestOptions options = null)
	{
		if (Channel != null)
		{
			return Channel;
		}
		if (!ChannelId.HasValue)
		{
			return null;
		}
		try
		{
			return (IMessageChannel)(await base.Discord.GetChannelAsync(ChannelId.Value, options).ConfigureAwait(continueOnCapturedContext: false));
		}
		catch (HttpException ex) when (ex.DiscordCode == DiscordErrorCode.MissingPermissions)
		{
			return null;
		}
	}

	async Task<IUserMessage> IDiscordInteraction.GetOriginalResponseAsync(RequestOptions options)
	{
		return await GetOriginalResponseAsync(options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IUserMessage> IDiscordInteraction.ModifyOriginalResponseAsync(Action<MessageProperties> func, RequestOptions options)
	{
		return await ModifyOriginalResponseAsync(func, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task IDiscordInteraction.RespondAsync(string text, Embed[] embeds, bool isTTS, bool ephemeral, AllowedMentions allowedMentions, MessageComponent components, Embed embed, RequestOptions options)
	{
		await RespondAsync(text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IUserMessage> IDiscordInteraction.FollowupAsync(string text, Embed[] embeds, bool isTTS, bool ephemeral, AllowedMentions allowedMentions, MessageComponent components, Embed embed, RequestOptions options)
	{
		return await FollowupAsync(text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IUserMessage> IDiscordInteraction.FollowupWithFilesAsync(IEnumerable<FileAttachment> attachments, string text, Embed[] embeds, bool isTTS, bool ephemeral, AllowedMentions allowedMentions, MessageComponent components, Embed embed, RequestOptions options)
	{
		return await FollowupWithFilesAsync(attachments, text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IUserMessage> IDiscordInteraction.FollowupWithFileAsync(Stream fileStream, string fileName, string text, Embed[] embeds, bool isTTS, bool ephemeral, AllowedMentions allowedMentions, MessageComponent components, Embed embed, RequestOptions options)
	{
		return await FollowupWithFileAsync(fileStream, fileName, text, embeds, isTTS, ephemeral, allowedMentions, components, embed).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IUserMessage> IDiscordInteraction.FollowupWithFileAsync(string filePath, string fileName, string text, Embed[] embeds, bool isTTS, bool ephemeral, AllowedMentions allowedMentions, MessageComponent components, Embed embed, RequestOptions options)
	{
		return await FollowupWithFileAsync(filePath, fileName, text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IUserMessage> IDiscordInteraction.FollowupWithFileAsync(FileAttachment attachment, string text, Embed[] embeds, bool isTTS, bool ephemeral, AllowedMentions allowedMentions, MessageComponent components, Embed embed, RequestOptions options)
	{
		return await FollowupWithFileAsync(attachment, text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options).ConfigureAwait(continueOnCapturedContext: false);
	}
}
