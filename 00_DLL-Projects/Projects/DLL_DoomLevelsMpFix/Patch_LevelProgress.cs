using System;
using System.Reflection;
using DoomLevels;
using HarmonyLib;
using UnityEngine;

namespace DoomLevelsMpFix
{
	/// <summary>
	/// A rebuilt level stamps the prefab again. Remembered spawner spots do not
	/// spawn, and remembered item spots are put back to air. Logout keeps the
	/// loaded copy under the player's id so the next login can take that copy.
	/// </summary>
	[HarmonyPatch(typeof(Spawner), nameof(Spawner.Spawn))]
	internal static class Patch_NoteSpawner
	{
		[ThreadStatic]
		private static bool _active;

		[ThreadStatic]
		private static Vector3i _pos;

		private static void Prefix(in SpawnQueue.SpawnData data)
		{
			_active = true;
			_pos = data.Pos;
		}

		private static void Postfix()
		{
			_active = false;
		}

		internal static bool Active => _active;

		internal static Vector3i Pos => _pos;
	}

	[HarmonyPatch(typeof(World), nameof(World.SpawnEntityInWorld))]
	internal static class Patch_NoteSpawnedMonster
	{
		private static void Postfix(Entity _entity)
		{
			if (!Patch_NoteSpawner.Active || _entity == null)
			{
				return;
			}

			RunStats.NoteSpawn(_entity.entityId, Patch_NoteSpawner.Pos);
		}
	}

	[HarmonyPatch(typeof(BlockSpawner), nameof(BlockSpawner.OnBlockEntityTransformAfterActivated))]
	internal static class Patch_SkipClearedSpawner
	{
		private static void Prefix(Vector3i _blockPos, ref BlockValue _blockValue)
		{
			if (RunStats.SpawnerCleared(_blockPos))
			{
				_blockValue.rawData |= Spawner.SpentBit;
			}
		}
	}

	[HarmonyPatch(typeof(Instances), "RestoreThings")]
	internal static class Patch_RestoreClearedSpots
	{
		private static void Postfix()
		{
			RunStats.ApplyLoaded();
		}
	}

	[HarmonyPatch(typeof(Item), nameof(Item.Touched))]
	internal static class Patch_RememberItem
	{
		private static readonly FieldInfo Taken = AccessTools.Field(typeof(Item), "_taken");
		private static readonly FieldInfo BlockPos = AccessTools.Field(typeof(Item), "_blockPos");

		private static void Postfix(Item __instance)
		{
			if (Taken == null || BlockPos == null || __instance == null || !(bool)Taken.GetValue(__instance))
			{
				return;
			}

			RunStats.ItemTaken((Vector3i)BlockPos.GetValue(__instance));
		}
	}

	[HarmonyPatch(typeof(Secret), nameof(Secret.Touched))]
	internal static class Patch_RememberSecret
	{
		private static readonly FieldInfo Found = AccessTools.Field(typeof(Secret), "_found");
		private static readonly FieldInfo BlockPos = AccessTools.Field(typeof(Secret), "_blockPos");

		private static void Postfix(Secret __instance)
		{
			if (Found == null || BlockPos == null || __instance == null || !(bool)Found.GetValue(__instance))
			{
				return;
			}

			RunStats.SecretFound((Vector3i)BlockPos.GetValue(__instance));
		}
	}

	[HarmonyPatch(typeof(NetPackageDoomItem), nameof(NetPackageDoomItem.ProcessPackage))]
	internal static class Patch_ItemCountOnce
	{
		private static bool Prefix(NetPackageDoomItem __instance)
		{
			int playerId = __instance != null && __instance.Sender != null ? __instance.Sender.entityId : -1;
			if (playerId < 0)
			{
				return true;
			}

			return RunStats.AllowPackage(playerId);
		}
	}

	[HarmonyPatch(typeof(GameManager), nameof(GameManager.PlayerDisconnected))]
	internal static class Patch_ParkLevelOnDisconnect
	{
		private static void Prefix(ClientInfo _cInfo)
		{
			RunStats.Park(_cInfo);
		}
	}
}
