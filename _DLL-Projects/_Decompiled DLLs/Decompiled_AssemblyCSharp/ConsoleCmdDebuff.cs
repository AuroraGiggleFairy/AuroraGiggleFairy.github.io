using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdDebuff : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient => true;

	public override DeviceFlag AllowedDeviceTypesClient => DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "debuff" };
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (!_senderInfo.IsLocalGame)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Command can only be used on clients, use \"debuffplayer\" instead for other players / remote clients");
			return;
		}
		EntityPlayer primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		if (primaryPlayer != null)
		{
			if (_params.Count == 1)
			{
				if (primaryPlayer.Buffs.GetBuff(_params[0]) == null)
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Debuff failed: buff \"" + _params[0] + "\" unknown or not active");
					PrintActiveBuffNames(primaryPlayer);
				}
				else
				{
					primaryPlayer.Buffs.RemoveBuff(_params[0]);
				}
			}
			else
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("debuff requires a buff name as the only argument!");
				PrintActiveBuffNames(primaryPlayer);
			}
		}
		else
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No local player found.");
		}
	}

	public static void PrintActiveBuffNames(EntityPlayer _player)
	{
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Active buffs:");
		foreach (BuffValue activeBuff in _player.Buffs.ActiveBuffs)
		{
			if (activeBuff != null && activeBuff.BuffClass != null)
			{
				BuffClass buffClass = activeBuff.BuffClass;
				if (buffClass.Name.Equals(buffClass.LocalizedName))
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output(" - " + buffClass.Name);
					continue;
				}
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output(" - " + buffClass.Name + " (" + buffClass.LocalizedName + ")");
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Removes a buff from the local player";
	}
}
