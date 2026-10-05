using System.Collections.Generic;
using DoomLevels;
using HarmonyLib;
using UnityEngine;

namespace DoomLevelsMpFix
{
	/// <summary>
	/// One-shot triggers and finished one-way walls are remembered with the level.
	/// The in-level map is the set of walls already seen. Those line indexes are
	/// saved with the level and drawn again after a relog, even in a new cell.
	/// </summary>
	[HarmonyPatch(typeof(BlockState), nameof(BlockState.Set))]
	internal static class Patch_RememberLevelState
	{
		private static void Postfix(Vector3i pos, byte bit, bool on)
		{
			if (!on)
			{
				return;
			}

			World world = GameManager.Instance?.World;
			BlockValue block = world == null ? BlockValue.Air : world.GetBlock(pos);
			if (bit == BlockState.TriggerSpentBit)
			{
				if (block.Block != null && block.Block.GetType().Name == "BlockTrigger")
				{
					RunStats.NoteTrigger(pos);
				}

				return;
			}

			if (bit == BlockState.MoverMovedBit && block.Block is BlockMover mover && !mover.Returns)
			{
				RunStats.NoteMover(pos);
			}
		}
	}

	[HarmonyPatch(typeof(Trigger), nameof(Trigger.Configure))]
	internal static class Patch_RestoreTrigger
	{
		private static void Postfix(Trigger __instance, Vector3i blockPos)
		{
			if (__instance == null || !RunStats.TriggerCleared(blockPos))
			{
				return;
			}

			Traverse.Create(__instance).Field("_spent").SetValue(true);
			if (!BlockState.Get(blockPos, BlockState.TriggerSpentBit))
			{
				BlockState.Set(blockPos, BlockState.TriggerSpentBit, true);
			}
		}
	}

	[HarmonyPatch(typeof(Mover), nameof(Mover.Configure))]
	internal static class Patch_RestoreMover
	{
		private static void Postfix(Mover __instance, Vector3i blockPos, float travel, bool returns, bool perpetual)
		{
			if (__instance == null || returns || perpetual || !RunStats.MoverCleared(blockPos))
			{
				return;
			}

			try
			{
				Traverse mover = Traverse.Create(__instance);
				mover.Field("_offset").SetValue(travel);
				mover.Field("_goal").SetValue(travel);
				mover.Field("_spent").SetValue(true);
				mover.Method("Place", true).GetValue();
				mover.Method("SetCells", false).GetValue();
			}
			catch (System.Exception e)
			{
				Debug.LogWarning("[DoomMultiplayer] could not restore mover at " + blockPos + ": " + e.Message);
			}

			if (!BlockState.Get(blockPos, BlockState.MoverMovedBit))
			{
				BlockState.Set(blockPos, BlockState.MoverMovedBit, true);
			}
		}
	}

	internal static class MapExplore
	{
		private const float TickSeconds = 1f;

		private static float _next;
		private static string _map = "";
		private static readonly List<int> Found = new List<int>();
		private static readonly HashSet<int> Sent = new HashSet<int>();

		internal static void Reset()
		{
			_map = "";
			Sent.Clear();
		}

		internal static void Show(string map, List<int> lines)
		{
			if (!string.Equals(_map, map ?? "", System.StringComparison.Ordinal))
			{
				_map = map ?? "";
				Sent.Clear();
			}

			if (lines == null || lines.Count == 0)
			{
				return;
			}

			for (int i = 0; i < lines.Count; i++)
			{
				Sent.Add(lines[i]);
			}

			ApplyPending();
		}

		internal static void Tick()
		{
			ApplyPending();
			if (!MapTally.Active || Time.unscaledTime < _next)
			{
				return;
			}

			_next = Time.unscaledTime + TickSeconds;
			if (!string.Equals(_map, MapTally.Map ?? "", System.StringComparison.Ordinal))
			{
				_map = MapTally.Map ?? "";
				Sent.Clear();
			}

			bool[] seen = Automap.Seen;
			if (seen == null)
			{
				return;
			}

			EntityPlayerLocal player = GameManager.Instance?.World?.GetPrimaryPlayer();
			if (player == null)
			{
				return;
			}

			Found.Clear();
			for (int i = 0; i < seen.Length; i++)
			{
				if (seen[i] && Sent.Add(i))
				{
					Found.Add(i);
				}
			}

			if (Found.Count == 0)
			{
				return;
			}

			ConnectionManager net = SingletonMonoBehaviour<ConnectionManager>.Instance;
			if (net == null || net.IsServer)
			{
				RunStats.RememberLines(player.entityId, Found);
				return;
			}

			net.SendToServer(PackageEmit.Take<NetPackageDoomMapReport>().Setup(Found));
		}

		private static void ApplyPending()
		{
			if (!MapTally.Active || Sent.Count == 0)
			{
				return;
			}

			bool[] seen = Automap.Seen;
			if (seen == null || Sent.Count == 0)
			{
				return;
			}

			foreach (int index in Sent)
			{
				if (index >= 0 && index < seen.Length)
				{
					seen[index] = true;
				}
			}
		}
	}

	[UnityEngine.Scripting.Preserve]
	public abstract class NetPackageDoomMapReport : NetPackage
	{
		private readonly List<int> _lines = new List<int>();

		public override NetPackageDirection PackageDirection => NetPackageDirection.ToServer;

		public NetPackageDoomMapReport Setup(List<int> lines)
		{
			_lines.Clear();
			if (lines != null)
			{
				_lines.AddRange(lines);
			}

			return this;
		}

		public override void read(PooledBinaryReader _br)
		{
			_lines.Clear();
			int count = _br.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				_lines.Add(_br.ReadInt32());
			}
		}

		public override void write(PooledBinaryWriter _bw)
		{
			base.write(_bw);
			System.IO.BinaryWriter writer = (System.IO.BinaryWriter)_bw;
			writer.Write(_lines.Count);
			for (int i = 0; i < _lines.Count; i++)
			{
				writer.Write(_lines[i]);
			}
		}

		public override void ProcessPackage(World _world, GameManager _callbacks)
		{
			if (Sender == null)
			{
				return;
			}

			RunStats.RememberLines(Sender.entityId, _lines);
		}

		public int Length()
		{
			return 8 + _lines.Count * 4;
		}
	}

	[UnityEngine.Scripting.Preserve]
	public abstract class NetPackageDoomMapPaint : NetPackage
	{
		private string _map = "";
		private readonly List<int> _lines = new List<int>();

		public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

		public NetPackageDoomMapPaint Setup(string map, List<int> lines)
		{
			_map = map ?? "";
			_lines.Clear();
			if (lines != null)
			{
				_lines.AddRange(lines);
			}

			return this;
		}

		public override void read(PooledBinaryReader _br)
		{
			_map = _br.ReadString();
			_lines.Clear();
			int count = _br.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				_lines.Add(_br.ReadInt32());
			}
		}

		public override void write(PooledBinaryWriter _bw)
		{
			base.write(_bw);
			System.IO.BinaryWriter writer = (System.IO.BinaryWriter)_bw;
			writer.Write(_map ?? "");
			writer.Write(_lines.Count);
			for (int i = 0; i < _lines.Count; i++)
			{
				writer.Write(_lines[i]);
			}
		}

		public override void ProcessPackage(World _world, GameManager _callbacks)
		{
			MapExplore.Show(_map, _lines);
		}

		public int Length()
		{
			return 12 + (_map == null ? 0 : _map.Length) + _lines.Count * 4;
		}
	}
}
