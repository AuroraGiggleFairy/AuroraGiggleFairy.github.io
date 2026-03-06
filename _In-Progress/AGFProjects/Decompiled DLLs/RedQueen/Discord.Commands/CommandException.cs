using System;

namespace Discord.Commands;

internal class CommandException : Exception
{
	public CommandInfo Command { get; }

	public ICommandContext Context { get; }

	public CommandException(CommandInfo command, ICommandContext context, Exception ex)
		: base("Error occurred executing " + command.GetLogText(context) + ".", ex)
	{
		Command = command;
		Context = context;
	}
}
