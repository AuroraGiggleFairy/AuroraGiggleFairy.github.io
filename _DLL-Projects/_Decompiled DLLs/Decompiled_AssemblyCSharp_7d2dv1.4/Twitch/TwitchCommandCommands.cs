using System.Collections.Generic;

namespace Twitch;

public class TwitchCommandCommands : BaseTwitchCommand
{
	public override PermissionLevels RequiredPermission => PermissionLevels.Mod;

	public override string[] CommandText => new string[1] { "#commands" };

	public override string[] LocalizedCommandNames => new string[1] { Localization.Get("TwitchCommand_Commands") };

	public override void Execute(ViewerEntry entry, TwitchIRCClient.TwitchChatMessage message)
	{
		TwitchManager.Current.DisplayCommands(message.isBroadcaster, message.isMod, message.isVIP, message.isSub);
	}

	public override void ExecuteConsole(List<string> arguments)
	{
		TwitchManager.Current.DisplayCommands(isBroadcaster: true, isMod: true, isVIP: true, isSub: true);
	}
}
