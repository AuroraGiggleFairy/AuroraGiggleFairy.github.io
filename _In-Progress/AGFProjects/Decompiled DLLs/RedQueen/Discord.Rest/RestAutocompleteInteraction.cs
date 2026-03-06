using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord.API;

namespace Discord.Rest;

internal class RestAutocompleteInteraction : RestInteraction, IAutocompleteInteraction, IDiscordInteraction, ISnowflakeEntity, IEntity<ulong>
{
	private object _lock = new object();

	public new RestAutocompleteInteractionData Data { get; }

	IAutocompleteInteractionData IAutocompleteInteraction.Data => Data;

	internal RestAutocompleteInteraction(DiscordRestClient client, Interaction model)
		: base(client, model.Id)
	{
		AutocompleteInteractionData autocompleteInteractionData = (model.Data.IsSpecified ? ((AutocompleteInteractionData)model.Data.Value) : null);
		if (autocompleteInteractionData != null)
		{
			Data = new RestAutocompleteInteractionData(autocompleteInteractionData);
		}
	}

	internal new static async Task<RestAutocompleteInteraction> CreateAsync(DiscordRestClient client, Interaction model, bool doApiCall)
	{
		RestAutocompleteInteraction entity = new RestAutocompleteInteraction(client, model);
		await entity.UpdateAsync(client, model, doApiCall).ConfigureAwait(continueOnCapturedContext: false);
		return entity;
	}

	public string Respond(IEnumerable<AutocompleteResult> result, RequestOptions options = null)
	{
		if (!InteractionHelper.CanSendResponse(this))
		{
			throw new TimeoutException($"Cannot respond to an interaction after {3.0} seconds!");
		}
		lock (_lock)
		{
			if (base.HasResponded)
			{
				throw new InvalidOperationException("Cannot respond twice to the same interaction");
			}
			base.HasResponded = true;
		}
		InteractionResponse payload = new InteractionResponse
		{
			Type = InteractionResponseType.ApplicationCommandAutocompleteResult,
			Data = new InteractionCallbackData
			{
				Choices = (result.Any() ? result.Select((AutocompleteResult x) => new ApplicationCommandOptionChoice
				{
					Name = x.Name,
					Value = x.Value
				}).ToArray() : Array.Empty<ApplicationCommandOptionChoice>())
			}
		};
		return SerializePayload(payload);
	}

	public string Respond(RequestOptions options = null, params AutocompleteResult[] result)
	{
		return Respond(result, options);
	}

	public override string Defer(bool ephemeral = false, RequestOptions options = null)
	{
		throw new NotSupportedException("Autocomplete interactions don't support this method!");
	}

	public override string Respond(string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
	{
		throw new NotSupportedException("Autocomplete interactions don't support this method!");
	}

	public override Task<RestFollowupMessage> FollowupAsync(string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
	{
		throw new NotSupportedException("Autocomplete interactions don't support this method!");
	}

	public override Task<RestFollowupMessage> FollowupWithFileAsync(Stream fileStream, string fileName, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
	{
		throw new NotSupportedException("Autocomplete interactions don't support this method!");
	}

	public override Task<RestFollowupMessage> FollowupWithFileAsync(string filePath, string fileName = null, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
	{
		throw new NotSupportedException("Autocomplete interactions don't support this method!");
	}

	public override Task<RestFollowupMessage> FollowupWithFileAsync(FileAttachment attachment, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
	{
		throw new NotSupportedException("Autocomplete interactions don't support this method!");
	}

	public override Task<RestFollowupMessage> FollowupWithFilesAsync(IEnumerable<FileAttachment> attachments, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
	{
		throw new NotSupportedException("Autocomplete interactions don't support this method!");
	}

	public override string RespondWithModal(Modal modal, RequestOptions options = null)
	{
		throw new NotSupportedException("Autocomplete interactions don't support this method!");
	}
}
