namespace Discord.Commands;

internal enum CommandError
{
	UnknownCommand = 1,
	ParseFailed,
	BadArgCount,
	ObjectNotFound,
	MultipleMatches,
	UnmetPrecondition,
	Exception,
	Unsuccessful
}
