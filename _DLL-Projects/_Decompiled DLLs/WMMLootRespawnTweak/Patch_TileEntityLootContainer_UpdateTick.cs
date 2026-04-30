using System;
using HarmonyLib;

[HarmonyPatch(typeof(TileEntityLootContainer))]
[HarmonyPatch("UpdateTick")]
[HarmonyPatch(new Type[] { typeof(World) })]
public class Patch_TileEntityLootContainer_UpdateTick
{
	private static bool Prefix(TileEntityLootContainer __instance, World world)
	{
		PatchTileEntity.UpdateTick(__instance, world);
		if (GamePrefs.GetInt(EnumGamePrefs.LootRespawnDays) > 0 && !__instance.bPlayerStorage && __instance.bTouched && __instance.IsEmpty())
		{
			int num = GameUtils.WorldTimeToHours(__instance.worldTimeTouched);
			num += GameUtils.WorldTimeToDays(__instance.worldTimeTouched) * 24;
			if ((GameUtils.WorldTimeToHours(world.worldTime) + GameUtils.WorldTimeToDays(world.worldTime) * 24 - num) / 24 < GamePrefs.GetInt(EnumGamePrefs.LootRespawnDays))
			{
				return false;
			}
			__instance.bWasTouched = false;
			__instance.bTouched = false;
			__instance.SetModified();
		}
		return false;
	}
}
