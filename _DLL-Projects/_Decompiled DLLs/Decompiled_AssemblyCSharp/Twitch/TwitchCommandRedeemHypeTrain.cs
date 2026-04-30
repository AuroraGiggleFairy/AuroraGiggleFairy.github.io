using System.Collections.Generic;

namespace Twitch;

public class TwitchCommandRedeemHypeTrain : BaseTwitchCommand
{
	public override PermissionLevels RequiredPermission => PermissionLevels.Mod;

	public override string[] CommandText => new string[1] { "#redeem_hypetrain" };

	public override string[] LocalizedCommandNames => new string[1] { Localization.Get("TwitchCommand_RedeemHypeTrain") };

	public override void Execute(ViewerEntry entry, TwitchIRCClient.TwitchChatMessage message)
	{
		string[] array = message.Message.Split(' ');
		if (array.Length == 2)
		{
			int _result = 0;
			if (StringParsers.TryParseSInt32(array[1], out _result))
			{
				TwitchManager.Current.HandleHypeTrainRedeem(_result);
			}
		}
	}

	public override void ExecuteConsole(List<string> arguments)
	{
		if (arguments.Count == 2)
		{
			int _result = 0;
			if (StringParsers.TryParseSInt32(arguments[1], out _result))
			{
				TwitchManager.Current.HandleHypeTrainRedeem(_result);
			}
		}
	}
}
