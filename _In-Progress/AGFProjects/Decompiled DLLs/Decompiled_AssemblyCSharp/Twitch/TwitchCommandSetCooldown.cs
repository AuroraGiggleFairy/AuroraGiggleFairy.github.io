using System.Collections.Generic;

namespace Twitch;

public class TwitchCommandSetCooldown : BaseTwitchCommand
{
	public override PermissionLevels RequiredPermission => PermissionLevels.Mod;

	public override string[] CommandText => new string[1] { "#setcooldown" };

	public override string[] LocalizedCommandNames => new string[1] { Localization.Get("TwitchCommand_SetCooldown") };

	public override void Execute(ViewerEntry entry, TwitchIRCClient.TwitchChatMessage message)
	{
		string[] array = message.Message.Split(' ');
		if (array.Length != 2)
		{
			return;
		}
		int result = 0;
		if (int.TryParse(array[1], out result))
		{
			if (result < 0)
			{
				result = 0;
			}
			TwitchManager.Current.SetCooldown((float)result + 0.5f, TwitchManager.CooldownTypes.Time);
		}
	}

	public override void ExecuteConsole(List<string> arguments)
	{
		if (arguments.Count != 2)
		{
			return;
		}
		int result = 0;
		if (int.TryParse(arguments[1], out result))
		{
			if (result < 0)
			{
				result = 0;
			}
			TwitchManager.Current.SetCooldown((float)result + 0.5f, TwitchManager.CooldownTypes.Time);
		}
	}
}
