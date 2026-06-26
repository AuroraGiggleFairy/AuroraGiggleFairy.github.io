using System.Collections.Generic;

namespace Twitch;

public class TwitchCommandTeleportBackpack : BaseTwitchCommand
{
	public override PermissionLevels RequiredPermission => PermissionLevels.Mod;

	public override string[] CommandText => new string[2] { "#tp_backpack", "#teleport_backpack" };

	public override string[] LocalizedCommandNames => new string[2]
	{
		Localization.Get("TwitchCommand_TeleportBackpack1"),
		Localization.Get("TwitchCommand_TeleportBackpack2")
	};

	public override void Execute(ViewerEntry entry, TwitchIRCClient.TwitchChatMessage message)
	{
		string[] array = message.Message.Split(' ');
		EntityPlayer localPlayer = TwitchManager.Current.LocalPlayer;
		if (array.Length == 2)
		{
			int _result = -1;
			if (StringParsers.TryParseSInt32(array[1], out _result) && TwitchManager.Current.LocalPlayer.Party != null)
			{
				localPlayer = TwitchManager.Current.LocalPlayer.Party.GetMemberAtIndex(_result, TwitchManager.Current.LocalPlayer);
				_ = localPlayer == null;
			}
		}
		else
		{
			GameEventManager.Current.HandleAction("action_teleport_backpack", localPlayer, localPlayer, twitchActivated: false);
		}
	}

	public override void ExecuteConsole(List<string> arguments)
	{
		EntityPlayer localPlayer = TwitchManager.Current.LocalPlayer;
		if (arguments.Count == 2)
		{
			int _result = -1;
			if (StringParsers.TryParseSInt32(arguments[1], out _result) && TwitchManager.Current.LocalPlayer.Party != null)
			{
				localPlayer = TwitchManager.Current.LocalPlayer.Party.GetMemberAtIndex(_result, TwitchManager.Current.LocalPlayer);
				_ = localPlayer == null;
			}
		}
		else
		{
			GameEventManager.Current.HandleAction("action_teleport_backpack", localPlayer, localPlayer, twitchActivated: false);
		}
	}
}
