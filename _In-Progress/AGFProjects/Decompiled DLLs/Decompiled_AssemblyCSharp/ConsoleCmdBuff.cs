using System;
using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdBuff : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient => true;

	public override DeviceFlag AllowedDeviceTypesClient => DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "buff" };
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (!_senderInfo.IsLocalGame)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Command can only be used on clients, use \"buffplayer\" instead for other players / remote clients");
		}
		else if (_params.Count == 1)
		{
			EntityPlayer primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
			if (primaryPlayer != null)
			{
				switch (primaryPlayer.Buffs.AddBuff(_params[0]))
				{
				case EntityBuffs.BuffStatus.FailedInvalidName:
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Buff failed: buff \"" + _params[0] + "\" unknown");
					PrintAvailableBuffNames();
					break;
				case EntityBuffs.BuffStatus.FailedImmune:
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Buff failed: entity is immune to \"" + _params[0] + "\"");
					break;
				case EntityBuffs.BuffStatus.FailedFriendlyFire:
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Buff failed: entity is friendly");
					break;
				case EntityBuffs.BuffStatus.FailedEditor:
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Buff failed: buff " + _params[0] + " not allowed in editor");
					break;
				case EntityBuffs.BuffStatus.FailedGameStat:
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Buff failed: missing required game stat.");
					break;
				}
			}
		}
		else
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("buff requires a buff name as the only argument!");
			PrintAvailableBuffNames();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Applies a buff to the local player";
	}

	public static void PrintAvailableBuffNames()
	{
		SortedDictionary<string, BuffClass> sortedDictionary = new SortedDictionary<string, BuffClass>(BuffManager.Buffs, StringComparer.OrdinalIgnoreCase);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Available buffs:");
		foreach (KeyValuePair<string, BuffClass> item in sortedDictionary)
		{
			if (item.Key.Equals(item.Value.LocalizedName))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output(" - " + item.Key);
				continue;
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(" - " + item.Key + " (" + item.Value.LocalizedName + ")");
		}
	}
}
