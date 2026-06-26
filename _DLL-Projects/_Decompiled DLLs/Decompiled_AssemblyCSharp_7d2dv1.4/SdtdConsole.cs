#define ENABLE_MONO
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class SdtdConsole : SingletonMonoBehaviour<SdtdConsole>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class CommandInstance
	{
		public readonly string command;

		public readonly IConsoleConnection sender;

		public CommandInstance(string _command, IConsoleConnection _sender)
		{
			command = _command;
			sender = _sender;
		}
	}

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<IConsoleCommand> m_Commands = new List<IConsoleCommand>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, IConsoleCommand> m_CommandsAllVariants = new CaseInsensitiveStringDictionary<IConsoleCommand>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ReadOnlyCollection<IConsoleCommand> m_CommandsReadOnly;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<IConsoleServer> m_Servers = new List<IConsoleServer>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<CommandInstance> m_commandsToExecuteAsync = new List<CommandInstance>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<string> m_currentCommandOutputList = new List<string>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int hideCommandExecutionLog = -1;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<string> tokenizedCommandList = new List<string>(16);

	public int HideCommandExecutionLog
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (hideCommandExecutionLog < 0)
			{
				hideCommandExecutionLog = GamePrefs.GetInt(EnumGamePrefs.HideCommandExecutionLog);
			}
			return hideCommandExecutionLog;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void singletonAwake()
	{
		Log.LogCallbacksExtended += LogCallback;
		m_CommandsReadOnly = new ReadOnlyCollection<IConsoleCommand>(m_Commands);
		PreserveCheckPatch.Enable();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LogCallback(string _formattedMessage, string _plainMessage, string _trace, LogType _type, DateTime _timestamp, long _uptime)
	{
		if (!GameManager.IsDedicatedServer)
		{
			return;
		}
		for (int i = 0; i < m_Servers.Count; i++)
		{
			IConsoleServer consoleServer = m_Servers[i];
			try
			{
				consoleServer.SendLog(_formattedMessage, _plainMessage, _trace, _type, _timestamp, _uptime);
			}
			catch (Exception e)
			{
				Log.Error("Error sending to console server:");
				Log.Exception(e);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Update()
	{
		if (m_commandsToExecuteAsync.Count <= 0)
		{
			return;
		}
		lock (m_commandsToExecuteAsync)
		{
			try
			{
				CommandSenderInfo senderInfo = new CommandSenderInfo
				{
					IsLocalGame = false,
					RemoteClientInfo = null,
					NetworkConnection = m_commandsToExecuteAsync[0].sender
				};
				List<string> output = executeCommand(m_commandsToExecuteAsync[0].command, senderInfo);
				m_commandsToExecuteAsync[0].sender.SendLines(output);
			}
			catch (Exception e)
			{
				Log.Exception(e);
			}
			m_commandsToExecuteAsync.RemoveAt(0);
		}
	}

	public void Output(string _line)
	{
		m_currentCommandOutputList?.Add(_line);
	}

	public void Output(string _format, params object[] _args)
	{
		Output(string.Format(_format, _args));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RegisterCommand(SortedList<string, IConsoleCommand> _commandsList, string _className, IConsoleCommand _command)
	{
		string[] commands = _command.GetCommands();
		string text = commands[0];
		if (_commandsList.ContainsKey(text))
		{
			Log.Warning("Command with name \"" + text + "\" already loaded, not loading from class " + _className);
			return;
		}
		_commandsList.Add(text, _command);
		if (GameManager.Instance.adminTools != null && !GameManager.Instance.adminTools.Commands.IsPermissionDefined(commands) && _command.DefaultPermissionLevel != 0)
		{
			Log.Out("Command \"{0}\" has no explicit permission level, but a default permission of {1}, adding to permission list", text, _command.DefaultPermissionLevel);
			GameManager.Instance.adminTools.Commands.AddCommand(text, _command.DefaultPermissionLevel, _save: false);
		}
	}

	public void RegisterCommands()
	{
		SortedList<string, IConsoleCommand> commandsList = new SortedList<string, IConsoleCommand>();
		ReflectionHelpers.FindTypesImplementingBase(typeof(IConsoleCommand), [PublicizedFrom(EAccessModifier.Internal)] (Type _type) =>
		{
			IConsoleCommand consoleCommand = ReflectionHelpers.Instantiate<IConsoleCommand>(_type);
			if (consoleCommand != null)
			{
				RegisterCommand(commandsList, _type.Name, consoleCommand);
			}
		});
		try
		{
			foreach (IConsoleCommand value2 in commandsList.Values)
			{
				m_Commands.Add(value2);
				for (int num = 0; num < value2.GetCommands().Length; num++)
				{
					string text = value2.GetCommands()[num];
					if (!string.IsNullOrEmpty(text))
					{
						if (m_CommandsAllVariants.TryGetValue(text, out var value))
						{
							Log.Warning("Command with alias \"" + text + "\" already registered from " + value.GetType().Name + ", not registering for class " + value2.GetType().Name);
						}
						else
						{
							m_CommandsAllVariants.Add(text, value2);
						}
					}
				}
			}
			ConsoleCmdHelp.ValidateNoCommandOverlap();
		}
		catch (Exception e)
		{
			Log.Error("Error registering commands");
			Log.Exception(e);
		}
		GameManager.Instance.adminTools?.Save();
	}

	public void RegisterServer(IConsoleServer _server)
	{
		if (_server != null)
		{
			m_Servers.Add(_server);
		}
	}

	public void Cleanup()
	{
		for (int i = 0; i < m_Servers.Count; i++)
		{
			IConsoleServer consoleServer = m_Servers[i];
			try
			{
				consoleServer.Disconnect();
			}
			catch (Exception e)
			{
				Log.Error("Error sending to console server:");
				Log.Exception(e);
			}
		}
		m_Servers.Clear();
	}

	public IConsoleCommand GetCommand(string _command, bool _alreadyTokenized = false)
	{
		if (!_alreadyTokenized)
		{
			int num = _command.IndexOf(' ');
			if (num >= 0)
			{
				_command = _command.Substring(0, num);
			}
		}
		if (m_CommandsAllVariants.TryGetValue(_command, out var value))
		{
			return value;
		}
		return null;
	}

	public IList<IConsoleCommand> GetCommands()
	{
		return m_CommandsReadOnly;
	}

	public void ExecuteAsync(string _command, IConsoleConnection _sender)
	{
		CommandInstance item = new CommandInstance(_command, _sender);
		lock (m_commandsToExecuteAsync)
		{
			m_commandsToExecuteAsync.Add(item);
		}
	}

	public List<string> ExecuteSync(string _command, ClientInfo _cInfo)
	{
		CommandSenderInfo senderInfo = new CommandSenderInfo
		{
			IsLocalGame = (_cInfo == null),
			RemoteClientInfo = _cInfo,
			NetworkConnection = null
		};
		return executeCommand(_command, senderInfo);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<string> executeCommand(string _command, CommandSenderInfo _senderInfo)
	{
		if (string.IsNullOrEmpty(_command))
		{
			return null;
		}
		m_currentCommandOutputList.Clear();
		List<string> list = tokenizeCommand(_command);
		if (list != null)
		{
			if (list[0] == string.Empty)
			{
				return null;
			}
			IConsoleCommand command = GetCommand(list[0], _alreadyTokenized: true);
			if (command != null)
			{
				if (!command.CanExecuteForDevice)
				{
					m_currentCommandOutputList.Add("*** ERROR: Command '" + list[0] + "' can not be executed on this device type.");
					return m_currentCommandOutputList;
				}
				if (GameManager.Instance.World == null && !command.AllowedInMainMenu)
				{
					m_currentCommandOutputList.Add("*** ERROR: Command '" + list[0] + "' can only be executed when a game is started.");
					return m_currentCommandOutputList;
				}
				if (_senderInfo.IsLocalGame)
				{
					if (HideCommandExecutionLog < 3)
					{
						Log.Out("Executing command '" + _command + "'");
					}
				}
				else if (_senderInfo.RemoteClientInfo != null)
				{
					if (HideCommandExecutionLog < 2)
					{
						Log.Out("Executing command '" + _command + "' from client " + _senderInfo.RemoteClientInfo);
					}
				}
				else if (HideCommandExecutionLog < 1)
				{
					Log.Out("Executing command '" + _command + "' by " + _senderInfo.NetworkConnection.GetDescription());
				}
				try
				{
					command.Execute(list.GetRange(1, list.Count - 1), _senderInfo);
				}
				catch (Exception ex)
				{
					m_currentCommandOutputList.Add("*** ERROR: Executing command '" + list[0] + "' failed: " + ex.Message);
					Log.Exception(ex);
				}
			}
			else
			{
				m_currentCommandOutputList.Add("*** ERROR: unknown command '" + list[0] + "'");
			}
		}
		return m_currentCommandOutputList;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<string> tokenizeCommand(string _command)
	{
		List<string> list = tokenizedCommandList;
		list.Clear();
		bool flag = false;
		int num = 0;
		for (int i = 0; i < _command.Length; i++)
		{
			if (!flag)
			{
				if (_command[i] == '"')
				{
					if (i - num > 0)
					{
						list.Add(_command.Substring(num, i - num));
					}
					num = i + 1;
					flag = true;
				}
				else if (_command[i] == ' ' || _command[i] == '\t')
				{
					if (i - num > 0)
					{
						list.Add(_command.Substring(num, i - num));
					}
					num = i + 1;
				}
			}
			else if (_command[i] == '"')
			{
				if (i + 1 < _command.Length && _command[i + 1] == '"')
				{
					i++;
					continue;
				}
				string text = _command.Substring(num, i - num);
				text = text.Replace("\"\"", "\"");
				list.Add(text);
				num = i + 1;
				flag = false;
			}
		}
		if (flag)
		{
			m_currentCommandOutputList.Add("*** ERROR: Quotation started at position " + num + " was not closed");
			return null;
		}
		if (num < _command.Length)
		{
			list.Add(_command.Substring(num, _command.Length - num));
		}
		return list;
	}
}
