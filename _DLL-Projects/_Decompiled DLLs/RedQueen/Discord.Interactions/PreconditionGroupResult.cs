using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Discord.Interactions;

internal class PreconditionGroupResult : PreconditionResult
{
	public IReadOnlyCollection<PreconditionResult> Results { get; }

	private PreconditionGroupResult(InteractionCommandError? error, string reason, IEnumerable<PreconditionResult> results)
		: base(error, reason)
	{
		Results = results?.ToImmutableArray();
	}

	public new static PreconditionGroupResult FromSuccess()
	{
		return new PreconditionGroupResult(null, null, null);
	}

	public new static PreconditionGroupResult FromError(Exception exception)
	{
		return new PreconditionGroupResult(InteractionCommandError.Exception, exception.Message, null);
	}

	public new static PreconditionGroupResult FromError(IResult result)
	{
		return new PreconditionGroupResult(result.Error, result.ErrorReason, null);
	}

	public static PreconditionGroupResult FromError(string reason, IEnumerable<PreconditionResult> results)
	{
		return new PreconditionGroupResult(InteractionCommandError.UnmetPrecondition, reason, results);
	}
}
