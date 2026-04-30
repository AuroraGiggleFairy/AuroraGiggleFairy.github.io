using System;
using System.Runtime.CompilerServices;

namespace Discord.Interactions;

internal struct SearchResult<T> : IResult where T : class, ICommandInfo
{
	public string Text
	{
		[_003Cc2a4ce43_002Da557_002D44ec_002Da0b8_002D00e1eb19124a_003EIsReadOnly]
		get;
	}

	public T Command
	{
		[_003Cc2a4ce43_002Da557_002D44ec_002Da0b8_002D00e1eb19124a_003EIsReadOnly]
		get;
	}

	public string[] RegexCaptureGroups
	{
		[_003Cc2a4ce43_002Da557_002D44ec_002Da0b8_002D00e1eb19124a_003EIsReadOnly]
		get;
	}

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

	private SearchResult(string text, T commandInfo, string[] captureGroups, InteractionCommandError? error, string reason)
	{
		Text = text;
		Error = error;
		RegexCaptureGroups = captureGroups;
		Command = commandInfo;
		ErrorReason = reason;
	}

	public static SearchResult<T> FromSuccess(string text, T commandInfo, string[] wildCardMatch = null)
	{
		return new SearchResult<T>(text, commandInfo, wildCardMatch, null, null);
	}

	public static SearchResult<T> FromError(string text, InteractionCommandError error, string reason)
	{
		return new SearchResult<T>(text, null, null, error, reason);
	}

	public static SearchResult<T> FromError(Exception ex)
	{
		return new SearchResult<T>(null, null, null, InteractionCommandError.Exception, ex.Message);
	}

	public static SearchResult<T> FromError(IResult result)
	{
		return new SearchResult<T>(null, null, null, result.Error, result.ErrorReason);
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
