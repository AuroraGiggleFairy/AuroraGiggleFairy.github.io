using System.Collections.Generic;

namespace Twitch;

public class BaseTwitchCommand
{
	public enum PermissionLevels
	{
		Everyone,
		VIP,
		Sub,
		Mod,
		Broadcaster
	}

	public List<string> CommandTextList = new List<string>();

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string allText;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static Dictionary<string, PermissionLevels> CommandPermissionOverrides;

	public virtual PermissionLevels RequiredPermission => PermissionLevels.Everyone;

	public virtual string[] CommandText => null;

	public virtual string[] LocalizedCommandNames => null;

	[PublicizedFrom(EAccessModifier.Private)]
	static BaseTwitchCommand()
	{
		allText = "";
		CommandPermissionOverrides = new Dictionary<string, PermissionLevels>();
	}

	public static void ClearCommandPermissionOverrides()
	{
		CommandPermissionOverrides.Clear();
	}

	public static void AddCommandPermissionOverride(string commandName, PermissionLevels permissionLevel)
	{
		if (!CommandPermissionOverrides.ContainsKey(commandName))
		{
			CommandPermissionOverrides.Add(commandName, permissionLevel);
		}
	}

	public static PermissionLevels GetPermission(BaseTwitchCommand cmd)
	{
		if (CommandPermissionOverrides.ContainsKey(cmd.CommandText[0]))
		{
			return CommandPermissionOverrides[cmd.CommandText[0]];
		}
		return cmd.RequiredPermission;
	}

	public BaseTwitchCommand()
	{
		SetupCommandTextList();
		if (allText == "")
		{
			allText = Localization.Get("lblAll");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupCommandTextList()
	{
		CommandTextList.AddRange(CommandText);
		string[] localizedCommandNames = LocalizedCommandNames;
		for (int i = 0; i < localizedCommandNames.Length; i++)
		{
			if (!CommandTextList.Contains(localizedCommandNames[i]))
			{
				CommandTextList.Add(localizedCommandNames[i]);
			}
		}
	}

	public virtual bool CheckAllowed(TwitchIRCClient.TwitchChatMessage message)
	{
		return GetPermission(this) switch
		{
			PermissionLevels.Everyone => true, 
			PermissionLevels.Mod => message.isMod, 
			PermissionLevels.Broadcaster => message.isBroadcaster, 
			PermissionLevels.VIP => message.isVIP, 
			PermissionLevels.Sub => message.isSub, 
			_ => false, 
		};
	}

	public virtual void Execute(ViewerEntry entry, TwitchIRCClient.TwitchChatMessage message)
	{
	}

	public virtual void ExecuteConsole(List<string> arguments)
	{
	}
}
