using HarmonyLib;
using UnityEngine;

namespace DoomArmour;

[HarmonyPatch(typeof(EntityAlive), "damageEntityLocal")]
public static class DamageSplit
{
	public static void Prefix(EntityAlive __instance, ref int _strength)
	{
		if (_strength <= 0 || !(__instance is EntityPlayer player))
		{
			return;
		}
		float share = Armour.Share(player);
		if (share <= 0f)
		{
			return;
		}
		Stat pool = Armour.Pool(player);
		if (pool == null)
		{
			return;
		}
		int held = Mathf.FloorToInt(pool.Value);
		if (held <= 0)
		{
			return;
		}
		int want = Mathf.RoundToInt((float)_strength * share);
		if (want > 0)
		{
			int relief = Mathf.Min(Armour.Level(player, "perkReinforcedArmour"), want);
			int paid = Mathf.Min(held, want - relief);
			_strength -= relief + paid;
			if (!player.isEntityRemote)
			{
				pool.Value = pool.Value - (float)paid + (float)Armour.Refund(player);
			}
		}
	}
}
