using System.Collections.Generic;

namespace Twitch;

public class TwitchCommandRedeemCharity : BaseTwitchCommand
{
	public override PermissionLevels RequiredPermission => PermissionLevels.Mod;

	public override string[] CommandText => new string[1] { "#redeem_charity" };

	public override string[] LocalizedCommandNames => new string[1] { Localization.Get("TwitchCommand_RedeemCharity") };

	public override void Execute(ViewerEntry entry, TwitchIRCClient.TwitchChatMessage message)
	{
		string[] array = message.Message.Split(' ');
		if (array.Length == 2)
		{
			int _result = 0;
			if (StringParsers.TryParseSInt32(array[1], out _result))
			{
				TwitchManager.Current.HandleCharityRedeem(message.UserName, _result);
			}
		}
		else if (array.Length == 3)
		{
			string text = array[1];
			text = ((!text.StartsWith("@")) ? text.ToLower() : text.Substring(1).ToLower());
			int _result2 = 0;
			if (StringParsers.TryParseSInt32(array[2], out _result2))
			{
				TwitchManager.Current.HandleCharityRedeem(text, _result2);
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
				TwitchManager.Current.HandleCharityRedeem(TwitchManager.Current.Authentication.userName, _result);
			}
		}
		else if (arguments.Count == 3)
		{
			string text = arguments[1];
			text = ((!text.StartsWith("@")) ? text.ToLower() : text.Substring(1).ToLower());
			int _result2 = 0;
			if (StringParsers.TryParseSInt32(arguments[2], out _result2))
			{
				TwitchManager.Current.HandleCharityRedeem(text, _result2);
			}
		}
	}
}
