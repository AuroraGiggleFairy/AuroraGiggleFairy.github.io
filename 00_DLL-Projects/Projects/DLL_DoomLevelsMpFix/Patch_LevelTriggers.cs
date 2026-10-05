using System.Collections.Generic;
using System.Reflection;
using Audio;
using DoomLevels;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Scripting;

namespace DoomLevelsMpFix
{
	/// <summary>
	/// Dedicated never gets OnTriggerEnter for remote players. Walk door lines and
	/// secret volumes against players already in a level, including the step they
	/// just moved so a thin line is not skipped between checks.
	/// </summary>
	internal static class LevelTriggerScan
	{
		private const float TickSeconds = 0.05f;
		private const float NearSq = 32f * 32f;

		private static float _next;
		private static readonly List<EntityPlayer> Players = new List<EntityPlayer>(8);
		private static readonly List<int> WasIds = new List<int>(8);
		private static readonly Dictionary<int, HashSet<int>> Inside = new Dictionary<int, HashSet<int>>();
		private static readonly Dictionary<int, Vector3> LastPos = new Dictionary<int, Vector3>();

		internal static void Tick()
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance == null ||
				SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
			{
				return;
			}

			if (Time.unscaledTime < _next)
			{
				return;
			}

			_next = Time.unscaledTime + TickSeconds;

			World world = GameManager.Instance?.World;
			if (world?.Players?.list == null)
			{
				return;
			}

			Players.Clear();
			List<EntityPlayer> list = world.Players.list;
			for (int i = 0; i < list.Count; i++)
			{
				EntityPlayer player = list[i];
				if (player == null || player.IsDead() || !Instances.IsInstanceSpace(player.position))
				{
					continue;
				}

				Players.Add(player);
			}

			if (Players.Count == 0)
			{
				Inside.Clear();
				LastPos.Clear();
				return;
			}

			HoldIfFell();
			ScanTriggers();
			ScanSecrets();
			for (int i = 0; i < Players.Count; i++)
			{
				LastPos[Players[i].entityId] = Players[i].position;
			}
		}

		private static void HoldIfFell()
		{
			for (int i = 0; i < Players.Count; i++)
			{
				EntityPlayer player = Players[i];
				Vector3i origin;
				Level level;
				int cell;
				if (!Instances.TryGet(player.entityId, out origin, out level, out cell) || level == null)
				{
					continue;
				}

				if (player.position.y >= origin.y)
				{
					continue;
				}

				float shell = LevelDb.Shell;
				Vector3 start = new Vector3(
					origin.x + shell + level.Start.x + level.StartOffX,
					origin.y + shell + level.Start.y + 0.35f,
					origin.z + shell + level.StartOffZ + level.Start.z);
				player.Buffs.AddBuff("doomBuffTeleport", -1, true, false, -1f);
				Instances.Teleport(player, start, null);
			}
		}

		private static void ScanTriggers()
		{
			List<Trigger> triggers = Trigger.Active;
			if (triggers == null)
			{
				return;
			}

			for (int t = triggers.Count - 1; t >= 0; t--)
			{
				Trigger trigger = triggers[t];
				if (trigger == null)
				{
					continue;
				}

				int id = trigger.GetInstanceID();
				if (trigger.Spent)
				{
					Inside.Remove(id);
					continue;
				}

				HashSet<int> set;
				if (!Inside.TryGetValue(id, out set))
				{
					set = new HashSet<int>();
					Inside[id] = set;
				}

				WasIds.Clear();
				foreach (int old in set)
				{
					WasIds.Add(old);
				}

				set.Clear();
				Vector3 at = WorldPos(trigger);

				for (int p = 0; p < Players.Count; p++)
				{
					EntityPlayer player = Players[p];
					if ((player.position - at).sqrMagnitude > NearSq || !Hits(trigger, player))
					{
						continue;
					}

					set.Add(player.entityId);
					if (WasIds.Contains(player.entityId))
					{
						continue;
					}

					Collider body = player.GetComponentInChildren<Collider>();
					if (body != null)
					{
						trigger.Entered(body);
					}
				}

				for (int w = 0; w < WasIds.Count; w++)
				{
					int playerId = WasIds[w];
					if (set.Contains(playerId))
					{
						continue;
					}

					EntityPlayer player = Find(playerId);
					Collider body = player != null ? player.GetComponentInChildren<Collider>() : null;
					if (body != null)
					{
						trigger.Left(body);
					}
				}

				if (set.Count == 0)
				{
					Inside.Remove(id);
				}
			}
		}

