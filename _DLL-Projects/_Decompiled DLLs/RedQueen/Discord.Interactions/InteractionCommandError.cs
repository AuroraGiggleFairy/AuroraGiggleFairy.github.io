namespace Discord.Interactions;

internal enum InteractionCommandError
{
	UnknownCommand,
	ConvertFailed,
	BadArgs,
	Exception,
	Unsuccessful,
	UnmetPrecondition,
	ParseFailed
}
