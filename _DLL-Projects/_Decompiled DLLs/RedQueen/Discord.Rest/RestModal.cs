using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord.API;
using Discord.API.Rest;
using Discord.Net.Rest;

namespace Discord.Rest;

internal class RestModal : RestInteraction, IDiscordInteraction, ISnowflakeEntity, IEntity<ulong>, IModalInteraction
{
	private object _lock = new object();

	public new RestModalData Data { get; set; }

	IModalInteractionData IModalInteraction.Data => Data;

	internal RestModal(DiscordRestClient client, Interaction model)
		: base(client, model.Id)
	{
		Data = new RestModalData(model.Data.IsSpecified ? ((ModalInteractionData)model.Data.Value) : null);
	}

	internal new static async Task<RestModal> CreateAsync(DiscordRestClient client, Interaction model, bool doApiCall)
	{
		RestModal entity = new RestModal(client, model);
		await entity.UpdateAsync(client, model, doApiCall);
		return entity;
	}

	public override string Defer(bool ephemeral = false, RequestOptions options = null)
	{
		if (!InteractionHelper.CanSendResponse(this))
		{
			throw new TimeoutException($"Cannot defer an interaction after {3.0} seconds!");
		}
		InteractionResponse payload = new InteractionResponse
		{
			Type = InteractionResponseType.DeferredChannelMessageWithSource,
			Data = new InteractionCallbackData
			{
				Flags = (ephemeral ? ((Optional<MessageFlags>)MessageFlags.Ephemeral) : Optional<MessageFlags>.Unspecified)
			}
		};
		lock (_lock)
		{
			if (base.HasResponded)
			{
				throw new InvalidOperationException("Cannot respond or defer twice to the same interaction");
			}
		}
		lock (_lock)
		{
			base.HasResponded = true;
		}
		return SerializePayload(payload);
	}

