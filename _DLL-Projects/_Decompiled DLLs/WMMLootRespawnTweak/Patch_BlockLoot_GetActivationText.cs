using System;
using HarmonyLib;

[HarmonyPatch(typeof(BlockLoot))]
[HarmonyPatch("GetActivationText")]
[HarmonyPatch(new Type[]
{
	typeof(WorldBase),
	typeof(BlockValue),
	typeof(int),
	typeof(Vector3i),
	typeof(EntityAlive)
})]
public class Patch_BlockLoot_GetActivationText
{
	private static bool Prefix(BlockLoot __instance, ref string __result, WorldBase _world, int _clrIdx, Vector3i _blockPos)
	{
		if (GamePrefs.GetInt(EnumGamePrefs.LootRespawnDays) <= 0)
		{
			return true;
		}
		if (!(_world.GetTileEntity(_clrIdx, _blockPos) is TileEntityLootContainer { bTouched: not false } tileEntityLootContainer))
		{
			return true;
		}
		if (tileEntityLootContainer.IsEmpty())
		{
			int num = GameUtils.WorldTimeToHours(tileEntityLootContainer.worldTimeTouched) + GameUtils.WorldTimeToDays(tileEntityLootContainer.worldTimeTouched) * 24;
			int num2 = num + GamePrefs.GetInt(EnumGamePrefs.LootRespawnDays) * 24;
			string arg = $"Day {num2 / 24} at {num2 % 24}:00";
			__result = $"Restocking on {arg}";
			return false;
		}
		return true;
	}
}
