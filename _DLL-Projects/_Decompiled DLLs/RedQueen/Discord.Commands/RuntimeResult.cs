using System.Diagnostics;

namespace Discord.Commands;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal abstract class RuntimeResult : IResult
{
	public CommandError? Error { get; }

	public string Reason { get; }

	public bool IsSuccess => !Error.HasValue;

	string IResult.ErrorReason => Reason;

	private string DebuggerDisplay
	{
		get
		{
			if (!IsSuccess)
			{
				return $"{Error}: {Reason}";
			}
			return "Success: " + (Reason ?? "No Reason");
		}
	}

	protected RuntimeResult(CommandError? error, string reason)
	{
		Error = error;
		Reason = reason;
	}

	public override string ToString()
	{
		object obj = Reason;
		if (obj == null)
		{
			if (!IsSuccess)
			{
				return "Unsuccessful";
			}
			obj = "Successful";
		}
		return (string)obj;
	}
}
