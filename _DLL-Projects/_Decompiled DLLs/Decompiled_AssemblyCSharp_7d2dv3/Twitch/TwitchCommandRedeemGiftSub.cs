using System.Collections.Generic;

namespace Twitch;

public class TwitchCommandRedeemGiftSub : BaseTwitchCommand
{
	public override PermissionLevels RequiredPermission => PermissionLevels.Mod;

	public override string[] CommandText => new string[1] { "#redeem_giftsub" };

	public override string[] LocalizedCommandNames => new string[1] { Localization.Get("TwitchCommand_RedeemGiftSubs") };

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
				TwitchManager.Current.HandleGiftSubEvent(text, _result, TwitchSubEventEntry.SubTierTypes.Tier1);
			}
		}
		else if (array.Length == 4)
		{
			string text2 = array[1];
			text2 = ((!text2.StartsWith("@")) ? text2.ToLower() : text2.Substring(1).ToLower());
			int _result2 = 0;
			int _result3 = 0;
			TwitchSubEventEntry.SubTierTypes tier = TwitchSubEventEntry.SubTierTypes.Tier1;
			StringParsers.TryParseSInt32(array[2], out _result2);
			StringParsers.TryParseSInt32(array[3], out _result3);
			switch (_result3)
			{
			case 2:
				tier = TwitchSubEventEntry.SubTierTypes.Tier2;
				break;
			case 3:
				tier = TwitchSubEventEntry.SubTierTypes.Tier3;
				break;
			}
			TwitchManager.Current.HandleGiftSubEvent(text2, _result2, tier);
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
				TwitchManager.Current.HandleGiftSubEvent(text, _result, TwitchSubEventEntry.SubTierTypes.Tier1);
			}
		}
		else if (arguments.Count == 4)
		{
			string text2 = arguments[1];
			text2 = ((!text2.StartsWith("@")) ? text2.ToLower() : text2.Substring(1).ToLower());
			int _result2 = 0;
			int _result3 = 0;
			TwitchSubEventEntry.SubTierTypes tier = TwitchSubEventEntry.SubTierTypes.Tier1;
			StringParsers.TryParseSInt32(arguments[2], out _result2);
			StringParsers.TryParseSInt32(arguments[3], out _result3);
			switch (_result3)
			{
			case 2:
				tier = TwitchSubEventEntry.SubTierTypes.Tier2;
				break;
			case 3:
				tier = TwitchSubEventEntry.SubTierTypes.Tier3;
				break;
			}
			TwitchManager.Current.HandleGiftSubEvent(text2, _result2, tier);
		}
	}
}
