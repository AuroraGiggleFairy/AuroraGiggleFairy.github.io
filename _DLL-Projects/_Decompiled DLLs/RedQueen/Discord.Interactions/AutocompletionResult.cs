using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Discord.Interactions;

internal struct AutocompletionResult : IResult
{
	public InteractionCommandError? Error
	{
		[_003Cc2a4ce43_002Da557_002D44ec_002Da0b8_002D00e1eb19124a_003EIsReadOnly]
		get;
	}

	public string ErrorReason
	{
		[_003Cc2a4ce43_002Da557_002D44ec_002Da0b8_002D00e1eb19124a_003EIsReadOnly]
		get;
	}

	public bool IsSuccess => !Error.HasValue;

	public IReadOnlyCollection<AutocompleteResult> Suggestions
	{
		[_003Cc2a4ce43_002Da557_002D44ec_002Da0b8_002D00e1eb19124a_003EIsReadOnly]
		get;
	}

	private AutocompletionResult(IEnumerable<AutocompleteResult> suggestions, InteractionCommandError? error, string reason)
	{
		Suggestions = suggestions?.ToImmutableArray();
		Error = error;
		ErrorReason = reason;
	}

	public static AutocompletionResult FromSuccess()
	{
		return new AutocompletionResult(null, null, null);
	}

	public static AutocompletionResult FromSuccess(IEnumerable<AutocompleteResult> suggestions)
	{
		return new AutocompletionResult(suggestions, null, null);
	}

	public static AutocompletionResult FromError(IResult result)
	{
		return new AutocompletionResult(null, result.Error, result.ErrorReason);
	}

	public static AutocompletionResult FromError(Exception exception)
	{
		return new AutocompletionResult(null, InteractionCommandError.Exception, exception.Message);
	}

	public static AutocompletionResult FromError(InteractionCommandError error, string reason)
	{
		return new AutocompletionResult(null, error, reason);
	}

	public override string ToString()
	{
		if (!IsSuccess)
		{
			return $"{Error}: {ErrorReason}";
		}
		return "Success";
	}
}
