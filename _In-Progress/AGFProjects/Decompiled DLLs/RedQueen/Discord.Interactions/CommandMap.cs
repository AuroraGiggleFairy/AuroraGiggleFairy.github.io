using System;
using System.Collections.Generic;
using System.Linq;

namespace Discord.Interactions;

internal class CommandMap<T> where T : class, ICommandInfo
{
	private readonly char[] _seperators;

	private readonly CommandMapNode<T> _root;

	private readonly InteractionService _commandService;

	public IReadOnlyCollection<char> Seperators => _seperators;

	public CommandMap(InteractionService commandService, char[] seperators = null)
	{
		_seperators = seperators ?? Array.Empty<char>();
		_commandService = commandService;
		_root = new CommandMapNode<T>(null, _commandService._wildCardExp);
	}

	public void AddCommand(T command, bool ignoreGroupNames = false)
	{
		if (ignoreGroupNames)
		{
			AddCommandToRoot(command);
		}
		else
		{
			AddCommand(command);
		}
	}

	public void AddCommandToRoot(T command)
	{
		string[] keywords = new string[1] { command.Name };
		_root.AddCommand(keywords, 0, command);
	}

	public void AddCommand(IList<string> input, T command)
	{
		_root.AddCommand(input, 0, command);
	}

	public void RemoveCommand(T command)
	{
		IList<string> commandPath = command.GetCommandPath();
		_root.RemoveCommand(commandPath, 0);
	}

	public SearchResult<T> GetCommand(string input)
	{
		if (_seperators.Any())
		{
			return GetCommand(input.Split(_seperators));
		}
		return GetCommand(new string[1] { input });
	}

	public SearchResult<T> GetCommand(IList<string> input)
	{
		return _root.GetCommand(input, 0);
	}

	private void AddCommand(T command)
	{
		IList<string> commandPath = command.GetCommandPath();
		_root.AddCommand(commandPath, 0, command);
	}
}
