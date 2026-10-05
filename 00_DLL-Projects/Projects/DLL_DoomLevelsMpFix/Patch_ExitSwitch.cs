using System.Reflection;
using DoomLevels;
using HarmonyLib;
using UnityEngine;

namespace DoomLevelsMpFix
{
	/// <summary>
	/// A switch clears its placed flag when its object disables. Logging out does that
	/// while the level stays loaded, and the next login does not run Configure again.
	/// Press then refuses on the server. The client sends the use and logs nothing.
	/// </summary>
	[HarmonyPatch(typeof(Switch), nameof(Switch.Press))]
	internal static class Patch_SwitchPressPlaced
	{
		private static FieldInfo _placed;
		private static FieldInfo _off;

		private static void Prefix(Switch __instance)
		{
			if (__instance == null)
			{
				return;
			}

			if (_placed == null)
			{
				_placed = AccessTools.Field(typeof(Switch), "_placed");
				_off = AccessTools.Field(typeof(Switch), "_off");
			}

			if (_placed == null || (bool)_placed.GetValue(__instance))
			{
				return;
			}

			string off = _off == null ? null : _off.GetValue(__instance) as string;
			if (string.IsNullOrEmpty(off))
			{
				return;
			}

			_placed.SetValue(__instance, true);
		}
	}

	[HarmonyPatch(typeof(BlockSwitch), nameof(BlockSwitch.OnBlockActivated))]
	internal static class Patch_ExitSwitchUse
	{
		private static bool Prefix(string _commandName, WorldBase _world, Vector3i _blockPos,
			BlockValue _blockValue, ref bool __result)
		{
			if (_commandName != "use" || !ExitSwitch.IsExit(_blockValue.Block))
			{
				return true;
			}

			if (ExitSwitch.Find(_world, _blockPos) != null)
			{
				return true;
			}

			ConnectionManager net = SingletonMonoBehaviour<ConnectionManager>.Instance;
			if (net == null || !net.IsClient)
			{
				return true;
			}

			net.SendToServer(PackageEmit.Take<NetPackageDoomActivate>().Setup(_blockPos));
			__result = true;
			return false;
		}
	}

	[HarmonyPatch(typeof(RemoteActivate), nameof(RemoteActivate.Run))]
	internal static class Patch_ExitSwitchServer
	{
		private static bool Prefix(Vector3i blockPos, BlockValue block, EntityPlayer player, ref bool __result)
		{
			if (player == null || !ExitSwitch.IsExit(block.Block))
			{
				return true;
			}

			World world = GameManager.Instance == null ? null : GameManager.Instance.World;
			if (world != null && ExitSwitch.Find(world, blockPos) != null)
			{
				return true;
			}

			Instances.Exit(player, ExitReason.Cleared);
			__result = true;
			return false;
		}
	}

	internal static class ExitSwitch
	{
		private static FieldInfo _exit;

		internal static Switch Find(WorldBase world, Vector3i pos)
		{
			Chunk chunk = world == null || world.ChunkCache == null
				? null
				: world.ChunkCache.GetChunkSync(World.toChunkXZ(pos.x), pos.y, World.toChunkXZ(pos.z)) as Chunk;
			BlockEntityData bed = chunk == null ? null : chunk.GetBlockEntity(pos);
			return bed == null || bed.transform == null ? null : bed.transform.GetComponent<Switch>();
		}

		internal static bool IsExit(Block block)
		{
			if (!(block is BlockSwitch))
			{
				return false;
			}

			if (_exit == null)
			{
				_exit = AccessTools.Field(typeof(BlockSwitch), "_exit");
			}

			return _exit != null && (bool)_exit.GetValue(block);
		}
	}
}
