using System;
using HarmonyLib;

[HarmonyPatch(typeof(GameManager))]
[HarmonyPatch("RequestToEnterGame")]
[HarmonyPatch(new Type[] { typeof(ClientInfo) })]
public class Patch_GameManager_RequestToEnterGame
{
	private static bool Prefix(ClientInfo _cInfo)
	{
		_cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageLootRespawnTweak>().Setup(GamePrefs.GetInt(EnumGamePrefs.LootRespawnDays)));
		return true;
	}
}
