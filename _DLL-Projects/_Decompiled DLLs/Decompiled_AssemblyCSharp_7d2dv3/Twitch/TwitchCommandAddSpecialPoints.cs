using System.Collections.Generic;

namespace Twitch;

public class TwitchCommandAddSpecialPoints : BaseTwitchCommand
{
	public override PermissionLevels RequiredPermission => PermissionLevels.Mod;

	public override string[] CommandText => new string[1] { "#addsp" };

	public override string[] LocalizedCommandNames => new string[1] { Localization.Get("TwitchCommand_AddSpecialPoints") };

	public override void Execute(ViewerEntry entry, TwitchIRCClient.TwitchChatMessage message)
	{
		string[] array = message.Message.Split(' ');
		if (array.Length != 3)
		{
			return;
		}
		int result = 0;
		if (int.TryParse(array[2], out result))
		{
			if (array[1].EqualsCaseInsensitive(BaseTwitchCommand.allText))
			{
				TwitchManager.Current.ViewerData.AddPoints("", result, isSpecial: true, displayNewTotal: true);
			}
			else
			{
				TwitchManager.Current.ViewerData.AddPoints(array[1], result, isSpecial: true, displayNewTotal: true);
			}
		}
	}

	public override void ExecuteConsole(List<string> arguments)
	{
		if (arguments.Count != 3)
		{
			return;
		}
		int result = 0;
		if (int.TryParse(arguments[2], out result))
		{
			if (arguments[1].EqualsCaseInsensitive(BaseTwitchCommand.allText))
			{
				TwitchManager.Current.ViewerData.AddPoints("", result, isSpecial: true, displayNewTotal: true);
			}
			else
			{
				TwitchManager.Current.ViewerData.AddPoints(arguments[1], result, isSpecial: true, displayNewTotal: true);
			}
		}
	}
}
