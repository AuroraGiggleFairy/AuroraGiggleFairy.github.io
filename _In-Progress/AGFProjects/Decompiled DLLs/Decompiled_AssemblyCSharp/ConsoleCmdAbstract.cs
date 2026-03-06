using System.Collections.Generic;
using Platform;

public abstract class ConsoleCmdAbstract : IConsoleCommand
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string[] commandNamesCache;

	[PublicizedFrom(EAccessModifier.Private)]
	public string commandDescriptionCache;

	[PublicizedFrom(EAccessModifier.Private)]
	public string commandHelpCache;

	[PublicizedFrom(EAccessModifier.Private)]
	public string primaryCommand;

	public virtual bool IsExecuteOnClient => false;

	public virtual int DefaultPermissionLevel => 0;

	public virtual bool AllowedInMainMenu => false;

	public virtual DeviceFlag AllowedDeviceTypes => DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX;

	public virtual DeviceFlag AllowedDeviceTypesClient => DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX;

	public virtual string PrimaryCommand => primaryCommand ?? (primaryCommand = GetCommands()[0]);

	public ConsoleCmdAbstract()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract string[] getCommands();

	public virtual string[] GetCommands()
	{
		return commandNamesCache ?? (commandNamesCache = getCommands());
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract string getDescription();

	public virtual string GetDescription()
	{
		return commandDescriptionCache ?? (commandDescriptionCache = getDescription());
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual string getHelp()
	{
		return null;
	}

	public virtual string GetHelp()
	{
		return commandHelpCache ?? (commandHelpCache = getHelp());
	}

	public abstract void Execute(List<string> _params, CommandSenderInfo _senderInfo);
}
