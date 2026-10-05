using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using DoomLevels;
using HarmonyLib;

namespace DoomLevelsMpFix
{
	/// <summary>
	/// Supply drops are aimed at living players. A player inside a level sits in instance
	/// space, so the crate and the plane were built around that level. Drop those players
	/// before the clusters are made. If nobody is in the real world, leave the drop due
	/// until someone is.
	/// </summary>
	[HarmonyPatch(typeof(AIDirectorAirDropComponent), nameof(AIDirectorAirDropComponent.SpawnAirDrop))]
	internal static class Patch_AirDropWait
	{
		private static bool Prefix(AIDirectorAirDropComponent __instance, ref bool __result)
		{
			if (__instance == null || __instance.activeAirDrop != null || __instance.Director == null)
			{
				return true;
			}

			AIDirectorPlayerManagementComponent players = __instance.Director.GetComponent<AIDirectorPlayerManagementComponent>();
			if (players?.trackedPlayers?.list == null)
			{
				return true;
			}

			bool anyone = false;
			for (int i = 0; i < players.trackedPlayers.list.Count; i++)
			{
				EntityPlayer player = players.trackedPlayers.list[i]?.Player;
				if (player == null || player.IsDead())
				{
					continue;
				}

				anyone = true;
				if (!Instances.IsInstanceSpace(player.position))
				{
					return true;
				}
			}

			if (!anyone)
			{
				return true;
			}

			__result = false;
			return false;
		}
	}

	[HarmonyPatch(typeof(AIAirDrop), nameof(AIAirDrop.MakePlayerClusters))]
	internal static class Patch_AirDropPlayers
	{
		private static readonly FieldInfo NumPlayers = AccessTools.Field(typeof(AIAirDrop), "numPlayers");

		private static void Prefix(AIAirDrop __instance, List<EntityPlayer> _players)
		{
			if (_players == null)
			{
				return;
			}

			for (int i = _players.Count - 1; i >= 0; i--)
			{
				EntityPlayer player = _players[i];
				if (player != null && Instances.IsInstanceSpace(player.position))
				{
					_players.RemoveAt(i);
				}
			}

			if (NumPlayers != null && __instance != null)
			{
				NumPlayers.SetValue(__instance, _players.Count);
			}
		}
	}

	[HarmonyPatch(typeof(AIAirDrop), nameof(AIAirDrop.CreateFlightPaths))]
	internal static class Patch_AirDropPaths
	{
		private static readonly FieldInfo Clusters = AccessTools.Field(typeof(AIAirDrop), "clusters");
		private static readonly FieldInfo FlightPaths = AccessTools.Field(typeof(AIAirDrop), "flightPaths");

		private static bool Prefix(AIAirDrop __instance)
		{
			IList clusters = Clusters != null ? Clusters.GetValue(__instance) as IList : null;
			if (clusters != null && clusters.Count > 0)
			{
				return true;
			}

			if (FlightPaths != null && __instance != null)
			{
				FlightPaths.SetValue(__instance, System.Activator.CreateInstance(FlightPaths.FieldType));
			}

			return false;
		}
	}
}
