using System;
using System.Threading.Tasks;

namespace Discord.Interactions;

internal interface IAutocompleteHandler
{
	InteractionService InteractionService { get; }

	Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services);

	Task<IResult> ExecuteAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services);
}
