using System;
using DoomLevels;
using HarmonyLib;
using UnityEngine;

namespace DoomLevelsMpFix
{
	/// <summary>
	/// The blood moon day and the horde that runs past midnight both close level
	/// entry. Someone already standing in a level can still be registered again
	/// after a relog. A new trip from the real map is refused, and the note is
	/// not taken.
	/// </summary>
	internal static class BloodMoonEnter
	{
		internal const string ClosedMessage = "Doom levels are closed on Blood Moon day.";

		internal static bool Closed()
		{
			World world = GameManager.Instance?.World;
			if (world == null)
			{
				return false;
			}

			int bloodMoonDay = GameStats.GetInt(EnumGameStats.BloodMoonDay);
			if (bloodMoonDay <= 0)
			{
				return false;
			}

			if (GameUtils.WorldTimeToDays(world.worldTime) == bloodMoonDay)
			{
				return true;
			}

			(int duskHour, int dawnHour) duskDawn = GameUtils.CalcDuskDawnHours(GameStats.GetInt(EnumGameStats.DayLightLength));
			return GameUtils.IsBloodMoonTime(world.worldTime, duskDawn, bloodMoonDay);
		}

		internal static bool IsLevelNote(string questId)
		{
			return !string.IsNullOrEmpty(questId) && questId.StartsWith("doom_", StringComparison.OrdinalIgnoreCase);
		}
	}

	[HarmonyPatch(typeof(Instances), nameof(Instances.Enter))]
	internal static class Patch_BloodMoonEnter
	{
		[HarmonyPriority(Priority.First)]
		private static bool Prefix(EntityPlayer player)
		{
			if (player == null || Instances.IsInstanceSpace(player.position) || !BloodMoonEnter.Closed())
			{
				return true;
			}

			GameManager.ShowTooltipMP(player, BloodMoonEnter.ClosedMessage);
			Debug.Log("[DoomMultiplayer] refused level entry during blood moon for " + player.entityId);
			return false;
		}
	}

	[HarmonyPatch(typeof(ItemActionQuest), nameof(ItemActionQuest.ExecuteInstantAction))]
	internal static class Patch_BloodMoonNote
	{
		private static bool Prefix(ItemActionQuest __instance, EntityAlive ent, ref bool __result)
		{
			if (__instance == null || !BloodMoonEnter.IsLevelNote(__instance.QuestGiven) || !BloodMoonEnter.Closed())
			{
				return true;
			}

			EntityPlayerLocal local = ent as EntityPlayerLocal;
			if (local != null)
			{
				GameManager.ShowTooltip(local, BloodMoonEnter.ClosedMessage);
			}

			__result = false;
			return false;
		}
	}

	[HarmonyPatch(typeof(XUiC_QuestOfferWindow), "btnAccept_OnPress")]
	internal static class Patch_BloodMoonNoteAccept
	{
		private static bool Prefix(XUiC_QuestOfferWindow __instance)
		{
			if (__instance == null || !BloodMoonEnter.IsLevelNote(__instance.Quest?.ID) || !BloodMoonEnter.Closed())
			{
				return true;
			}

			EntityPlayerLocal local = __instance.xui?.playerUI?.entityPlayer;
			if (local != null)
			{
				GameManager.ShowTooltip(local, BloodMoonEnter.ClosedMessage);
			}

			__instance.xui?.playerUI?.windowManager?.Close(__instance.WindowGroup);
			return false;
		}
	}
}
