using System.Collections.Generic;

namespace Twitch;

public class TwitchCommandUnpauseCommand : BaseTwitchCommand
{
	public override PermissionLevels RequiredPermission => PermissionLevels.Mod;

	public override string[] CommandText => new string[1] { "#unpause" };

	public override string[] LocalizedCommandNames => new string[1] { Localization.Get("TwitchCommand_Unpause") };

	public override void Execute(ViewerEntry entry, TwitchIRCClient.TwitchChatMessage message)
	{
		if (message.Message.Split(' ').Length == 1)
		{
			TwitchManager.Current.SetTwitchActive(newActive: true);
		}
	}

	public override void ExecuteConsole(List<string> arguments)
	{
		if (arguments.Count == 1)
		{
			TwitchManager.Current.SetTwitchActive(newActive: true);
		}
	}
}
