using System;
using System.Diagnostics;

namespace Discord.Commands;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class PreconditionResult : IResult
{
	public CommandError? Error { get; }

	public string ErrorReason { get; }

	public bool IsSuccess => !Error.HasValue;

	private string DebuggerDisplay
	{
		get
		{
			if (!IsSuccess)
			{
				return $"{Error}: {ErrorReason}";
			}
			return "Success";
		}
	}

	protected PreconditionResult(CommandError? error, string errorReason)
	{
		Error = error;
		ErrorReason = errorReason;
	}

	public static PreconditionResult FromSuccess()
	{
		return new PreconditionResult(null, null);
	}

	public static PreconditionResult FromError(string reason)
	{
		return new PreconditionResult(CommandError.UnmetPrecondition, reason);
	}

	public static PreconditionResult FromError(Exception ex)
	{
		return new PreconditionResult(CommandError.Exception, ex.Message);
	}

	public static PreconditionResult FromError(IResult result)
	{
		return new PreconditionResult(result.Error, result.ErrorReason);
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
