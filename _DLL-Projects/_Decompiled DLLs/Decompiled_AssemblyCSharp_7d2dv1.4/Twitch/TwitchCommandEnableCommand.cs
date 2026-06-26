using System.Collections.Generic;

namespace Twitch;

public class TwitchCommandEnableCommand : BaseTwitchCommand
{
	public override PermissionLevels RequiredPermission => PermissionLevels.Mod;

	public override string[] CommandText => new string[1] { "#enable" };

	public override string[] LocalizedCommandNames => new string[1] { Localization.Get("TwitchCommand_Enable") };

	public override void Execute(ViewerEntry entry, TwitchIRCClient.TwitchChatMessage message)
	{
		string[] array = message.Message.Split(' ');
		if (array.Length != 2)
		{
			return;
		}
		TwitchManager current = TwitchManager.Current;
		bool flag = false;
		foreach (string key in TwitchActionManager.TwitchActions.Keys)
		{
			TwitchAction twitchAction = TwitchActionManager.TwitchActions[key];
			if (twitchAction.IsInPreset(current.CurrentActionPreset) && twitchAction.Command.Equals(array[1]))
			{
				twitchAction.Enabled = true;
				flag = true;
			}
		}
		if (flag)
		{
			current.SendChannelMessage("[7DTD]: Command Enabled: " + array[1]);
			current.SetupAvailableCommands();
		}
	}

	public override void ExecuteConsole(List<string> arguments)
	{
		if (arguments.Count != 2)
		{
			return;
		}
		TwitchManager current = TwitchManager.Current;
		bool flag = false;
		foreach (string key in TwitchActionManager.TwitchActions.Keys)
		{
			TwitchAction twitchAction = TwitchActionManager.TwitchActions[key];
			if (twitchAction.IsInPreset(current.CurrentActionPreset) && twitchAction.Command.Equals(arguments[1]))
			{
				twitchAction.Enabled = true;
				flag = true;
			}
		}
		if (flag)
		{
			current.SendChannelMessage("[7DTD]: Command Enabled: " + arguments[1]);
			current.SetupAvailableCommands();
		}
	}
}
