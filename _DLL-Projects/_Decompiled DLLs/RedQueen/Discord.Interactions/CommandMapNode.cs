using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Discord.Interactions;

internal class CommandMapNode<T> where T : class, ICommandInfo
{
	private const string RegexWildCardExp = "(\\S+)?";

	private readonly string _wildCardStr = "*";

	private readonly ConcurrentDictionary<string, CommandMapNode<T>> _nodes;

	private readonly ConcurrentDictionary<string, T> _commands;

	private readonly ConcurrentDictionary<Regex, T> _wildCardCommands;

	public IReadOnlyDictionary<string, CommandMapNode<T>> Nodes => _nodes;

	public IReadOnlyDictionary<string, T> Commands => _commands;

	public IReadOnlyDictionary<Regex, T> WildCardCommands => _wildCardCommands;

	public string Name { get; }

	public CommandMapNode(string name, string wildCardExp = null)
	{
		Name = name;
		_nodes = new ConcurrentDictionary<string, CommandMapNode<T>>();
		_commands = new ConcurrentDictionary<string, T>();
		_wildCardCommands = new ConcurrentDictionary<Regex, T>();
		if (!string.IsNullOrEmpty(wildCardExp))
		{
			_wildCardStr = wildCardExp;
		}
	}

	public void AddCommand(IList<string> keywords, int index, T commandInfo)
	{
		if (keywords.Count == index + 1)
		{
			if (commandInfo.SupportsWildCards && commandInfo.Name.Contains(_wildCardStr))
			{
				string text = RegexUtils.EscapeExcluding(commandInfo.Name, _wildCardStr.ToArray());
				Regex key = new Regex("\\A" + text.Replace(_wildCardStr, "(\\S+)?") + "\\Z", RegexOptions.Compiled | RegexOptions.Singleline);
				if (!_wildCardCommands.TryAdd(key, commandInfo))
				{
					throw new InvalidOperationException("A " + typeof(T).FullName + " already exists with the same name: " + string.Join(" ", keywords));
				}
			}
			else if (!_commands.TryAdd(keywords[index], commandInfo))
			{
				throw new InvalidOperationException("A " + typeof(T).FullName + " already exists with the same name: " + string.Join(" ", keywords));
			}
		}
		else
		{
			_nodes.GetOrAdd(keywords[index], (string name) => new CommandMapNode<T>(name, _wildCardStr)).AddCommand(keywords, ++index, commandInfo);
		}
	}

	public bool RemoveCommand(IList<string> keywords, int index)
	{
		T value;
		if (keywords.Count == index + 1)
		{
			return _commands.TryRemove(keywords[index], out value);
		}
		if (!_nodes.TryGetValue(keywords[index], out var value2))
		{
			throw new InvalidOperationException("No descendant node was found with the name " + keywords[index]);
		}
		return value2.RemoveCommand(keywords, ++index);
	}

	public SearchResult<T> GetCommand(IList<string> keywords, int index)
	{
		string text = string.Join(" ", keywords);
		CommandMapNode<T> value2;
		if (keywords.Count == index + 1)
		{
			if (_commands.TryGetValue(keywords[index], out var value))
			{
				return SearchResult<T>.FromSuccess(text, value);
			}
			foreach (KeyValuePair<Regex, T> wildCardCommand in _wildCardCommands)
			{
				Match match = wildCardCommand.Key.Match(keywords[index]);
				if (match.Success)
				{
					string[] array = new string[match.Groups.Count - 1];
					for (int i = 1; i < match.Groups.Count; i++)
					{
						array[i - 1] = match.Groups[i].Value;
					}
					return SearchResult<T>.FromSuccess(text, wildCardCommand.Value, array.ToArray());
				}
			}
		}
		else if (_nodes.TryGetValue(keywords[index], out value2))
		{
			return value2.GetCommand(keywords, ++index);
		}
		return SearchResult<T>.FromError(text, InteractionCommandError.UnknownCommand, "No " + typeof(T).FullName + " found for " + text);
	}

	public SearchResult<T> GetCommand(string text, int index, char[] seperators)
	{
		string[] keywords = text.Split(seperators);
		return GetCommand(keywords, index);
	}
}
