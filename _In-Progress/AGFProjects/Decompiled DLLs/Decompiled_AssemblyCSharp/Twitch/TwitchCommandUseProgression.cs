using System.Collections.Generic;

namespace Twitch;

public class TwitchCommandUseProgression : BaseTwitchCommand
{
	public override PermissionLevels RequiredPermission => PermissionLevels.Broadcaster;

	public override string[] CommandText => new string[1] { "#useprogression" };

	public override string[] LocalizedCommandNames => new string[1] { Localization.Get("TwitchCommand_UseProgression") };

	public override void Execute(ViewerEntry entry, TwitchIRCClient.TwitchChatMessage message)
	{
		string[] array = message.Message.Split(' ');
		if (array.Length == 2)
		{
			bool result = false;
			if (bool.TryParse(array[1], out result))
			{
				TwitchManager.Current.SetUseProgression(result);
			}
			TwitchManager.Current.SendChannelMessage("[7DTD]: Use Progression Enabled: " + (TwitchManager.Current.UseProgression ? "Yes" : "No"));
		}
	}

	public override void ExecuteConsole(List<string> arguments)
	{
		if (arguments.Count == 2)
		{
			bool result = false;
			if (bool.TryParse(arguments[1], out result))
			{
				TwitchManager.Current.SetUseProgression(result);
			}
			TwitchManager.Current.SendChannelMessage("[7DTD]: Use Progression Enabled: " + (TwitchManager.Current.UseProgression ? "Yes" : "No"));
		}
	}
}
