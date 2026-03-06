using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.API;
using Discord.API.Rest;
using Discord.Rest;

namespace Discord.WebSocket;

internal class SocketModal : SocketInteraction, IDiscordInteraction, ISnowflakeEntity, IEntity<ulong>, IModalInteraction
{
	private object _lock = new object();

	public new SocketModalData Data { get; set; }

	public override bool HasResponded { get; internal set; }

	IModalInteractionData IModalInteraction.Data => Data;

	internal SocketModal(DiscordSocketClient client, Interaction model, ISocketMessageChannel channel, SocketUser user)
		: base(client, model.Id, channel, user)
	{
		Data = new SocketModalData(model.Data.IsSpecified ? ((ModalInteractionData)model.Data.Value) : null);
	}

	internal new static SocketModal Create(DiscordSocketClient client, Interaction model, ISocketMessageChannel channel, SocketUser user)
	{
		SocketModal socketModal = new SocketModal(client, model, channel, user);
		socketModal.Update(model);
		return socketModal;
	}

	public override async Task RespondWithFilesAsync(IEnumerable<FileAttachment> attachments, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
	{
		if (!base.IsValidToken)
		{
			throw new InvalidOperationException("Interaction token is no longer valid");
		}
		if (!InteractionHelper.CanSendResponse(this))
		{
			throw new TimeoutException($"Cannot respond to an interaction after {3.0} seconds!");
		}
		if (embeds == null)
		{
			embeds = Array.Empty<Embed>();
		}
		if (embed != null)
		{
			embeds = new Embed[1] { embed }.Concat(embeds).ToArray();
		}
		Preconditions.AtMost((allowedMentions?.RoleIds?.Count).GetValueOrDefault(), 100, "RoleIds", "A max of 100 role Ids are allowed.");
		Preconditions.AtMost((allowedMentions?.UserIds?.Count).GetValueOrDefault(), 100, "UserIds", "A max of 100 user Ids are allowed.");
		Preconditions.AtMost(embeds.Length, 10, "embeds", "A max of 10 embeds are allowed.");
		if (allowedMentions != null && allowedMentions.AllowedTypes.HasValue)
		{
			if (allowedMentions.AllowedTypes.Value.HasFlag(AllowedMentionTypes.Users) && allowedMentions.UserIds != null && allowedMentions.UserIds.Count > 0)
			{
				throw new ArgumentException("The Users flag is mutually exclusive with the list of User Ids.", "allowedMentions");
			}
			if (allowedMentions.AllowedTypes.Value.HasFlag(AllowedMentionTypes.Roles) && allowedMentions.RoleIds != null && allowedMentions.RoleIds.Count > 0)
			{
				throw new ArgumentException("The Roles flag is mutually exclusive with the list of Role Ids.", "allowedMentions");
			}
		}
		UploadInteractionFileParams obj = new UploadInteractionFileParams(attachments?.ToArray())
		{
			Type = InteractionResponseType.ChannelMessageWithSource
		};
		obj.Content = ((text != null) ? ((Optional<string>)text) : Optional<string>.Unspecified);
		obj.AllowedMentions = ((allowedMentions != null) ? ((Optional<global::Discord.API.AllowedMentions>)(allowedMentions?.ToModel())) : Optional<global::Discord.API.AllowedMentions>.Unspecified);
		obj.Embeds = (embeds.Any() ? ((Optional<global::Discord.API.Embed[]>)embeds.Select((Embed x) => x.ToModel()).ToArray()) : Optional<global::Discord.API.Embed[]>.Unspecified);
		obj.IsTTS = isTTS;
		global::Discord.API.ActionRowComponent[] array = components?.Components.Select((ActionRowComponent x) => new global::Discord.API.ActionRowComponent(x)).ToArray();
		obj.MessageComponents = ((array != null) ? ((Optional<global::Discord.API.ActionRowComponent[]>)array) : Optional<global::Discord.API.ActionRowComponent[]>.Unspecified);
		obj.Flags = (ephemeral ? ((Optional<MessageFlags>)MessageFlags.Ephemeral) : Optional<MessageFlags>.Unspecified);
		UploadInteractionFileParams response = obj;
		lock (_lock)
		{
			if (HasResponded)
			{
				throw new InvalidOperationException("Cannot respond, update, or defer the same interaction twice");
			}
		}
		await InteractionHelper.SendInteractionResponseAsync(base.Discord, response, this, base.Channel, options).ConfigureAwait(continueOnCapturedContext: false);
		HasResponded = true;
	}

	public override async Task RespondAsync(string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
	{
		if (!base.IsValidToken)
		{
			throw new InvalidOperationException("Interaction token is no longer valid");
		}
		if (!InteractionHelper.CanSendResponse(this))
		{
			throw new TimeoutException($"Cannot respond to an interaction after {3.0} seconds!");
		}
		if (embeds == null)
		{
			embeds = Array.Empty<Embed>();
		}
		if (embed != null)
		{
			embeds = new Embed[1] { embed }.Concat(embeds).ToArray();
		}
		Preconditions.AtMost((allowedMentions?.RoleIds?.Count).GetValueOrDefault(), 100, "RoleIds", "A max of 100 role Ids are allowed.");
		Preconditions.AtMost((allowedMentions?.UserIds?.Count).GetValueOrDefault(), 100, "UserIds", "A max of 100 user Ids are allowed.");
		Preconditions.AtMost(embeds.Length, 10, "embeds", "A max of 10 embeds are allowed.");
		if (allowedMentions != null && allowedMentions.AllowedTypes.HasValue)
		{
			if (allowedMentions.AllowedTypes.Value.HasFlag(AllowedMentionTypes.Users) && allowedMentions.UserIds != null && allowedMentions.UserIds.Count > 0)
			{
				throw new ArgumentException("The Users flag is mutually exclusive with the list of User Ids.", "allowedMentions");
			}
			if (allowedMentions.AllowedTypes.Value.HasFlag(AllowedMentionTypes.Roles) && allowedMentions.RoleIds != null && allowedMentions.RoleIds.Count > 0)
			{
				throw new ArgumentException("The Roles flag is mutually exclusive with the list of Role Ids.", "allowedMentions");
			}
		}
		InteractionResponse obj = new InteractionResponse
		{
			Type = InteractionResponseType.ChannelMessageWithSource
		};
		InteractionCallbackData interactionCallbackData = new InteractionCallbackData();
		interactionCallbackData.Content = ((text != null) ? ((Optional<string>)text) : Optional<string>.Unspecified);
		interactionCallbackData.AllowedMentions = allowedMentions?.ToModel();
		interactionCallbackData.Embeds = embeds.Select((Embed x) => x.ToModel()).ToArray();
		interactionCallbackData.TTS = isTTS;
		interactionCallbackData.Flags = (ephemeral ? ((Optional<MessageFlags>)MessageFlags.Ephemeral) : Optional<MessageFlags>.Unspecified);
		global::Discord.API.ActionRowComponent[] array = components?.Components.Select((ActionRowComponent x) => new global::Discord.API.ActionRowComponent(x)).ToArray();
		interactionCallbackData.Components = ((array != null) ? ((Optional<global::Discord.API.ActionRowComponent[]>)array) : Optional<global::Discord.API.ActionRowComponent[]>.Unspecified);
		obj.Data = interactionCallbackData;
		InteractionResponse response = obj;
		lock (_lock)
		{
			if (HasResponded)
			{
				throw new InvalidOperationException("Cannot respond, update, or defer twice to the same interaction");
			}
		}
		await InteractionHelper.SendInteractionResponseAsync(base.Discord, response, this, base.Channel, options).ConfigureAwait(continueOnCapturedContext: false);
		HasResponded = true;
	}

	public async Task UpdateAsync(Action<MessageProperties> func, RequestOptions options = null)
	{
		MessageProperties messageProperties = new MessageProperties();
		func(messageProperties);
		if (!base.IsValidToken)
		{
			throw new InvalidOperationException("Interaction token is no longer valid");
		}
		if (!InteractionHelper.CanSendResponse(this))
		{
			throw new TimeoutException($"Cannot respond to an interaction after {3.0} seconds!");
		}
		if (messageProperties.AllowedMentions.IsSpecified)
		{
			AllowedMentions value = messageProperties.AllowedMentions.Value;
			Preconditions.AtMost((value?.RoleIds?.Count).GetValueOrDefault(), 100, "allowedMentions", "A max of 100 role Ids are allowed.");
			Preconditions.AtMost((value?.UserIds?.Count).GetValueOrDefault(), 100, "allowedMentions", "A max of 100 user Ids are allowed.");
		}
		Optional<Embed> embed = messageProperties.Embed;
		Optional<Embed[]> embeds = messageProperties.Embeds;
		bool num = messageProperties.Content.IsSpecified && !string.IsNullOrEmpty(messageProperties.Content.Value);
		int num2;
		if (!embed.IsSpecified || !(embed.Value != null))
		{
			if (embeds.IsSpecified)
			{
				Embed[] value2 = embeds.Value;
				num2 = ((value2 != null && value2.Length != 0) ? 1 : 0);
			}
			else
			{
				num2 = 0;
			}
		}
		else
		{
			num2 = 1;
		}
		bool flag = (byte)num2 != 0;
		if (!num && !flag)
		{
			Preconditions.NotNullOrEmpty(messageProperties.Content.IsSpecified ? messageProperties.Content.Value : string.Empty, "Content");
		}
		List<global::Discord.API.Embed> list = ((embed.IsSpecified || embeds.IsSpecified) ? new List<global::Discord.API.Embed>() : null);
		if (embed.IsSpecified && embed.Value != null)
		{
			list.Add(embed.Value.ToModel());
		}
		if (embeds.IsSpecified && embeds.Value != null)
		{
			list.AddRange(embeds.Value.Select((Embed x) => x.ToModel()));
		}
		Preconditions.AtMost(list?.Count ?? 0, 10, "Embeds", "A max of 10 embeds are allowed.");
		if (messageProperties.AllowedMentions.IsSpecified && messageProperties.AllowedMentions.Value != null && messageProperties.AllowedMentions.Value.AllowedTypes.HasValue)
		{
			AllowedMentions value3 = messageProperties.AllowedMentions.Value;
			if (value3.AllowedTypes.Value.HasFlag(AllowedMentionTypes.Users) && value3.UserIds != null && value3.UserIds.Count > 0)
			{
				throw new ArgumentException("The Users flag is mutually exclusive with the list of User Ids.", "AllowedMentions");
			}
			if (value3.AllowedTypes.Value.HasFlag(AllowedMentionTypes.Roles) && value3.RoleIds != null && value3.RoleIds.Count > 0)
			{
				throw new ArgumentException("The Roles flag is mutually exclusive with the list of Role Ids.", "AllowedMentions");
			}
		}
		InteractionResponse obj = new InteractionResponse
		{
			Type = InteractionResponseType.UpdateMessage
		};
		InteractionCallbackData obj2 = new InteractionCallbackData
		{
			Content = messageProperties.Content,
			AllowedMentions = (messageProperties.AllowedMentions.IsSpecified ? ((Optional<global::Discord.API.AllowedMentions>)(messageProperties.AllowedMentions.Value?.ToModel())) : Optional<global::Discord.API.AllowedMentions>.Unspecified)
		};
		global::Discord.API.Embed[] array = list?.ToArray();
		obj2.Embeds = ((array != null) ? ((Optional<global::Discord.API.Embed[]>)array) : Optional<global::Discord.API.Embed[]>.Unspecified);
		obj2.Components = (messageProperties.Components.IsSpecified ? ((Optional<global::Discord.API.ActionRowComponent[]>)(messageProperties.Components.Value?.Components.Select((ActionRowComponent x) => new global::Discord.API.ActionRowComponent(x)).ToArray() ?? Array.Empty<global::Discord.API.ActionRowComponent>())) : Optional<global::Discord.API.ActionRowComponent[]>.Unspecified);
		obj2.Flags = ((!messageProperties.Flags.IsSpecified) ? Optional<MessageFlags>.Unspecified : (((Optional<MessageFlags>?)messageProperties.Flags.Value) ?? Optional<MessageFlags>.Unspecified));
		obj.Data = obj2;
		InteractionResponse response = obj;
		lock (_lock)
		{
			if (HasResponded)
			{
				throw new InvalidOperationException("Cannot respond, update, or defer twice to the same interaction");
			}
		}
		await InteractionHelper.SendInteractionResponseAsync(base.Discord, response, this, base.Channel, options).ConfigureAwait(continueOnCapturedContext: false);
		HasResponded = true;
	}

	public override async Task<RestFollowupMessage> FollowupAsync(string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
	{
		if (!base.IsValidToken)
		{
			throw new InvalidOperationException("Interaction token is no longer valid");
		}
		if (embeds == null)
		{
			embeds = Array.Empty<Embed>();
		}
		if (embed != null)
		{
			embeds = new Embed[1] { embed }.Concat(embeds).ToArray();
		}
		Preconditions.AtMost((allowedMentions?.RoleIds?.Count).GetValueOrDefault(), 100, "RoleIds", "A max of 100 role Ids are allowed.");
		Preconditions.AtMost((allowedMentions?.UserIds?.Count).GetValueOrDefault(), 100, "UserIds", "A max of 100 user Ids are allowed.");
		Preconditions.AtMost(embeds.Length, 10, "embeds", "A max of 10 embeds are allowed.");
		CreateWebhookMessageParams obj = new CreateWebhookMessageParams
		{
			Content = text
		};
		global::Discord.API.AllowedMentions allowedMentions2 = allowedMentions?.ToModel();
		obj.AllowedMentions = ((allowedMentions2 != null) ? ((Optional<global::Discord.API.AllowedMentions>)allowedMentions2) : Optional<global::Discord.API.AllowedMentions>.Unspecified);
		obj.IsTTS = isTTS;
		obj.Embeds = embeds.Select((Embed x) => x.ToModel()).ToArray();
		global::Discord.API.ActionRowComponent[] array = components?.Components.Select((ActionRowComponent x) => new global::Discord.API.ActionRowComponent(x)).ToArray();
		obj.Components = ((array != null) ? ((Optional<global::Discord.API.ActionRowComponent[]>)array) : Optional<global::Discord.API.ActionRowComponent[]>.Unspecified);
		CreateWebhookMessageParams createWebhookMessageParams = obj;
		if (ephemeral)
		{
			createWebhookMessageParams.Flags = MessageFlags.Ephemeral;
		}
		return await InteractionHelper.SendFollowupAsync(base.Discord.Rest, createWebhookMessageParams, base.Token, base.Channel, options);
	}

	public override async Task<RestFollowupMessage> FollowupWithFilesAsync(IEnumerable<FileAttachment> attachments, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
	{
		if (!base.IsValidToken)
		{
			throw new InvalidOperationException("Interaction token is no longer valid");
		}
		if (embeds == null)
		{
			embeds = Array.Empty<Embed>();
		}
		if (embed != null)
		{
			embeds = new Embed[1] { embed }.Concat(embeds).ToArray();
		}
		Preconditions.AtMost((allowedMentions?.RoleIds?.Count).GetValueOrDefault(), 100, "RoleIds", "A max of 100 role Ids are allowed.");
		Preconditions.AtMost((allowedMentions?.UserIds?.Count).GetValueOrDefault(), 100, "UserIds", "A max of 100 user Ids are allowed.");
		Preconditions.AtMost(embeds.Length, 10, "embeds", "A max of 10 embeds are allowed.");
		foreach (FileAttachment attachment in attachments)
		{
			Preconditions.NotNullOrEmpty(attachment.FileName, "FileName", "File Name must not be empty or null");
		}
		if (allowedMentions != null && allowedMentions.AllowedTypes.HasValue)
		{
			if (allowedMentions.AllowedTypes.Value.HasFlag(AllowedMentionTypes.Users) && allowedMentions.UserIds != null && allowedMentions.UserIds.Count > 0)
			{
				throw new ArgumentException("The Users flag is mutually exclusive with the list of User Ids.", "allowedMentions");
			}
			if (allowedMentions.AllowedTypes.Value.HasFlag(AllowedMentionTypes.Roles) && allowedMentions.RoleIds != null && allowedMentions.RoleIds.Count > 0)
			{
				throw new ArgumentException("The Roles flag is mutually exclusive with the list of Role Ids.", "allowedMentions");
			}
		}
		MessageFlags messageFlags = MessageFlags.None;
		if (ephemeral)
		{
			messageFlags |= MessageFlags.Ephemeral;
		}
		UploadWebhookFileParams obj = new UploadWebhookFileParams(attachments.ToArray())
		{
			Flags = messageFlags,
			Content = text,
			IsTTS = isTTS,
			Embeds = (embeds.Any() ? ((Optional<global::Discord.API.Embed[]>)embeds.Select((Embed x) => x.ToModel()).ToArray()) : Optional<global::Discord.API.Embed[]>.Unspecified)
		};
		global::Discord.API.AllowedMentions allowedMentions2 = allowedMentions?.ToModel();
		obj.AllowedMentions = ((allowedMentions2 != null) ? ((Optional<global::Discord.API.AllowedMentions>)allowedMentions2) : Optional<global::Discord.API.AllowedMentions>.Unspecified);
		global::Discord.API.ActionRowComponent[] array = components?.Components.Select((ActionRowComponent x) => new global::Discord.API.ActionRowComponent(x)).ToArray();
		obj.MessageComponents = ((array != null) ? ((Optional<global::Discord.API.ActionRowComponent[]>)array) : Optional<global::Discord.API.ActionRowComponent[]>.Unspecified);
		UploadWebhookFileParams args = obj;
		return await InteractionHelper.SendFollowupAsync(base.Discord, args, base.Token, base.Channel, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public override async Task DeferAsync(bool ephemeral = false, RequestOptions options = null)
	{
		if (!InteractionHelper.CanSendResponse(this))
		{
			throw new TimeoutException($"Cannot defer an interaction after {3.0} seconds of no response/acknowledgement");
		}
		InteractionResponse response = new InteractionResponse
		{
			Type = InteractionResponseType.DeferredUpdateMessage,
			Data = (ephemeral ? ((Optional<InteractionCallbackData>)new InteractionCallbackData
			{
				Flags = MessageFlags.Ephemeral
			}) : Optional<InteractionCallbackData>.Unspecified)
		};
		lock (_lock)
		{
			if (HasResponded)
			{
				throw new InvalidOperationException("Cannot respond or defer twice to the same interaction");
			}
		}
		await base.Discord.Rest.ApiClient.CreateInteractionResponseAsync(response, base.Id, base.Token, options).ConfigureAwait(continueOnCapturedContext: false);
		lock (_lock)
		{
			HasResponded = true;
		}
	}

	public override Task RespondWithModalAsync(Modal modal, RequestOptions options = null)
	{
		throw new NotSupportedException("You cannot respond to a modal with a modal!");
	}
}
