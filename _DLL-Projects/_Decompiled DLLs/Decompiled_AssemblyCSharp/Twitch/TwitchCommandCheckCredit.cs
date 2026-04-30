using System.Collections.Generic;

namespace Twitch;

public class TwitchCommandCheckCredit : BaseTwitchCommand
{
	public override string[] CommandText => new string[1] { "#checkcredit" };

	public override string[] LocalizedCommandNames => new string[1] { Localization.Get("TwitchCommand_CheckCredit") };

	public override void Execute(ViewerEntry entry, TwitchIRCClient.TwitchChatMessage message)
	{
		string[] array = message.Message.Split(' ');
		if (array.Length == 2)
		{
			if (message.isMod || message.isBroadcaster)
			{
				string text = array[1];
				text = ((!text.StartsWith("@")) ? text.ToLower() : text.Substring(1).ToLower());
				TwitchManager current = TwitchManager.Current;
				if (current.ViewerData.HasViewerEntry(text))
				{
					current.SendChannelCreditOutputMessage(text);
				}
				else
				{
					current.ircClient.SendChannelMessage($"[7DTD]: No viewer data for {array[1]}.", useQueue: true);
				}
			}
		}
		else if (array.Length == 1)
		{
			TwitchManager.Current.SendChannelCreditOutputMessage(message.UserName);
		}
	}

	public override void ExecuteConsole(List<string> arguments)
	{
		if (arguments.Count == 2)
		{
			string text = arguments[1];
			text = ((!text.StartsWith("@")) ? text.ToLower() : text.Substring(1).ToLower());
			TwitchManager current = TwitchManager.Current;
			if (current.ViewerData.HasViewerEntry(text))
			{
				current.SendChannelPointOutputMessage(text);
			}
			else
			{
				current.ircClient.SendChannelMessage($"[7DTD]: No viewer data for {arguments[1]}.", useQueue: true);
			}
		}
		else if (arguments.Count == 1)
		{
			TwitchManager.Current.SendChannelPointOutputMessage(TwitchManager.Current.Authentication.userName);
		}
	}
}
