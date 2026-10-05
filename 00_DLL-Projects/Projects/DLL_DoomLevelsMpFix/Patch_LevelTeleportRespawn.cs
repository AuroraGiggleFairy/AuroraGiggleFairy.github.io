using System.Collections;
using DoomLevels;
using HarmonyLib;
using UnityEngine;

namespace DoomLevelsMpFix
{
	/// <summary>
	/// A teleport respawn sets Y to -1 and searches for ground. Into a level, and back out to
	/// the main world, Doom already has the destination. Finish at that position.
	/// Any other teleport keeps the vanilla ground search.
	/// </summary>
	[HarmonyPatch(typeof(PlayerMoveController), "updateRespawn")]
	internal static class Patch_LevelTeleportRespawn
	{
		private static bool Prefix(PlayerMoveController __instance)
		{
			if (__instance == null || __instance.respawnReason != RespawnType.Teleport)
			{
				return true;
			}

			EntityPlayerLocal player = Traverse.Create(__instance).Field("entityPlayerLocal").GetValue<EntityPlayerLocal>();
			if (player != null && !player.Spawned)
			{
				Patch_PlayerRigTargets.Apply(player);
			}

			bool inLevel = player != null && Instances.IsInstanceSpace(player.position);
			bool returning = ReturnTeleport.Pending;
			if (player == null || player.Spawned || (!inLevel && !returning))
			{
				return true;
			}

			bool holdGround = returning && !inLevel;
			ReturnTeleport.Clear();

			Vector3 at = player.position;
			player.onGround = true;
			player.bDead = false;
			player.Spawned = true;
			player.OnTeleportRespawnCompleted();
			player.ResetLastTickPos(at);
			player.AfterPlayerRespawn(RespawnType.Teleport);
			player.EnableCamera(true);
			player.SetControllable(true);

			var block = new Vector3i(at);
			GameManager.Instance.PlayerSpawnedInWorld(null, RespawnType.Teleport, block, player.entityId);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToClientsOrServer(
				NetPackageManager.GetPackage<NetPackagePlayerSpawnedInWorld>()
					.Setup(RespawnType.Teleport, block, player.entityId));

			World world = GameManager.Instance.World;
			if (world != null)
			{
				world.RefreshEntitiesOnMap();
			}

			if (LocalPlayerUI.primaryUI != null)
			{
				LocalPlayerUI.primaryUI.windowManager.Close(XUiC_LoadingScreen.ID);
			}

			var holding = AccessTools.Method(typeof(PlayerMoveController), "initializeHoldingItemLater");
			if (holding != null)
			{
				var wait = holding.Invoke(__instance, new object[] { 0.1f }) as IEnumerator;
				if (wait != null)
				{
					GameManager.Instance.StartCoroutine(wait);
				}
			}

			Traverse.Create(__instance).Field("bLastRespawnActive").SetValue(false);
			if (holdGround)
			{
				ReturnTeleport.BeginHold(player, at);
			}

			return false;
		}
	}

	internal static class ReturnTeleport
	{
		private const int GroundMask = 1342242816;

		private static bool _pending;
		private static bool _hold;
		private static Vector3 _at;
		private static float _until;

		internal static bool Pending
		{
			get { return _pending; }
		}

		internal static bool SuppressOrigin
		{
			get { return _pending || _hold; }
		}

		internal static void Mark()
		{
			_pending = true;
		}

		internal static void Clear()
		{
			_pending = false;
		}

		internal static void BeginHold(EntityPlayerLocal player, Vector3 at)
		{
			_hold = true;
			_at = at;
			_until = Time.time + 5f;
			Stay(player, at);
		}

		internal static void Tick()
		{
			if (!_hold)
			{
				return;
			}

			EntityPlayerLocal player = GameManager.Instance?.World?.GetPrimaryPlayer();
			if (player == null || Time.time > _until)
			{
				_hold = false;
				return;
			}

			Vector3 at = _at;
			Ray ray = new Ray(at + Vector3.up - Origin.position, Vector3.down);
			if (Physics.Raycast(ray, out RaycastHit hit, 3f, GroundMask))
			{
				float skin = 0.05f;
				if (player.m_characterController != null)
				{
					skin = player.m_characterController.GetSkinWidth();
				}

				at.y = hit.point.y + Origin.position.y + skin;
				Stay(player, at);
				_hold = false;
				return;
			}

			Stay(player, _at);
		}

		private static void Stay(EntityPlayerLocal player, Vector3 at)
		{
			if (player == null)
			{
				return;
			}

			player.SetPosition(at);
			player.onGround = true;
			player.motion = Vector3.zero;
		}
	}

	/// <summary>
	/// Set only when this local player is leaving a Doom level for the main world.
	/// The respawn then finishes at the destination instead of searching for ground.
	/// </summary>
	[HarmonyPatch(typeof(EntityPlayerLocal), nameof(EntityPlayerLocal.TeleportToPosition))]
	internal static class Patch_ReturnTeleportMark
	{
		private static void Prefix(EntityPlayerLocal __instance, Vector3 _pos)
		{
			if (__instance == null)
			{
				return;
			}

			if (Instances.IsInstanceSpace(__instance.position) && !Instances.IsInstanceSpace(_pos))
			{
				ReturnTeleport.Mark();
			}
		}
	}
}
