using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Discord.Rest;
using Discord.WebSocket;

namespace Discord.Interactions;

internal abstract class AutocompleteHandler : IAutocompleteHandler
{
	public InteractionService InteractionService { get; set; }

	public abstract Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services);

	protected virtual string GetLogString(IInteractionContext context)
	{
		IAutocompleteInteraction autocompleteInteraction = context.Interaction as IAutocompleteInteraction;
		return autocompleteInteraction.Data.CommandName + ": " + autocompleteInteraction.Data.Current.Name + " Autocomplete";
	}

	public async Task<IResult> ExecuteAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
	{
		switch (InteractionService._runMode)
		{
		case RunMode.Sync:
			return await ExecuteInternalAsync(context, autocompleteInteraction, parameter, services).ConfigureAwait(continueOnCapturedContext: false);
		case RunMode.Async:
			Task.Run(async delegate
			{
				await ExecuteInternalAsync(context, autocompleteInteraction, parameter, services).ConfigureAwait(continueOnCapturedContext: false);
			});
			return ExecuteResult.FromSuccess();
		default:
			throw new InvalidOperationException($"RunMode {InteractionService._runMode} is not supported.");
		}
	}

	private async Task<IResult> ExecuteInternalAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
	{
		IResult result2;
		try
		{
			AutocompletionResult result = await GenerateSuggestionsAsync(context, autocompleteInteraction, parameter, services).ConfigureAwait(continueOnCapturedContext: false);
			if (result.IsSuccess)
			{
				if (!(autocompleteInteraction is RestAutocompleteInteraction restAutocompleteInteraction))
				{
					if (autocompleteInteraction is SocketAutocompleteInteraction socketAutocompleteInteraction)
					{
						await socketAutocompleteInteraction.RespondAsync(result.Suggestions).ConfigureAwait(continueOnCapturedContext: false);
					}
				}
				else
				{
					string text = restAutocompleteInteraction.Respond(result.Suggestions);
					if (context is IRestInteractionContext { InteractionResponseCallback: not null } restInteractionContext)
					{
						await restInteractionContext.InteractionResponseCallback(text).ConfigureAwait(continueOnCapturedContext: false);
					}
					else
					{
						await InteractionService._restResponseCallback(context, text).ConfigureAwait(continueOnCapturedContext: false);
					}
				}
			}
			await InteractionService._autocompleteHandlerExecutedEvent.InvokeAsync(this, context, result).ConfigureAwait(continueOnCapturedContext: false);
			result2 = result;
		}
		catch (Exception innerException)
		{
			Exception originalEx = innerException;
			while (innerException is TargetInvocationException)
			{
				innerException = innerException.InnerException;
			}
			await InteractionService._cmdLogger.ErrorAsync(innerException).ConfigureAwait(continueOnCapturedContext: false);
			ExecuteResult result3 = ExecuteResult.FromError(innerException);
			await InteractionService._autocompleteHandlerExecutedEvent.InvokeAsync(this, context, result3).ConfigureAwait(continueOnCapturedContext: false);
			if (InteractionService._throwOnError)
			{
				if (innerException == originalEx)
				{
					throw;
				}
				ExceptionDispatchInfo.Capture(innerException).Throw();
			}
			result2 = result3;
		}
		finally
		{
			await InteractionService._cmdLogger.VerboseAsync("Executed " + GetLogString(context)).ConfigureAwait(continueOnCapturedContext: false);
		}
		return result2;
	}
}
