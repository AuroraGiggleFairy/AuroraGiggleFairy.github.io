using System.Collections.Generic;

namespace Twitch;

public class TwitchCommandRedeemRaid : BaseTwitchCommand
{
	public override PermissionLevels RequiredPermission => PermissionLevels.Mod;

	public override string[] CommandText => new string[1] { "#redeem_raid" };

	public override string[] LocalizedCommandNames => new string[1] { Localization.Get("TwitchCommand_RedeemRaid") };

	public override void Execute(ViewerEntry entry, TwitchIRCClient.TwitchChatMessage message)
	{
		string[] array = message.Message.Split(' ');
		if (array.Length == 3)
		{
			string text = array[1];
			text = ((!text.StartsWith("@")) ? text.ToLower() : text.Substring(1).ToLower());
			int _result = 0;
			if (StringParsers.TryParseSInt32(array[2], out _result))
			{
				TwitchManager.Current.HandleRaidRedeem(text, _result);
			}
		}
	}

	public override void ExecuteConsole(List<string> arguments)
	{
		if (arguments.Count == 3)
		{
			string text = arguments[1];
			text = ((!text.StartsWith("@")) ? text.ToLower() : text.Substring(1).ToLower());
			int _result = 0;
			if (StringParsers.TryParseSInt32(arguments[2], out _result))
			{
				TwitchManager.Current.HandleRaidRedeem(text, _result);
			}
		}
	}
}
