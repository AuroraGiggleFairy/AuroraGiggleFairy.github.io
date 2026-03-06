using System;

namespace Discord.Commands;

internal class MatchResult : IResult
{
	public CommandMatch? Match { get; }

	public IResult Pipeline { get; }

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

	private MatchResult(CommandMatch? match, IResult pipeline, CommandError? error, string errorReason)
	{
		Match = match;
		Error = error;
		Pipeline = pipeline;
		ErrorReason = errorReason;
	}

	public static MatchResult FromSuccess(CommandMatch match, IResult pipeline)
	{
		return new MatchResult(match, pipeline, null, null);
	}

	public static MatchResult FromError(CommandError error, string reason)
	{
		return new MatchResult(null, null, error, reason);
	}

	public static MatchResult FromError(Exception ex)
	{
		return FromError(CommandError.Exception, ex.Message);
	}

	public static MatchResult FromError(IResult result)
	{
		return new MatchResult(null, null, result.Error, result.ErrorReason);
	}

	public static MatchResult FromError(IResult pipeline, CommandError error, string reason)
	{
		return new MatchResult(null, pipeline, error, reason);
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
