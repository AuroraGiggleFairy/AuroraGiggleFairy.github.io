using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Discord.Commands;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class PreconditionGroupResult : PreconditionResult
{
	public IReadOnlyCollection<PreconditionResult> PreconditionResults { get; }

	private string DebuggerDisplay
	{
		get
		{
			if (!base.IsSuccess)
			{
				return $"{base.Error}: {base.ErrorReason}";
			}
			return "Success";
		}
	}

	protected PreconditionGroupResult(CommandError? error, string errorReason, ICollection<PreconditionResult> preconditions)
		: base(error, errorReason)
	{
		PreconditionResults = (preconditions ?? new List<PreconditionResult>(0)).ToReadOnlyCollection();
	}

	public new static PreconditionGroupResult FromSuccess()
	{
		return new PreconditionGroupResult(null, null, null);
	}

	public static PreconditionGroupResult FromError(string reason, ICollection<PreconditionResult> preconditions)
	{
		return new PreconditionGroupResult(CommandError.UnmetPrecondition, reason, preconditions);
	}

	public new static PreconditionGroupResult FromError(Exception ex)
	{
		return new PreconditionGroupResult(CommandError.Exception, ex.Message, null);
	}

	public new static PreconditionGroupResult FromError(IResult result)
	{
		return new PreconditionGroupResult(result.Error, result.ErrorReason, null);
	}

	public override string ToString()
	{
		if (!base.IsSuccess)
		{
			return $"{base.Error}: {base.ErrorReason}";
		}
		return "Success";
	}
}
