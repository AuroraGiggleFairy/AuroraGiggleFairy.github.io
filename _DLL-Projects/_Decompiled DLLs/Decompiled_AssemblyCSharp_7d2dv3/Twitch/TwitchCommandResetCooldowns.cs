using System.Collections.Generic;

namespace Twitch;

public class TwitchCommandResetCooldowns : BaseTwitchCommand
{
	public override PermissionLevels RequiredPermission => PermissionLevels.Mod;

	public override string[] CommandText => new string[1] { "#reset_cooldowns" };

	public override string[] LocalizedCommandNames => new string[1] { Localization.Get("TwitchCommand_ResetCooldowns") };

	public override void Execute(ViewerEntry entry, TwitchIRCClient.TwitchChatMessage message)
	{
		TwitchManager current = TwitchManager.Current;
		float currentUnityTime = current.CurrentUnityTime;
		foreach (string key in TwitchActionManager.TwitchActions.Keys)
		{
			TwitchActionManager.TwitchActions[key].ResetCooldown(currentUnityTime);
		}
		current.SetupAvailableCommands();
		current.SendChannelMessage("Action Cooldowns have been reset!");
	}

	public override void ExecuteConsole(List<string> arguments)
	{
		if (arguments.Count != 1)
		{
			return;
		}
		TwitchManager current = TwitchManager.Current;
		float currentUnityTime = current.CurrentUnityTime;
		foreach (string key in TwitchActionManager.TwitchActions.Keys)
		{
			TwitchActionManager.TwitchActions[key].ResetCooldown(currentUnityTime);
		}
		current.SetupAvailableCommands();
		current.SendChannelMessage("Action Cooldowns have been reset!");
	}
}
