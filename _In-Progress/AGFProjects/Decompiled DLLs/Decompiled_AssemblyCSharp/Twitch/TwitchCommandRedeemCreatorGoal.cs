using System.Collections.Generic;

namespace Twitch;

public class TwitchCommandRedeemCreatorGoal : BaseTwitchCommand
{
	public override PermissionLevels RequiredPermission => PermissionLevels.Mod;

	public override string[] CommandText => new string[1] { "#redeem_goal" };

	public override string[] LocalizedCommandNames => new string[1] { Localization.Get("TwitchCommand_RedeemGoal") };

	public override void Execute(ViewerEntry entry, TwitchIRCClient.TwitchChatMessage message)
	{
		string[] array = message.Message.Split(' ');
		if (array.Length == 2)
		{
			TwitchManager.Current.HandleCreatorGoalRedeem(array[1]);
		}
	}

	public override void ExecuteConsole(List<string> arguments)
	{
		if (arguments.Count == 2)
		{
			TwitchManager.Current.HandleCreatorGoalRedeem(arguments[1]);
		}
	}
}
