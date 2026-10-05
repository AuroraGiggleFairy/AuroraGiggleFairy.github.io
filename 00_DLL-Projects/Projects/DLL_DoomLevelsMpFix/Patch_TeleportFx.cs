using System.Collections.Generic;
using System.IO;
using System.Reflection;
using DoomLevels;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Scripting;

namespace DoomLevelsMpFix
{
	/// <summary>
	/// Teleporter fog is spawned where the crossing is detected. On a dedicated
	/// server that spawn is discarded. One player is in the level, so the burst
	/// is sent to that player. Singleplayer already spawns it inside Land.
	/// </summary>
	[HarmonyPatch(typeof(Trigger), "Land")]
	internal static class Patch_TeleportFxSend
	{
		private static readonly FieldInfo BlockPosField = AccessTools.Field(typeof(Trigger), "_blockPos");
		private static readonly FieldInfo DestinationField = AccessTools.Field(typeof(Trigger), "_destination");
		private static readonly FieldInfo AngleField = AccessTools.Field(typeof(Trigger), "_angle");

		private static bool _pending;
		private static Vector3 _from;
		private static Vector3 _arrival;

		private static void Prefix(Trigger __instance, EntityAlive entity, EntityPlayer player)
		{
			_pending = false;
			if (player == null || player is EntityPlayerLocal || entity == null)
			{
				return;
			}

			ConnectionManager net = SingletonMonoBehaviour<ConnectionManager>.Instance;
			if (net == null || net.IsClient || BlockPosField == null || DestinationField == null || AngleField == null)
			{
				return;
			}

			Vector3i blockPos = (Vector3i)BlockPosField.GetValue(__instance);
			Vector3 destination = (Vector3)DestinationField.GetValue(__instance);
			int angle = (int)AngleField.GetValue(__instance);
			var to = new Vector3(blockPos.x + destination.x, blockPos.y + destination.y + 0.1f, blockPos.z + destination.z);
			float yaw = angle * Mathf.Deg2Rad;
			var look = new Vector3(Mathf.Cos(yaw), 0f, Mathf.Sin(yaw));
			_from = entity.GetPosition();
			_arrival = to + look * (20f / 32f);
			_pending = true;
		}

		private static void Postfix(EntityPlayer player)
		{
			if (!_pending || player == null)
			{
				return;
			}

			_pending = false;
			ConnectionManager net = SingletonMonoBehaviour<ConnectionManager>.Instance;
			if (net == null)
			{
				return;
			}

			net.SendPackage(PackageEmit.Take<NetPackageDoomTeleportFx>().Setup(_from, _arrival), false, player.entityId);
		}
	}

	/// <summary>
	/// The client can also run Land when its own collider crosses the line.
	/// Whichever burst arrives first is shown. The second copy of the same spot is skipped.
	/// </summary>
	[HarmonyPatch(typeof(Effects), nameof(Effects.Teleport))]
	internal static class Patch_TeleportFxOnce
	{
		private const float WindowSeconds = 0.4f;
		private const float SameSpotSq = 1.5f * 1.5f;

		private static readonly List<Vector3> Spots = new List<Vector3>(4);
		private static readonly List<float> When = new List<float>(4);

		private static bool Prefix(Vector3 worldPos)
		{
			if (GameManager.IsDedicatedServer)
			{
				return true;
			}

			float now = Time.unscaledTime;
			for (int i = Spots.Count - 1; i >= 0; i--)
			{
				if (now - When[i] > WindowSeconds)
				{
					Spots.RemoveAt(i);
					When.RemoveAt(i);
					continue;
				}

				if ((Spots[i] - worldPos).sqrMagnitude < SameSpotSq)
				{
					return false;
				}
			}

			Spots.Add(worldPos);
			When.Add(now);
			return true;
		}
	}

	[Preserve]
	public abstract class NetPackageDoomTeleportFx : NetPackage
	{
		private float _fx;
		private float _fy;
		private float _fz;
		private float _ax;
		private float _ay;
		private float _az;

		public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

		internal NetPackageDoomTeleportFx Setup(Vector3 from, Vector3 arrival)
		{
			_fx = from.x;
			_fy = from.y;
			_fz = from.z;
			_ax = arrival.x;
			_ay = arrival.y;
			_az = arrival.z;
			return this;
		}

		public override void read(PooledBinaryReader _br)
		{
			_fx = _br.ReadSingle();
			_fy = _br.ReadSingle();
			_fz = _br.ReadSingle();
			_ax = _br.ReadSingle();
			_ay = _br.ReadSingle();
			_az = _br.ReadSingle();
		}

		public override void write(PooledBinaryWriter _bw)
		{
			base.write(_bw);
			BinaryWriter writer = (BinaryWriter)_bw;
			writer.Write(_fx);
			writer.Write(_fy);
			writer.Write(_fz);
			writer.Write(_ax);
			writer.Write(_ay);
			writer.Write(_az);
		}

		public override void ProcessPackage(World _world, GameManager _callbacks)
		{
			if (GameManager.IsDedicatedServer)
			{
				return;
			}

			Effects.Teleport(new Vector3(_fx, _fy, _fz));
			Effects.Teleport(new Vector3(_ax, _ay, _az));
		}

		public int Length()
		{
			return 24;
		}
	}
}
