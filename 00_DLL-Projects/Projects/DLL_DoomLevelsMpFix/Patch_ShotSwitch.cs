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
	/// Shot switches log the hit on the shooting client, then BlockShoot.Shot
	/// returns immediately because that machine is a client. The door only
	/// moves when the server runs Shot. Tell the server about the hit.
	/// Singleplayer never sends: IsClient is false and Shot already runs there.
	/// </summary>
	[HarmonyPatch(typeof(BlockShoot), nameof(BlockShoot.DamageBlock))]
	internal static class Patch_ShotSwitch
	{
		private static bool Prefix(BlockValue _blockValue, BlockValueRef _blockValueRef, ref int __result)
		{
			if (!ShotSwitchRelay.PackageJustFired(_blockValueRef.BlockPosition))
			{
				return true;
			}

			__result = _blockValue.damage;
			return false;
		}

		private static void Postfix(BlockValueRef _blockValueRef, int _damagePoints, ItemActionAttack.AttackHitInfo _attackHitInfo)
		{
			if (_damagePoints <= 0 || !ShotSwitchRelay.Hitscan(_attackHitInfo))
			{
				return;
			}

			ConnectionManager net = SingletonMonoBehaviour<ConnectionManager>.Instance;
			if (net == null)
			{
				return;
			}

			Vector3i pos = _blockValueRef.BlockPosition;
			if (!net.IsClient)
			{
				ShotSwitchRelay.NoteServer(pos);
				return;
			}

			if (!Instances.IsInstanceSpace(pos.ToVector3() + new Vector3(0.5f, 0.5f, 0.5f)))
			{
				return;
			}

			net.SendToServer(PackageEmit.Take<NetPackageDoomShotSwitch>().Setup(pos));
		}
	}

	internal static class ShotSwitchRelay
	{
		private const float SameHitSeconds = 0.05f;
		private const float MaxRangeSq = 160f * 160f;

		private static readonly Dictionary<Vector3i, float> ServerAt = new Dictionary<Vector3i, float>();
		private static readonly Dictionary<Vector3i, float> PackageAt = new Dictionary<Vector3i, float>();
		private static readonly MethodInfo ShotMethod = AccessTools.Method(typeof(BlockShoot), "Shot");

		internal static bool Hitscan(ItemActionAttack.AttackHitInfo info)
		{
			if (info == null || info.WeaponTypeTag.IsEmpty)
			{
				return false;
			}

			return info.WeaponTypeTag.Test_AnySet(ItemActionAttack.RangedTag) ||
				info.WeaponTypeTag.Test_AnySet(ItemActionAttack.MeleeTag);
		}

		internal static void NoteServer(Vector3i pos)
		{
			ServerAt[pos] = Time.unscaledTime;
			Trim(ServerAt);
		}

		internal static bool PackageJustFired(Vector3i pos)
		{
			float at;
			if (!PackageAt.TryGetValue(pos, out at))
			{
				return false;
			}

			return Time.unscaledTime - at < SameHitSeconds;
		}

		internal static void Run(World world, Vector3i pos, int senderId)
		{
			if (world == null || ShotMethod == null)
			{
				return;
			}

			if (!Instances.IsInstanceSpace(pos.ToVector3() + new Vector3(0.5f, 0.5f, 0.5f)))
			{
				return;
			}

			float serverAt;
			if (ServerAt.TryGetValue(pos, out serverAt) && Time.unscaledTime - serverAt < SameHitSeconds)
			{
				return;
			}

			EntityPlayer player = world.GetEntity(senderId) as EntityPlayer;
			if (player == null || player.IsDead())
			{
				return;
			}

			if ((player.position - (pos.ToVector3() + new Vector3(0.5f, 0.5f, 0.5f))).sqrMagnitude > MaxRangeSq)
			{
				return;
			}

			BlockValue value = world.GetBlock(pos);
			BlockShoot shoot = value.Block as BlockShoot;
			if (shoot == null)
			{
				return;
			}

			PackageAt[pos] = Time.unscaledTime;
			Trim(PackageAt);
			ShotMethod.Invoke(shoot, new object[] { world, pos });
			Log.Out("[DoomMultiplayer] shot switch at " + pos);
		}

		private static void Trim(Dictionary<Vector3i, float> map)
		{
			if (map.Count < 32)
			{
				return;
			}

			float now = Time.unscaledTime;
			List<Vector3i> drop = null;
			foreach (KeyValuePair<Vector3i, float> pair in map)
			{
				if (now - pair.Value < 1f)
				{
					continue;
				}

				if (drop == null)
				{
					drop = new List<Vector3i>();
				}

				drop.Add(pair.Key);
			}

			if (drop == null)
			{
				return;
			}

			for (int i = 0; i < drop.Count; i++)
			{
				map.Remove(drop[i]);
			}
		}
	}

	[Preserve]
	public abstract class NetPackageDoomShotSwitch : NetPackage
	{
		private int _x;
		private int _y;
		private int _z;

		public override NetPackageDirection PackageDirection => NetPackageDirection.ToServer;

		internal NetPackageDoomShotSwitch Setup(Vector3i pos)
		{
			_x = pos.x;
			_y = pos.y;
			_z = pos.z;
			return this;
		}

		public override void read(PooledBinaryReader _br)
		{
			_x = _br.ReadInt32();
			_y = _br.ReadInt32();
			_z = _br.ReadInt32();
		}

		public override void write(PooledBinaryWriter _bw)
		{
			base.write(_bw);
			((BinaryWriter)_bw).Write(_x);
			((BinaryWriter)_bw).Write(_y);
			((BinaryWriter)_bw).Write(_z);
		}

		public override void ProcessPackage(World _world, GameManager _callbacks)
		{
			if (Sender == null)
			{
				return;
			}

			ShotSwitchRelay.Run(_world, new Vector3i(_x, _y, _z), Sender.entityId);
		}

		public int Length()
		{
			return 12;
		}
	}
}
