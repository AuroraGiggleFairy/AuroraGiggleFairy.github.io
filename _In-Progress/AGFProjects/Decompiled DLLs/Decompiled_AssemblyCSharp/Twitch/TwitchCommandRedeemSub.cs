using System.Collections.Generic;

namespace Twitch;

public class TwitchCommandRedeemSub : BaseTwitchCommand
{
	public override PermissionLevels RequiredPermission => PermissionLevels.Mod;

	public override string[] CommandText => new string[1] { "#redeem_sub" };

	public override string[] LocalizedCommandNames => new string[1] { Localization.Get("TwitchCommand_RedeemSub") };

	public override void Execute(ViewerEntry entry, TwitchIRCClient.TwitchChatMessage message)
	{
		string[] array = message.Message.Split(' ');
		if (array.Length == 2)
		{
			string text = array[1];
			text = ((!text.StartsWith("@")) ? text.ToLower() : text.Substring(1).ToLower());
			TwitchManager.Current.HandleSubEvent(text, 1, TwitchSubEventEntry.SubTierTypes.Tier1);
		}
		else if (array.Length == 3)
		{
			string text2 = array[1];
			text2 = ((!text2.StartsWith("@")) ? text2.ToLower() : text2.Substring(1).ToLower());
			int _result = 0;
			if (StringParsers.TryParseSInt32(array[2], out _result))
			{
				TwitchManager.Current.HandleSubEvent(text2, _result, TwitchSubEventEntry.SubTierTypes.Tier1);
			}
		}
		else
		{
			if (array.Length != 4)
			{
				return;
			}
			string text3 = array[1];
			text3 = ((!text3.StartsWith("@")) ? text3.ToLower() : text3.Substring(1).ToLower());
			int _result2 = 0;
			int _result3 = 0;
			TwitchSubEventEntry.SubTierTypes tier = TwitchSubEventEntry.SubTierTypes.Tier1;
			StringParsers.TryParseSInt32(array[2], out _result2);
			if (array[3].Trim().ToLower() == "prime")
			{
				tier = TwitchSubEventEntry.SubTierTypes.Prime;
			}
			else
			{
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
			}
			TwitchManager.Current.HandleSubEvent(text3, _result2, tier);
		}
	}

	public override void ExecuteConsole(List<string> arguments)
	{
		if (arguments.Count == 2)
		{
			string text = arguments[1];
			text = ((!text.StartsWith("@")) ? text.ToLower() : text.Substring(1).ToLower());
			TwitchManager.Current.HandleSubEvent(text, 1, TwitchSubEventEntry.SubTierTypes.Tier1);
		}
		else if (arguments.Count == 3)
		{
			string text2 = arguments[1];
			text2 = ((!text2.StartsWith("@")) ? text2.ToLower() : text2.Substring(1).ToLower());
			int _result = 0;
			if (StringParsers.TryParseSInt32(arguments[2], out _result))
			{
				TwitchManager.Current.HandleSubEvent(text2, _result, TwitchSubEventEntry.SubTierTypes.Tier1);
			}
		}
		else
		{
			if (arguments.Count != 4)
			{
				return;
			}
			string text3 = arguments[1];
			text3 = ((!text3.StartsWith("@")) ? text3.ToLower() : text3.Substring(1).ToLower());
			int _result2 = 0;
			int _result3 = 0;
			TwitchSubEventEntry.SubTierTypes tier = TwitchSubEventEntry.SubTierTypes.Tier1;
			StringParsers.TryParseSInt32(arguments[2], out _result2);
			if (arguments[3].Trim().ToLower() == "prime")
			{
				tier = TwitchSubEventEntry.SubTierTypes.Prime;
			}
			else
			{
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
			}
			TwitchManager.Current.HandleSubEvent(text3, _result2, tier);
		}
	}
}
