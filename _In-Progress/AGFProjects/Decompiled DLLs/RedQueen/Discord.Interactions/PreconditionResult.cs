using System;

namespace Discord.Interactions;

internal class PreconditionResult : IResult
{
	public InteractionCommandError? Error { get; }

	public string ErrorReason { get; }

	public bool IsSuccess => !Error.HasValue;

	protected PreconditionResult(InteractionCommandError? error, string reason)
	{
		Error = error;
		ErrorReason = reason;
	}

	public static PreconditionResult FromSuccess()
	{
		return new PreconditionResult(null, null);
	}

	public static PreconditionResult FromError(Exception exception)
	{
		return new PreconditionResult(InteractionCommandError.Exception, exception.Message);
	}

	public static PreconditionResult FromError(IResult result)
	{
		return new PreconditionResult(result.Error, result.ErrorReason);
	}

	public static PreconditionResult FromError(string reason)
	{
		return new PreconditionResult(InteractionCommandError.UnmetPrecondition, reason);
	}
}