		private static void ScanSecrets()
		{
			List<Secret> secrets = Secret.Active;
			if (secrets == null)
			{
				return;
			}

			for (int s = secrets.Count - 1; s >= 0; s--)
			{
				Secret secret = secrets[s];
				if (secret == null || secret.Taken)
				{
					continue;
				}

				Vector3 at = WorldPos(secret);
				for (int p = 0; p < Players.Count; p++)
				{
					EntityPlayer player = Players[p];
					if ((player.position - at).sqrMagnitude > NearSq || !Hits(secret, player))
					{
						continue;
					}

					Collider body = player.GetComponentInChildren<Collider>();
					if (body != null)
					{
						secret.Touched(body);
					}
				}
			}
		}

		private static Vector3 WorldPos(MonoBehaviour volume)
		{
			return volume.transform.position + Origin.position;
		}

		private static bool Hits(MonoBehaviour volume, EntityPlayer player)
		{
			Collider[] cols = volume.GetComponentsInChildren<Collider>(true);
			if (cols == null || cols.Length == 0)
			{
				return false;
			}

			Bounds body = player.boundingBox;
			Vector3 previous = body.center;
			Vector3 last;
			if (LastPos.TryGetValue(player.entityId, out last))
			{
				previous = body.center - (player.position - last);
			}

			for (int i = 0; i < cols.Length; i++)
			{
				Collider col = cols[i];
				if (col == null || !col.enabled || !col.isTrigger)
				{
					continue;
				}

				Bounds box = col.bounds;
				box.center += Origin.position;
				for (int s = 0; s <= 3; s++)
				{
					Bounds sample = body;
					sample.center = Vector3.Lerp(previous, body.center, s / 3f);
					if (box.Intersects(sample))
					{
						return true;
					}
				}
			}

			return false;
		}

		private static EntityPlayer Find(int entityId)
		{
			for (int i = 0; i < Players.Count; i++)
			{
				if (Players[i].entityId == entityId)
				{
					return Players[i];
				}
			}

			return null;
		}
	}

	[HarmonyPatch(typeof(Trigger), "CapsuleReaches")]
	internal static class Patch_TriggerCapsule
	{
		private static bool Prefix(ref bool __result)
		{
			if (!GameManager.IsDedicatedServer)
			{
				return true;
			}

			__result = true;
			return false;
		}
	}

	[HarmonyPatch(typeof(Secret), nameof(Secret.Touched))]
	internal static class Patch_SecretRemotePlayer
	{
		private static readonly FieldInfo FoundField = AccessTools.Field(typeof(Secret), "_found");
		private static readonly FieldInfo PosField = AccessTools.Field(typeof(Secret), "_blockPos");

		private static bool Prefix(Secret __instance, Collider _other)
		{
			if (_other == null || FoundField == null || PosField == null)
			{
				return true;
			}

			ConnectionManager net = SingletonMonoBehaviour<ConnectionManager>.Instance;
			if (net == null || net.IsClient)
			{
				return true;
			}

			if (_other.GetComponentInParent<EntityPlayerLocal>() != null)
			{
				return true;
			}

			EntityPlayer player = _other.GetComponentInParent<EntityPlayer>();
			if (player == null || player.IsDead())
			{
				return false;
			}

			if ((bool)FoundField.GetValue(__instance))
			{
				return false;
			}

			FoundField.SetValue(__instance, true);
			Vector3i pos = (Vector3i)PosField.GetValue(__instance);
			Stats stats = Instances.StatsFor(player.entityId);
			if (stats != null)
			{
				stats.Secrets++;
			}

			BlockState.Set(pos, BlockState.TriggerSpentBit, true);
			net.SendPackage(PackageEmit.Take<NetPackageDoomSecretFound>(), false, player.entityId);
			Debug.Log("[DoomMultiplayer] secret found by " + player.entityId + " at " + pos +
				(stats == null ? "" : " " + stats.Secrets + "/" + stats.TotalSecrets));

			Collider[] cols = __instance.GetComponentsInChildren<Collider>(true);
			for (int i = 0; i < cols.Length; i++)
			{
				if (cols[i] != null)
				{
					cols[i].enabled = false;
				}
			}

			return false;
		}
	}

	[Preserve]
	public abstract class NetPackageDoomSecretFound : NetPackage
	{
		public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

		public override void read(PooledBinaryReader _br)
		{
		}

		public override void write(PooledBinaryWriter _bw)
		{
			base.write(_bw);
		}

		public override void ProcessPackage(World _world, GameManager _callbacks)
		{
			if (_world == null)
			{
				return;
			}

			Manager.PlayInsidePlayerHead("doom_dssecret", -1, 0f, false, false);
			EntityPlayerLocal local = _world.GetPrimaryPlayer();
			if (local != null)
			{
				GameManager.ShowTooltip(local, Localization.Get("doom_secret_found"));
			}
		}

		public int Length()
		{
			return 1;
		}
	}
}
