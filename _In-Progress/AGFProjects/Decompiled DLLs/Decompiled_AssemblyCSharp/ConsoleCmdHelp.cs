using System;
using System.Collections.Generic;
using System.Text;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdHelp : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Private)]
	public struct HelpTopic(string _desc, Action<List<string>> _action, string _actionCompleteText)
	{
		public string Description = _desc;

		public Action<List<string>> Action = _action;

		public string ActionCompleteText = _actionCompleteText;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static StringBuilder sb = new StringBuilder();

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, HelpTopic> helpTopics = new Dictionary<string, HelpTopic>
	{
		{
			"output",
			new HelpTopic("Prints commands to log file", OutputHelp, "Printed commands to log file")
		},
		{
			"outputdetailed",
			new HelpTopic("Prints commands with details to log file", OutputDetailedHelp, "Printed commands with details to log file")
		},
		{
			"search",
			new HelpTopic("Search for all commands matching a string", SearchHelp, "<first argument will be the string to match>")
		},
		{
			"*",
			new HelpTopic("Search for all commands matching a string", SearchHelp, "<first argument will be the string to match>")
		}
	};

	public override bool AllowedInMainMenu => true;

	public override int DefaultPermissionLevel => 1000;

	public override DeviceFlag AllowedDeviceTypes => DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "help" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Help on console and specific commands";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "\r\n\t\t\t|Usage:\r\n\t\t\t|  1. help\r\n\t\t\t|  2. help * <searchstring>\r\n\t\t\t|  3. help <command name>\r\n\t\t\t|  4. help output\r\n\t\t\t|  5. help outputdetailed\r\n\t\t\t|1. Show general help and list all available commands\r\n\t\t\t|2. List commands where either the name or the description contains the given text\r\n\t\t\t|3. Show help for the given command\r\n\t\t\t|4. Write command list to log file\r\n\t\t\t|5. Write command list with help texts to log file\r\n\t\t\t".Unindent();
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		sb.Clear();
		if (_params.Count == 0)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("*** Generic Console Help ***");
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("To get further help on a specific topic or command type (without the brackets)");
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("    help <topic / command>");
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("");
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Generic notation of command parameters:");
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("   <param name>              Required parameter");
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("   <entityId / player name>  Possible types of parameter values");
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("   [param name]              Optional parameter");
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("");
			sb.Clear();
			BuildStringCommandDescriptions();
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(sb.ToString());
			sb.Clear();
		}
		else
		{
			Action<List<string>> action = null;
			if (helpTopics.ContainsKey(_params[0]))
			{
				action = helpTopics[_params[0]].Action;
			}
			sb.Clear();
			BuildStringHelpText(_params[0]);
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(sb.ToString());
			sb.Clear();
			action?.Invoke(_params);
		}
	}

	public static void ValidateNoCommandOverlap()
	{
		IList<IConsoleCommand> commands = SingletonMonoBehaviour<SdtdConsole>.Instance.GetCommands();
		for (int i = 0; i < commands.Count; i++)
		{
			string[] commands2 = commands[i].GetCommands();
			foreach (string text in commands2)
			{
				if (helpTopics.ContainsKey(text))
				{
					Log.Warning("Command with alias \"" + text + "\" conflicts with help topic command");
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void BuildStringCommandDescriptions()
	{
		sb.AppendLine("*** List of Help Topics ***");
		foreach (KeyValuePair<string, HelpTopic> helpTopic in helpTopics)
		{
			sb.Append(helpTopic.Key);
			sb.Append(" => ");
			sb.AppendLine(helpTopic.Value.Description);
		}
		sb.AppendLine("");
		sb.AppendLine("*** List of Commands ***");
		IList<IConsoleCommand> commands = SingletonMonoBehaviour<SdtdConsole>.Instance.GetCommands();
		for (int i = 0; i < commands.Count; i++)
		{
			IConsoleCommand consoleCommand = commands[i];
			if (consoleCommand.CanExecuteForDevice && consoleCommand != null)
			{
				string[] commands2 = consoleCommand.GetCommands();
				foreach (string value in commands2)
				{
					sb.Append(" ");
					sb.Append(value);
				}
				sb.Append(" => ");
				sb.AppendLine(consoleCommand.GetDescription());
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void BuildStringHelpText(string key)
	{
		string text = null;
		string text2 = null;
		if (helpTopics.ContainsKey(key))
		{
			text = "Topic: " + key;
			text2 = helpTopics[key].ActionCompleteText;
		}
		else
		{
			IConsoleCommand command = SingletonMonoBehaviour<SdtdConsole>.Instance.GetCommand(key);
			if (command != null && command.CanExecuteForDevice)
			{
				text = "Command(s): " + string.Join(", ", command.GetCommands());
				text2 = command.GetHelp();
				if (string.IsNullOrEmpty(text2))
				{
					text2 = "No detailed help available.\nDescription: " + command.GetDescription();
				}
			}
		}
		if (text != null)
		{
			sb.AppendLine("*** " + text + " ***");
			string[] array = text2.Split('\n');
			foreach (string value in array)
			{
				sb.AppendLine(value);
			}
		}
		else
		{
			sb.AppendLine("No command or topic found by \"" + key + "\"");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void OutputHelp(List<string> _params)
	{
		sb.Clear();
		BuildStringCommandDescriptions();
		Log.Out(sb.ToString());
		sb.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void OutputDetailedHelp(List<string> _params)
	{
		sb.Clear();
		sb.AppendLine("*** List of Help Topics ***");
		foreach (KeyValuePair<string, HelpTopic> helpTopic in helpTopics)
		{
			BuildStringHelpText(helpTopic.Key);
			sb.AppendLine();
		}
		sb.AppendLine("*** List of Commands ***");
		foreach (IConsoleCommand command in SingletonMonoBehaviour<SdtdConsole>.Instance.GetCommands())
		{
			BuildStringHelpText(command.GetCommands()[0]);
			sb.AppendLine();
		}
		Log.Out(sb.ToString());
		sb.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void SearchHelp(List<string> _params)
	{
		sb.Clear();
		if (_params.Count < 2)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Argument for search mask missing");
			return;
		}
		string text = _params[1];
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("*** List of Commands for \"" + text + "\" ***");
		IList<IConsoleCommand> commands = SingletonMonoBehaviour<SdtdConsole>.Instance.GetCommands();
		for (int i = 0; i < commands.Count; i++)
		{
			IConsoleCommand consoleCommand = commands[i];
			string description = consoleCommand.GetDescription();
			bool flag = text == null;
			if (!flag)
			{
				flag = description.ContainsCaseInsensitive(text);
				string[] commands2 = consoleCommand.GetCommands();
				foreach (string a in commands2)
				{
					flag |= a.ContainsCaseInsensitive(text);
				}
			}
			if (flag)
			{
				string[] commands2 = consoleCommand.GetCommands();
				foreach (string value in commands2)
				{
					sb.Append(" ");
					sb.Append(value);
				}
				sb.Append(" => ");
				sb.Append(description);
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output(sb.ToString());
				sb.Length = 0;
			}
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output(sb.ToString());
		sb.Clear();
	}
}
