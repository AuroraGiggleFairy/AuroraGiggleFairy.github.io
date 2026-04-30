namespace Discord.Interactions;

internal abstract class RuntimeResult : IResult
{
	public InteractionCommandError? Error { get; }

	public string ErrorReason { get; }

	public bool IsSuccess => !Error.HasValue;

	protected RuntimeResult(InteractionCommandError? error, string reason)
	{
		Error = error;
		ErrorReason = reason;
	}

	public override string ToString()
	{
		object obj = ErrorReason;
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
