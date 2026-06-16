using System.Collections.Generic;
using System.Text;

namespace Twitch;

public class TwitchCommandDebug : BaseTwitchCommand
{
	public override PermissionLevels RequiredPermission => PermissionLevels.Mod;

	public override string[] CommandText => new string[1] { "#debug" };

	public override string[] LocalizedCommandNames => new string[1] { Localization.Get("#debug") };

	public override void Execute(ViewerEntry entry, TwitchIRCClient.TwitchChatMessage message)
	{
		TwitchManager.Current.DisplayDebug(message.Message);
	}

	public override void ExecuteConsole(List<string> arguments)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < arguments.Count; i++)
		{
			stringBuilder.Append(arguments[i] + " ");
		}
		TwitchManager.Current.DisplayDebug(stringBuilder.ToString());
	}
}
