using System.Collections.Generic;

namespace Twitch;

public class TwitchCommandRemoveViewer : BaseTwitchCommand
{
	public override PermissionLevels RequiredPermission => PermissionLevels.Mod;

	public override string[] CommandText => new string[1] { "#remove_viewer" };

	public override string[] LocalizedCommandNames => new string[1] { Localization.Get("TwitchCommand_RemoveViewer") };

	public override void Execute(ViewerEntry entry, TwitchIRCClient.TwitchChatMessage message)
	{
		string[] array = message.Message.Split(' ');
		if (array.Length == 2)
		{
			TwitchManager.Current.ViewerData.RemoveViewerEntry(array[1]);
		}
	}

	public override void ExecuteConsole(List<string> arguments)
	{
		if (arguments.Count == 2)
		{
			TwitchManager.Current.ViewerData.RemoveViewerEntry(arguments[1]);
		}
	}
}
