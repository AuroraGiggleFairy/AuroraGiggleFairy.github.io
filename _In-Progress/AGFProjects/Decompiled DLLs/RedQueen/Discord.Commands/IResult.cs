namespace Discord.Commands;

internal interface IResult
{
	CommandError? Error { get; }

	string ErrorReason { get; }

	bool IsSuccess { get; }
}
