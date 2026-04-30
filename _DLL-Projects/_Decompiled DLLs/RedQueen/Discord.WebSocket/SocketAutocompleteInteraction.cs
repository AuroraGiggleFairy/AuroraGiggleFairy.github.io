using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.API;
using Discord.Rest;

namespace Discord.WebSocket;

internal class SocketAutocompleteInteraction : SocketInteraction, IAutocompleteInteraction, IDiscordInteraction, ISnowflakeEntity, IEntity<ulong>
{
	private object _lock = new object();

	public new SocketAutocompleteInteractionData Data { get; }

	public override bool HasResponded { get; internal set; }

	IAutocompleteInteractionData IAutocompleteInteraction.Data => Data;

	IDiscordInteractionData IDiscordInteraction.Data => Data;

	internal SocketAutocompleteInteraction(DiscordSocketClient client, Interaction model, ISocketMessageChannel channel, SocketUser user)
		: base(client, model.Id, channel, user)
	{
		AutocompleteInteractionData autocompleteInteractionData = (model.Data.IsSpecified ? ((AutocompleteInteractionData)model.Data.Value) : null);
		if (autocompleteInteractionData != null)
		{
			Data = new SocketAutocompleteInteractionData(autocompleteInteractionData);
		}
	}

	internal new static SocketAutocompleteInteraction Create(DiscordSocketClient client, Interaction model, ISocketMessageChannel channel, SocketUser user)
	{
		SocketAutocompleteInteraction socketAutocompleteInteraction = new SocketAutocompleteInteraction(client, model, channel, user);
		socketAutocompleteInteraction.Update(model);
		return socketAutocompleteInteraction;
	}

	public async Task RespondAsync(IEnumerable<AutocompleteResult> result, RequestOptions options = null)
	{
		if (!InteractionHelper.CanSendResponse(this))
		{
			throw new TimeoutException($"Cannot respond to an interaction after {3.0} seconds!");
		}
		lock (_lock)
		{
			if (HasResponded)
			{
				throw new InvalidOperationException("Cannot respond twice to the same interaction");
			}
		}
		await InteractionHelper.SendAutocompleteResultAsync(base.Discord, result, base.Id, base.Token, options).ConfigureAwait(continueOnCapturedContext: false);
		lock (_lock)
		{
			HasResponded = true;
		}
	}

	public Task RespondAsync(RequestOptions options = null, params AutocompleteResult[] result)
	{
		return RespondAsync(result, options);
	}

	public override Task RespondAsync(string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
	{
		throw new NotSupportedException("Autocomplete interactions don't support this method!");
	}

	public override Task<RestFollowupMessage> FollowupAsync(string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
	{
		throw new NotSupportedException("Autocomplete interactions don't support this method!");
	}

	public override Task<RestFollowupMessage> FollowupWithFilesAsync(IEnumerable<FileAttachment> attachments, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
	{
		throw new NotSupportedException("Autocomplete interactions don't support this method!");
	}

	public override Task DeferAsync(bool ephemeral = false, RequestOptions options = null)
	{
		throw new NotSupportedException("Autocomplete interactions don't support this method!");
	}

	public override Task RespondWithFilesAsync(IEnumerable<FileAttachment> attachments, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
	{
		throw new NotSupportedException("Autocomplete interactions don't support this method!");
	}

	public override Task RespondWithModalAsync(Modal modal, RequestOptions requestOptions = null)
	{
		throw new NotSupportedException("Autocomplete interactions cannot have normal responces!");
	}
}