	public override async Task<RestFollowupMessage> FollowupAsync(string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent component = null, Embed embed = null, RequestOptions options = null)
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
		global::Discord.API.ActionRowComponent[] array = component?.Components.Select((ActionRowComponent x) => new global::Discord.API.ActionRowComponent(x)).ToArray();
		obj.Components = ((array != null) ? ((Optional<global::Discord.API.ActionRowComponent[]>)array) : Optional<global::Discord.API.ActionRowComponent[]>.Unspecified);
		CreateWebhookMessageParams createWebhookMessageParams = obj;
		if (ephemeral)
		{
			createWebhookMessageParams.Flags = MessageFlags.Ephemeral;
		}
		return await InteractionHelper.SendFollowupAsync(base.Discord, createWebhookMessageParams, base.Token, base.Channel, options);
	}

	public override async Task<RestFollowupMessage> FollowupWithFileAsync(Stream fileStream, string fileName, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent component = null, Embed embed = null, RequestOptions options = null)
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
		Preconditions.NotNull(fileStream, "fileStream", "File Stream must have data");
		Preconditions.NotNullOrEmpty(fileName, "fileName", "File Name must not be empty or null");
		CreateWebhookMessageParams obj = new CreateWebhookMessageParams
		{
			Content = text
		};
		global::Discord.API.AllowedMentions allowedMentions2 = allowedMentions?.ToModel();
		obj.AllowedMentions = ((allowedMentions2 != null) ? ((Optional<global::Discord.API.AllowedMentions>)allowedMentions2) : Optional<global::Discord.API.AllowedMentions>.Unspecified);
		obj.IsTTS = isTTS;
		obj.Embeds = embeds.Select((Embed x) => x.ToModel()).ToArray();
		global::Discord.API.ActionRowComponent[] array = component?.Components.Select((ActionRowComponent x) => new global::Discord.API.ActionRowComponent(x)).ToArray();
		obj.Components = ((array != null) ? ((Optional<global::Discord.API.ActionRowComponent[]>)array) : Optional<global::Discord.API.ActionRowComponent[]>.Unspecified);
		obj.File = ((fileStream != null) ? ((Optional<MultipartFile>)new MultipartFile(fileStream, fileName)) : Optional<MultipartFile>.Unspecified);
		CreateWebhookMessageParams createWebhookMessageParams = obj;
		if (ephemeral)
		{
			createWebhookMessageParams.Flags = MessageFlags.Ephemeral;
		}
		return await InteractionHelper.SendFollowupAsync(base.Discord, createWebhookMessageParams, base.Token, base.Channel, options);
	}

	public override async Task<RestFollowupMessage> FollowupWithFileAsync(string filePath, string text = null, string fileName = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent component = null, Embed embed = null, RequestOptions options = null)
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
		Preconditions.NotNullOrEmpty(filePath, "filePath", "Path must exist");
		if (fileName == null)
		{
			fileName = Path.GetFileName(filePath);
		}
		Preconditions.NotNullOrEmpty(fileName, "fileName", "File Name must not be empty or null");
		CreateWebhookMessageParams obj = new CreateWebhookMessageParams
		{
			Content = text
		};
		global::Discord.API.AllowedMentions allowedMentions2 = allowedMentions?.ToModel();
		obj.AllowedMentions = ((allowedMentions2 != null) ? ((Optional<global::Discord.API.AllowedMentions>)allowedMentions2) : Optional<global::Discord.API.AllowedMentions>.Unspecified);
		obj.IsTTS = isTTS;
		obj.Embeds = embeds.Select((Embed x) => x.ToModel()).ToArray();
		global::Discord.API.ActionRowComponent[] array = component?.Components.Select((ActionRowComponent x) => new global::Discord.API.ActionRowComponent(x)).ToArray();
		obj.Components = ((array != null) ? ((Optional<global::Discord.API.ActionRowComponent[]>)array) : Optional<global::Discord.API.ActionRowComponent[]>.Unspecified);
		obj.File = ((!string.IsNullOrEmpty(filePath)) ? ((Optional<MultipartFile>)new MultipartFile(new MemoryStream(File.ReadAllBytes(filePath), writable: false), fileName)) : Optional<MultipartFile>.Unspecified);
		CreateWebhookMessageParams createWebhookMessageParams = obj;
		if (ephemeral)
		{
			createWebhookMessageParams.Flags = MessageFlags.Ephemeral;
		}
		return await InteractionHelper.SendFollowupAsync(base.Discord, createWebhookMessageParams, base.Token, base.Channel, options);
	}

	public override string Respond(string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent component = null, Embed embed = null, RequestOptions options = null)
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
		InteractionCallbackData obj2 = new InteractionCallbackData
		{
			Content = text
		};
		global::Discord.API.AllowedMentions allowedMentions2 = allowedMentions?.ToModel();
		obj2.AllowedMentions = ((allowedMentions2 != null) ? ((Optional<global::Discord.API.AllowedMentions>)allowedMentions2) : Optional<global::Discord.API.AllowedMentions>.Unspecified);
		obj2.Embeds = embeds.Select((Embed x) => x.ToModel()).ToArray();
		obj2.TTS = isTTS;
		global::Discord.API.ActionRowComponent[] array = component?.Components.Select((ActionRowComponent x) => new global::Discord.API.ActionRowComponent(x)).ToArray();
		obj2.Components = ((array != null) ? ((Optional<global::Discord.API.ActionRowComponent[]>)array) : Optional<global::Discord.API.ActionRowComponent[]>.Unspecified);
		obj2.Flags = (ephemeral ? ((Optional<MessageFlags>)MessageFlags.Ephemeral) : Optional<MessageFlags>.Unspecified);
		obj.Data = obj2;
		InteractionResponse payload = obj;
		lock (_lock)
		{
			if (base.HasResponded)
			{
				throw new InvalidOperationException("Cannot respond twice to the same interaction");
			}
		}
		lock (_lock)
		{
			base.HasResponded = true;
		}
		return SerializePayload(payload);
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

	public override Task<RestFollowupMessage> FollowupWithFileAsync(FileAttachment attachment, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
	{
		return FollowupWithFilesAsync(new FileAttachment[1] { attachment }, text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options);
	}

	public override string RespondWithModal(Modal modal, RequestOptions requestOptions = null)
	{
		throw new NotSupportedException("Modal interactions cannot have modal responces!");
	}
}
