using DoomLevels;
using HarmonyLib;
using UnityEngine;

namespace DoomLevelsMpFix
{
	internal static class HazardScan
	{
		internal static void Tick()
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance == null || SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
			{
				return;
			}

			World world = GameManager.Instance?.World;
			if (world?.Players?.list == null || DamageTrigger.Active == null)
			{
				return;
			}

			for (int t = DamageTrigger.Active.Count - 1; t >= 0; t--)
			{
				DamageTrigger trigger = DamageTrigger.Active[t];
				if (trigger == null || string.IsNullOrEmpty(trigger.Buff))
				{
					continue;
				}

				Collider[] cols = trigger.GetComponentsInChildren<Collider>(true);
				if (cols == null || cols.Length == 0)
				{
					continue;
				}

				for (int p = 0; p < world.Players.list.Count; p++)
				{
					EntityPlayer player = world.Players.list[p];
					if (player == null || player.IsDead() || player.Buffs == null || !Instances.IsInstanceSpace(player.position))
					{
						continue;
					}

					if (!StandingOn(cols, player))
					{
						continue;
					}

					Apply(trigger, player);
				}
			}
		}

		private static void Apply(DamageTrigger trigger, EntityPlayer player)
		{
			Collider playerCol = player.GetComponentInChildren<Collider>();
			if (playerCol != null)
			{
				trigger.Touched(playerCol);
			}
		}

		private static bool StandingOn(Collider[] cols, EntityPlayer player)
		{
			Bounds body = player.boundingBox;
			for (int i = 0; i < cols.Length; i++)
			{
				Collider col = cols[i];
				if (col == null || !col.enabled)
				{
					continue;
				}

				Bounds box = col.bounds;
				box.center += Origin.position;
				if (box.Intersects(body))
				{
					return true;
				}
			}

			return false;
		}
	}

	[HarmonyPatch(typeof(DamageTrigger), "OnDisable")]
	internal static class Patch_HazardStayOnDedicated
	{
		private static bool Prefix()
		{
			return !GameManager.IsDedicatedServer;
		}
	}
}
