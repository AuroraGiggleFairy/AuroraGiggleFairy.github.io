using System;
using System.Runtime.CompilerServices;

namespace Discord.Interactions;

internal struct TypeConverterResult : IResult
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

	private TypeConverterResult(object value, InteractionCommandError? error, string reason)
	{
		Value = value;
		Error = error;
		ErrorReason = reason;
	}

	public static TypeConverterResult FromSuccess(object value)
	{
		return new TypeConverterResult(value, null, null);
	}

	public static TypeConverterResult FromError(Exception exception)
	{
		return new TypeConverterResult(null, InteractionCommandError.Exception, exception.Message);
	}

	public static TypeConverterResult FromError(InteractionCommandError error, string reason)
	{
		return new TypeConverterResult(null, error, reason);
	}

	public static TypeConverterResult FromError(IResult result)
	{
		return new TypeConverterResult(null, result.Error, result.ErrorReason);
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
