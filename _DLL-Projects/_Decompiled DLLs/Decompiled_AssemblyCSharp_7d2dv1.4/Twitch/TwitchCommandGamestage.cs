using System.Collections.Generic;

namespace Twitch;

public class TwitchCommandGamestage : BaseTwitchCommand
{
	public override string[] CommandText => new string[2] { "#gamestage", "#gs" };

	public override string[] LocalizedCommandNames => new string[2]
	{
		Localization.Get("TwitchCommand_Gamestage1"),
		Localization.Get("TwitchCommand_Gamestage1")
	};

	public override void Execute(ViewerEntry entry, TwitchIRCClient.TwitchChatMessage message)
	{
		TwitchManager.Current.DisplayGameStage();
	}

	public override void ExecuteConsole(List<string> arguments)
	{
		TwitchManager.Current.DisplayGameStage();
	}
}
