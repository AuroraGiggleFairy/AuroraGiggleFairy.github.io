using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Discord.Commands;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal struct ExecuteResult : IResult
{
	public Exception Exception
	{
		[_003Cc51ba3f1_002D43fc_002D4bc9_002D811d_002D97306e7eb83f_003EIsReadOnly]
		get;
	}

	public CommandError? Error
	{
		[_003Cc51ba3f1_002D43fc_002D4bc9_002D811d_002D97306e7eb83f_003EIsReadOnly]
		get;
	}

	public string ErrorReason
	{
		[_003Cc51ba3f1_002D43fc_002D4bc9_002D811d_002D97306e7eb83f_003EIsReadOnly]
		get;
	}

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

	private ExecuteResult(Exception exception, CommandError? error, string errorReason)
	{
		Exception = exception;
		Error = error;
		ErrorReason = errorReason;
	}

	public static ExecuteResult FromSuccess()
	{
		return new ExecuteResult(null, null, null);
	}

	public static ExecuteResult FromError(CommandError error, string reason)
	{
		return new ExecuteResult(null, error, reason);
	}

	public static ExecuteResult FromError(Exception ex)
	{
		return new ExecuteResult(ex, CommandError.Exception, ex.Message);
	}

	public static ExecuteResult FromError(IResult result)
	{
		return new ExecuteResult(null, result.Error, result.ErrorReason);
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
