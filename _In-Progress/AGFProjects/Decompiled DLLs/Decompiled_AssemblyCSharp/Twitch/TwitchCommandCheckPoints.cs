using System.Collections.Generic;

namespace Twitch;

public class TwitchCommandCheckPoints : BaseTwitchCommand
{
	public override string[] CommandText => new string[2] { "#checkpoints", "#cp" };

	public override string[] LocalizedCommandNames => new string[2]
	{
		Localization.Get("TwitchCommand_CheckPoints1"),
		Localization.Get("TwitchCommand_CheckPoints2")
	};

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
					current.SendChannelPointOutputMessage(text);
				}
				else
				{
					current.ircClient.SendChannelMessage($"[7DTD]: No viewer data for {array[1]}.", useQueue: true);
				}
			}
		}
		else if (array.Length == 1)
		{
			TwitchManager.Current.SendChannelPointOutputMessage(message.UserName);
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
