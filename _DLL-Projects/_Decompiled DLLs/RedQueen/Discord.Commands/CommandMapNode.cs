using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Discord.Commands;

internal class CommandMapNode
{
	private static readonly char[] WhitespaceChars = new char[3] { ' ', '\r', '\n' };

	private readonly ConcurrentDictionary<string, CommandMapNode> _nodes;

	private readonly string _name;

	private readonly object _lockObj = new object();

	private System.Collections.Immutable.ImmutableArray<CommandInfo> _commands;

	public bool IsEmpty
	{
		get
		{
			if (_commands.Length == 0)
			{
				return _nodes.Count == 0;
			}
			return false;
		}
	}

	public CommandMapNode(string name)
	{
		_name = name;
		_nodes = new ConcurrentDictionary<string, CommandMapNode>();
		_commands = System.Collections.Immutable.ImmutableArray.Create<CommandInfo>();
	}

	public void AddCommand(CommandService service, string text, int index, CommandInfo command)
	{
		int num = NextSegment(text, index, service._separatorChar);
		lock (_lockObj)
		{
			if (text == "")
			{
				if (_name == "")
				{
					throw new InvalidOperationException("Cannot add commands to the root node.");
				}
				_commands = _commands.Add(command);
			}
			else
			{
				string text2 = ((num != -1) ? text.Substring(index, num - index) : text.Substring(index));
				string fullName = ((_name == "") ? text2 : (_name + service._separatorChar + text2));
				_nodes.GetOrAdd(text2, (string x) => new CommandMapNode(fullName)).AddCommand(service, (num == -1) ? "" : text, num + 1, command);
			}
		}
	}

	public void RemoveCommand(CommandService service, string text, int index, CommandInfo command)
	{
		int num = NextSegment(text, index, service._separatorChar);
		lock (_lockObj)
		{
			if (text == "")
			{
				_commands = _commands.Remove(command);
				return;
			}
			string key = ((num != -1) ? text.Substring(index, num - index) : text.Substring(index));
			if (_nodes.TryGetValue(key, out var value))
			{
				value.RemoveCommand(service, (num == -1) ? "" : text, num + 1, command);
				if (value.IsEmpty)
				{
					_nodes.TryRemove(key, out value);
				}
			}
		}
	}

	public IEnumerable<CommandMatch> GetCommands(CommandService service, string text, int index, bool visitChildren = true)
	{
		System.Collections.Immutable.ImmutableArray<CommandInfo> commands = _commands;
		for (int i = 0; i < commands.Length; i++)
		{
			yield return new CommandMatch(_commands[i], _name);
		}
		if (!visitChildren)
		{
			yield break;
		}
		int num = NextSegment(text, index, service._separatorChar);
		string key = ((num != -1) ? text.Substring(index, num - index) : text.Substring(index));
		if (_nodes.TryGetValue(key, out var value))
		{
			foreach (CommandMatch command in value.GetCommands(service, (num == -1) ? "" : text, num + 1))
			{
				yield return command;
			}
		}
		num = NextSegment(text, index, WhitespaceChars, service._separatorChar);
		if (num == -1)
		{
			yield break;
		}
		key = text.Substring(index, num - index);
		if (!_nodes.TryGetValue(key, out value))
		{
			yield break;
		}
		foreach (CommandMatch command2 in value.GetCommands(service, (num == -1) ? "" : text, num + 1, visitChildren: false))
		{
			yield return command2;
		}
	}

	private static int NextSegment(string text, int startIndex, char separator)
	{
		return text.IndexOf(separator, startIndex);
	}

	private static int NextSegment(string text, int startIndex, char[] separators, char except)
	{
		int num = int.MaxValue;
		for (int i = 0; i < separators.Length; i++)
		{
			if (separators[i] != except)
			{
				int num2 = text.IndexOf(separators[i], startIndex);
				if (num2 != -1 && num2 < num)
				{
					num = num2;
				}
			}
		}
		if (num == int.MaxValue)
		{
			return -1;
		}
		return num;
	}
}
