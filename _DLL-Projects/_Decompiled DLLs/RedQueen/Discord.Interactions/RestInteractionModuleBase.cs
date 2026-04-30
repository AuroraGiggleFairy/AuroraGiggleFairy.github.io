using System;
using System.Threading.Tasks;
using Discord.Rest;

namespace Discord.Interactions;

internal abstract class RestInteractionModuleBase<T> : InteractionModuleBase<T> where T : class, IInteractionContext
{
	public InteractionService InteractionService { get; set; }

	protected override async Task DeferAsync(bool ephemeral = false, RequestOptions options = null)
	{
		string text = ((base.Context.Interaction as RestInteraction) ?? throw new InvalidOperationException("Invalid interaction type. Interaction must be a type of RestInteraction in order to execute this method")).Defer(ephemeral, options);
		if (!(base.Context is IRestInteractionContext { InteractionResponseCallback: not null } restInteractionContext))
		{
			await InteractionService._restResponseCallback(base.Context, text).ConfigureAwait(continueOnCapturedContext: false);
		}
		else
		{
			await restInteractionContext.InteractionResponseCallback(text).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	protected override async Task RespondAsync(string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, RequestOptions options = null, MessageComponent components = null, Embed embed = null)
	{
		string text2 = ((base.Context.Interaction as RestInteraction) ?? throw new InvalidOperationException("Invalid interaction type. Interaction must be a type of RestInteraction in order to execute this method")).Respond(text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options);
		if (!(base.Context is IRestInteractionContext { InteractionResponseCallback: not null } restInteractionContext))
		{
			await InteractionService._restResponseCallback(base.Context, text2).ConfigureAwait(continueOnCapturedContext: false);
		}
		else
		{
			await restInteractionContext.InteractionResponseCallback(text2).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	protected override async Task RespondWithModalAsync(Modal modal, RequestOptions options = null)
	{
		string text = ((base.Context.Interaction as RestInteraction) ?? throw new InvalidOperationException("Invalid interaction type. Interaction must be a type of RestInteraction in order to execute this method")).RespondWithModal(modal, options);
		if (!(base.Context is IRestInteractionContext { InteractionResponseCallback: not null } restInteractionContext))
		{
			await InteractionService._restResponseCallback(base.Context, text).ConfigureAwait(continueOnCapturedContext: false);
		}
		else
		{
			await restInteractionContext.InteractionResponseCallback(text).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	protected override async Task RespondWithModalAsync<TModal>(string customId, RequestOptions options = null)
	{
		string text = ((base.Context.Interaction as RestInteraction) ?? throw new InvalidOperationException("Invalid interaction type. Interaction must be a type of RestInteraction in order to execute this method")).RespondWithModal<TModal>(customId, options);
		if (!(base.Context is IRestInteractionContext { InteractionResponseCallback: not null } restInteractionContext))
		{
			await InteractionService._restResponseCallback(base.Context, text).ConfigureAwait(continueOnCapturedContext: false);
		}
		else
		{
			await restInteractionContext.InteractionResponseCallback(text).ConfigureAwait(continueOnCapturedContext: false);
		}
	}
}
