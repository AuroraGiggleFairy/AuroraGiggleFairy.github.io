using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.API;
using Discord.API.Rest;
using Discord.Rest;

namespace Discord.WebSocket;

internal class SocketMessageComponent : SocketInteraction, IComponentInteraction, IDiscordInteraction, ISnowflakeEntity, IEntity<ulong>
{
	private object _lock = new object();

	public new SocketMessageComponentData Data { get; }

	public SocketUserMessage Message { get; private set; }

	public override bool HasResponded { get; internal set; }

	IComponentInteractionData IComponentInteraction.Data => Data;

	IUserMessage IComponentInteraction.Message => Message;

	IDiscordInteractionData IDiscordInteraction.Data => Data;

	internal SocketMessageComponent(DiscordSocketClient client, Interaction model, ISocketMessageChannel channel, SocketUser user)
		: base(client, model.Id, channel, user)
	{
		Data = new SocketMessageComponentData(model.Data.IsSpecified ? ((MessageComponentInteractionData)model.Data.Value) : null);
	}

	internal new static SocketMessageComponent Create(DiscordSocketClient client, Interaction model, ISocketMessageChannel channel, SocketUser user)
	{
		SocketMessageComponent socketMessageComponent = new SocketMessageComponent(client, model, channel, user);
		socketMessageComponent.Update(model);
		return socketMessageComponent;
	}

	internal override void Update(Interaction model)
	{
		base.Update(model);
		if (!model.Message.IsSpecified)
		{
			return;
		}
		if (Message == null)
		{
			SocketUser socketUser = null;
			if (base.Channel is SocketGuildChannel socketGuildChannel)
			{
				if (model.Message.Value.WebhookId.IsSpecified)
				{
					socketUser = SocketWebhookUser.Create(socketGuildChannel.Guild, base.Discord.State, model.Message.Value.Author.Value, model.Message.Value.WebhookId.Value);
				}
				else if (model.Message.Value.Author.IsSpecified)
				{
					socketUser = socketGuildChannel.Guild.GetUser(model.Message.Value.Author.Value.Id);
				}
			}
			else if (model.Message.Value.Author.IsSpecified)
			{
				socketUser = (base.Channel as SocketChannel)?.GetUser(model.Message.Value.Author.Value.Id);
			}
			if (socketUser == null)
			{
				socketUser = base.Discord.State.GetOrAddUser(model.Message.Value.Author.Value.Id, (ulong _) => SocketGlobalUser.Create(base.Discord, base.Discord.State, model.Message.Value.Author.Value));
			}
			Message = SocketUserMessage.Create(base.Discord, base.Discord.State, socketUser, base.Channel, model.Message.Value);
		}
		else
		{
			Message.Update(base.Discord.State, model.Message.Value);
		}
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
		bool flag = (messageProperties.Content.IsSpecified ? (!string.IsNullOrEmpty(messageProperties.Content.Value)) : (!string.IsNullOrEmpty(Message.Content)));
		if (embed.IsSpecified && embed.Value != null)
		{
			goto IL_01b1;
		}
		if (embeds.IsSpecified)
		{
			Embed[] value2 = embeds.Value;
			if (value2 != null && value2.Length != 0)
			{
				goto IL_01b1;
			}
		}
		int num = (Message.Embeds.Any() ? 1 : 0);
		goto IL_01b2;
		IL_01b1:
		num = 1;
		goto IL_01b2;
		IL_01b2:
		bool flag2 = (byte)num != 0;
		bool num2 = messageProperties.Components.IsSpecified && messageProperties.Components.Value != null;
		bool isSpecified = messageProperties.Attachments.IsSpecified;
		bool isSpecified2 = messageProperties.Flags.IsSpecified;
		if (!num2 && !flag && !flag2 && !isSpecified && !isSpecified2)
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
		if (!messageProperties.Attachments.IsSpecified)
		{
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
			await InteractionHelper.SendInteractionResponseAsync(base.Discord, response, this, base.Channel, options).ConfigureAwait(continueOnCapturedContext: false);
		}
		else
		{
			UploadInteractionFileParams obj3 = new UploadInteractionFileParams(messageProperties.Attachments.Value.ToArray())
			{
				Type = InteractionResponseType.UpdateMessage,
				Content = messageProperties.Content,
				AllowedMentions = (messageProperties.AllowedMentions.IsSpecified ? ((Optional<global::Discord.API.AllowedMentions>)(messageProperties.AllowedMentions.Value?.ToModel())) : Optional<global::Discord.API.AllowedMentions>.Unspecified)
			};
			global::Discord.API.Embed[] array = list?.ToArray();
			obj3.Embeds = ((array != null) ? ((Optional<global::Discord.API.Embed[]>)array) : Optional<global::Discord.API.Embed[]>.Unspecified);
			obj3.MessageComponents = (messageProperties.Components.IsSpecified ? ((Optional<global::Discord.API.ActionRowComponent[]>)(messageProperties.Components.Value?.Components.Select((ActionRowComponent x) => new global::Discord.API.ActionRowComponent(x)).ToArray() ?? Array.Empty<global::Discord.API.ActionRowComponent>())) : Optional<global::Discord.API.ActionRowComponent[]>.Unspecified);
			obj3.Flags = ((!messageProperties.Flags.IsSpecified) ? Optional<MessageFlags>.Unspecified : (((Optional<MessageFlags>?)messageProperties.Flags.Value) ?? Optional<MessageFlags>.Unspecified));
			UploadInteractionFileParams response2 = obj3;
			await InteractionHelper.SendInteractionResponseAsync(base.Discord, response2, this, base.Channel, options).ConfigureAwait(continueOnCapturedContext: false);
		}
		lock (_lock)
		{
			if (HasResponded)
			{
				throw new InvalidOperationException("Cannot respond, update, or defer twice to the same interaction");
			}
		}
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

	public async Task DeferLoadingAsync(bool ephemeral = false, RequestOptions options = null)
	{
		if (!InteractionHelper.CanSendResponse(this))
		{
			throw new TimeoutException($"Cannot defer an interaction after {3.0} seconds of no response/acknowledgement");
		}
		InteractionResponse response = new InteractionResponse
		{
			Type = InteractionResponseType.DeferredChannelMessageWithSource,
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
		HasResponded = true;
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
		HasResponded = true;
	}

	public override async Task RespondWithModalAsync(Modal modal, RequestOptions options = null)
	{
		if (!base.IsValidToken)
		{
			throw new InvalidOperationException("Interaction token is no longer valid");
		}
		if (!InteractionHelper.CanSendResponse(this))
		{
			throw new TimeoutException($"Cannot respond to an interaction after {3.0} seconds!");
		}
		InteractionResponse response = new InteractionResponse
		{
			Type = InteractionResponseType.Modal,
			Data = new InteractionCallbackData
			{
				CustomId = modal.CustomId,
				Title = modal.Title,
				Components = modal.Component.Components.Select((ActionRowComponent x) => new global::Discord.API.ActionRowComponent(x)).ToArray()
			}
		};
		lock (_lock)
		{
			if (HasResponded)
			{
				throw new InvalidOperationException("Cannot respond twice to the same interaction");
			}
		}
		await InteractionHelper.SendInteractionResponseAsync(base.Discord, response, this, base.Channel, options).ConfigureAwait(continueOnCapturedContext: false);
		lock (_lock)
		{
			HasResponded = true;
		}
	}
}
