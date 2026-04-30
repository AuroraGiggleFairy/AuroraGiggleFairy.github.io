using System.Collections.Generic;

namespace Discord.Commands;

internal class CommandMap
{
	private readonly CommandService _service;

	private readonly CommandMapNode _root;

	private static readonly string[] BlankAliases = new string[1] { "" };

	public CommandMap(CommandService service)
	{
		_service = service;
		_root = new CommandMapNode("");
	}

	public void AddCommand(CommandInfo command)
	{
		foreach (string alias in command.Aliases)
		{
			_root.AddCommand(_service, alias, 0, command);
		}
	}

	public void RemoveCommand(CommandInfo command)
	{
		foreach (string alias in command.Aliases)
		{
			_root.RemoveCommand(_service, alias, 0, command);
		}
	}

	public IEnumerable<CommandMatch> GetCommands(string text)
	{
		return _root.GetCommands(_service, text, 0, text != "");
	}
}
