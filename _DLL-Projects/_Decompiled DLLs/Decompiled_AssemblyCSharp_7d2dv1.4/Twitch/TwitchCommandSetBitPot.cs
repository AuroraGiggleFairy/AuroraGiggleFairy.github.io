using System.Collections.Generic;

namespace Twitch;

public class TwitchCommandSetBitPot : BaseTwitchCommand
{
	public override PermissionLevels RequiredPermission => PermissionLevels.Mod;

	public override string[] CommandText => new string[1] { "#setbitpot" };

	public override string[] LocalizedCommandNames => new string[1] { Localization.Get("TwitchCommand_SetBitPot") };

	public override void Execute(ViewerEntry entry, TwitchIRCClient.TwitchChatMessage message)
	{
		string[] array = message.Message.Split(' ');
		if (array.Length == 2)
		{
			int result = 0;
			if (int.TryParse(array[1], out result))
			{
				TwitchManager.Current.SetBitPot(result);
			}
		}
	}

	public override void ExecuteConsole(List<string> arguments)
	{
		if (arguments.Count == 2)
		{
			int result = 0;
			if (int.TryParse(arguments[1], out result))
			{
				TwitchManager.Current.SetBitPot(result);
			}
		}
	}
}
