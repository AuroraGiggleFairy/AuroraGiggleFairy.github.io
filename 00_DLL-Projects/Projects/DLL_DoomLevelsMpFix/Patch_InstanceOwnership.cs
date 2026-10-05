using System;
using DoomLevels;
using HarmonyLib;
using UnityEngine;

namespace DoomLevelsMpFix
{
	internal static class ChunkOwner
	{
		[ThreadStatic]
		internal static int PlayerId;
	}

	internal static class Handoff
	{
		internal static int PlayerId;
	}

	[HarmonyPatch(typeof(Instances), nameof(Instances.Handover))]
	internal static class Patch_Instances_Handover
	{
		private static void Prefix(EntityPlayer player)
		{
			Handoff.PlayerId = player != null ? player.entityId : 0;
		}
	}

	[HarmonyPatch(typeof(Instances), nameof(Instances.Enter))]
	internal static class Patch_Instances_Enter
	{
		private static void Prefix(EntityPlayer player)
		{
			ChunkOwner.PlayerId = player != null ? player.entityId : -1;
		}

		private static void Postfix(EntityPlayer player)
		{
			try
			{
				if (player != null && Instances.IsInside(player.entityId))
				{
					InstanceSync.SendEnter(player);
				}
			}
			finally
			{
				if (player != null && Handoff.PlayerId == player.entityId)
				{
					Handoff.PlayerId = 0;
				}

				ChunkOwner.PlayerId = -1;
			}
		}
	}

	[HarmonyPatch(typeof(Instances), nameof(Instances.Release))]
	internal static class Patch_Instances_Release
	{
		private static void Prefix(int playerId, out bool __state)
		{
			__state = Instances.IsInside(playerId);
		}

		private static void Postfix(int playerId, bool __state)
		{
			if (!__state)
			{
				return;
			}

			try
			{
				if (Handoff.PlayerId == playerId)
				{
					PartyHud.RequestRefresh();
					return;
				}

				InstanceSync.SendLeave(playerId);
			}
			catch (Exception e)
			{
				Debug.LogError("[DoomMultiplayer] leave hook: " + e.Message);
			}
		}
	}

	[HarmonyPatch(typeof(ChunkManager), nameof(ChunkManager.AddChunkObserver))]
	internal static class Patch_ChunkManager_AddChunkObserver
	{
		private static void Prefix(ref int _entityIdToSendChunksTo)
		{
			int id = ChunkOwner.PlayerId;
			if (id > 0 && _entityIdToSendChunksTo == -1)
			{
				_entityIdToSendChunksTo = id;
			}
		}
	}

	[HarmonyPatch(typeof(Instances), nameof(Instances.TryGet))]
	internal static class Patch_Instances_TryGet
	{
		private static void Postfix(int playerId, ref Vector3i origin, ref Level level, ref int cell, ref bool __result)
		{
			if (__result)
			{
				return;
			}

			if (!InstanceSync.TryGet(playerId, out string map, out origin, out cell))
			{
				return;
			}

			LevelDb.Load();
			level = LevelDb.Get(map);
			__result = level != null;
		}
	}
}
