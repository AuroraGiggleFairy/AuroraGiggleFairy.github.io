using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Discord.API;

namespace Discord.Rest;

internal class RestPingInteraction : RestInteraction, IDiscordInteraction, ISnowflakeEntity, IEntity<ulong>
{
	internal RestPingInteraction(BaseDiscordClient client, ulong id)
		: base(client, id)
	{
	}

	internal new static async Task<RestPingInteraction> CreateAsync(DiscordRestClient client, Interaction model, bool doApiCall)
	{
		RestPingInteraction entity = new RestPingInteraction(client, model.Id);
		await entity.UpdateAsync(client, model, doApiCall);
		return entity;
	}

	public string AcknowledgePing()
	{
		InteractionResponse payload = new InteractionResponse
		{
			Type = InteractionResponseType.Pong
		};
		return SerializePayload(payload);
	}

	public override string Defer(bool ephemeral = false, RequestOptions options = null)
	{
		throw new NotSupportedException();
	}

	public override string RespondWithModal(Modal modal, RequestOptions options = null)
	{
		throw new NotSupportedException();
	}

	public override string Respond(string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
	{
		throw new NotSupportedException();
	}

	public override Task<RestFollowupMessage> FollowupAsync(string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
	{
		throw new NotSupportedException();
	}

	public override Task<RestFollowupMessage> FollowupWithFileAsync(Stream fileStream, string fileName, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
	{
		throw new NotSupportedException();
	}

	public override Task<RestFollowupMessage> FollowupWithFileAsync(string filePath, string fileName = null, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
	{
		throw new NotSupportedException();
	}

	public override Task<RestFollowupMessage> FollowupWithFileAsync(FileAttachment attachment, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
	{
		throw new NotSupportedException();
	}

	public override Task<RestFollowupMessage> FollowupWithFilesAsync(IEnumerable<FileAttachment> attachments, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
	{
		throw new NotSupportedException();
	}
}
