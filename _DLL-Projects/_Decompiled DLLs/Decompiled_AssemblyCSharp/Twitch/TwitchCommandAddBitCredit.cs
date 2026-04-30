using System.Collections.Generic;

namespace Twitch;

public class TwitchCommandAddBitCredit : BaseTwitchCommand
{
	public override PermissionLevels RequiredPermission => PermissionLevels.Mod;

	public override string[] CommandText => new string[1] { "#addcredit" };

	public override string[] LocalizedCommandNames => new string[1] { Localization.Get("TwitchCommand_AddBitCredit") };

	public override void Execute(ViewerEntry entry, TwitchIRCClient.TwitchChatMessage message)
	{
		string[] array = message.Message.Split(' ');
		if (array.Length == 3)
		{
			int result = 0;
			if (int.TryParse(array[2], out result))
			{
				TwitchManager.Current.ViewerData.AddCredit(array[1], result, displayNewTotal: true);
			}
		}
	}

	public override void ExecuteConsole(List<string> arguments)
	{
		if (arguments.Count == 3)
		{
			int result = 0;
			if (int.TryParse(arguments[2], out result))
			{
				TwitchManager.Current.ViewerData.AddCredit(arguments[1], result, displayNewTotal: true);
			}
		}
	}
}
