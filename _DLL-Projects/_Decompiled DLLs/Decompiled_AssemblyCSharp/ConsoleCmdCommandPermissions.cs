using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdCommandPermissions : ConsoleCmdAbstract
{
	public override bool AllowedInMainMenu => true;

	public override DeviceFlag AllowedDeviceTypes => DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "commandpermission", "cp" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Manage command permission levels";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Set/get permission levels required to execute a given command. Default\nlevel required for commands that are not explicitly specified is 0.\nUsage:\n   cp add <command> <level>\n   cp remove <command>\n   cp list";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (GameManager.Instance.adminTools == null)
		{
			return;
		}
		if (_params.Count >= 1)
		{
			if (_params[0].EqualsCaseInsensitive("add"))
			{
				ExecuteAdd(_params);
			}
			else if (_params[0].EqualsCaseInsensitive("remove"))
			{
				ExecuteRemove(_params);
			}
			else if (_params[0].EqualsCaseInsensitive("list"))
			{
				ExecuteList();
			}
			else
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Invalid sub command \"" + _params[0] + "\".");
			}
		}
		else
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No sub command given.");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ExecuteAdd(List<string> _params)
	{
		if (_params.Count != 3)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Wrong number of arguments, expected 3, found " + _params.Count + ".");
			return;
		}
		if (SingletonMonoBehaviour<SdtdConsole>.Instance.GetCommand(_params[1]) == null)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("\"" + _params[1] + "\" is not a valid command.");
			return;
		}
		if (!int.TryParse(_params[2], out var result))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("\"" + _params[2] + "\" is not a valid integer.");
			return;
		}
		GameManager.Instance.adminTools.Commands.AddCommand(_params[1], result);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"{_params[1]} added with permission level of {result}.");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ExecuteRemove(List<string> _params)
	{
		if (_params.Count != 2)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Wrong number of arguments, expected 2, found " + _params.Count + ".");
			return;
		}
		IConsoleCommand command = SingletonMonoBehaviour<SdtdConsole>.Instance.GetCommand(_params[1]);
		if (command == null)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("\"" + _params[1] + "\" is not a valid command.");
			return;
		}
		GameManager.Instance.adminTools.Commands.RemoveCommand(command.GetCommands());
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"{_params[1]} removed from permissions list.");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ExecuteList()
	{
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Defined Command Permissions:");
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("  Level: Command");
		foreach (KeyValuePair<string, AdminCommands.CommandPermission> command in GameManager.Instance.adminTools.Commands.GetCommands())
		{
			AdminCommands.CommandPermission value = command.Value;
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"  {value.PermissionLevel,5}: {value.Command}");
		}
	}
}
