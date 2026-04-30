using System;
using System.Runtime.CompilerServices;

namespace Discord.Interactions;

internal struct ParseResult : IResult
{
	public object Value
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

	private ParseResult(object value, InteractionCommandError? error, string reason)
	{
		Value = value;
		Error = error;
		ErrorReason = reason;
	}

	public static ParseResult FromSuccess(object value)
	{
		return new ParseResult(value, null, null);
	}

	public static ParseResult FromError(Exception exception)
	{
		return new ParseResult(null, InteractionCommandError.Exception, exception.Message);
	}

	public static ParseResult FromError(InteractionCommandError error, string reason)
	{
		return new ParseResult(null, error, reason);
	}

	public static ParseResult FromError(IResult result)
	{
		return new ParseResult(null, result.Error, result.ErrorReason);
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
