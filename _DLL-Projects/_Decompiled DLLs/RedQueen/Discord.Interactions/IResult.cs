namespace Discord.Interactions;

internal interface IResult
{
	InteractionCommandError? Error { get; }

	string ErrorReason { get; }

	bool IsSuccess { get; }
}
