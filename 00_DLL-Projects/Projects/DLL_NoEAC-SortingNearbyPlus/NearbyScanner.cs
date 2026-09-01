using System.Collections.Generic;
using UnityEngine;

namespace SortingNearbyPlus
{
	internal static class NearbyScanner
	{
		private const int YMin = 0;
		private const int YMax = 253;

		private static int _landClaimRadius;

		public static void RefreshLandClaimRadius()
		{
			int size = GameStats.GetInt(EnumGameStats.LandClaimSize);
			_landClaimRadius = (size % 2 == 1 ? size - 1 : size) / 2;
		}

		public static bool TryGetBounds(Vector3i origin, out Vector3i min, out Vector3i max)
		{
			min = max = default;
			World world = GameManager.Instance?.World;
			if (world == null || !world.GetWorldExtent(out Vector3i worldMin, out Vector3i worldMax))
			{
				return false;
			}

			int h = SettingsManager.HorizontalRange;
			int v = SettingsManager.VerticalRange;
			min.x = origin.x - h;
			max.x = origin.x + h;
			min.z = origin.z - h;
			max.z = origin.z + h;
			if (v == -1)
			{
				min.y = YMin;
				max.y = YMax;
			}
			else
			{
				min.y = origin.y - v;
				max.y = origin.y + v;
			}

			if (SettingsManager.LandClaimClamp && TryGetLandClaim(origin, out Vector3i lcb))
			{
				min.x = Mathf.Max(min.x, lcb.x - _landClaimRadius);
				max.x = Mathf.Min(max.x, lcb.x + _landClaimRadius);
				min.z = Mathf.Max(min.z, lcb.z - _landClaimRadius);
				max.z = Mathf.Min(max.z, lcb.z + _landClaimRadius);
			}

			min.x = Mathf.Max(min.x, worldMin.x);
			max.x = Mathf.Min(max.x, worldMax.x);
			min.y = Mathf.Max(min.y, worldMin.y, YMin);
			max.y = Mathf.Min(max.y, worldMax.y, YMax);
			min.z = Mathf.Max(min.z, worldMin.z);
			max.z = Mathf.Min(max.z, worldMax.z);
			return min.x <= max.x && min.y <= max.y && min.z <= max.z;
		}

		public static List<ChestTarget> FindPlayerChests(World world, Vector3i origin, Vector3i min, Vector3i max)
		{
			var results = new List<ChestTarget>();
			int minCx = min.x >> 4;
			int maxCx = max.x >> 4;
			int minCz = min.z >> 4;
			int maxCz = max.z >> 4;
			for (int cx = minCx; cx <= maxCx; cx++)
			{
				for (int cz = minCz; cz <= maxCz; cz++)
				{
					if (!(world.GetChunkSync(cx, cz) is Chunk chunk))
					{
						continue;
					}

					DictionaryList<Vector3i, TileEntity> tes = chunk.GetTileEntities();
					if (tes?.dict == null)
					{
						continue;
					}

					foreach (KeyValuePair<Vector3i, TileEntity> kvp in tes.dict)
					{
						TileEntity te = kvp.Value;
						if (te == null)
						{
							continue;
						}

						Vector3i pos = te.ToWorldPos();
						if (!InBounds(pos, min, max))
						{
							continue;
						}

						if (!StorageUtil.TryAsContainer(te, out ITileEntityLootable storage)
							|| !StorageUtil.IsPlayerStorage(storage)
							|| StorageUtil.IsSortingBox(te.blockValue.Block)
							|| StorageUtil.IsInUse(te))
						{
							continue;
						}

						results.Add(new ChestTarget
						{
							TileEntity = te,
							Storage = storage,
							Pos = pos,
							Distance = Chebyshev(origin, pos)
						});
					}
				}
			}

			results.Sort((a, b) => a.Distance.CompareTo(b.Distance));
			return results;
		}

		public static List<EntityVehicle> FindVehicles(World world, Vector3 origin, Vector3i min, Vector3i max)
		{
			var results = new List<EntityVehicle>();
			foreach (Entity entity in world.Entities.list)
			{
				if (!(entity is EntityVehicle vehicle) || vehicle.IsDead())
				{
					continue;
				}

				Vector3i pos = new Vector3i(vehicle.position);
				if (!InBounds(pos, min, max))
				{
					continue;
				}

				results.Add(vehicle);
			}

			results.Sort((a, b) => Vector3.Distance(origin, a.position).CompareTo(Vector3.Distance(origin, b.position)));
			return results;
		}

		public static bool InBounds(Vector3i pos, Vector3i min, Vector3i max)
		{
			return pos.x >= min.x && pos.x <= max.x
				&& pos.y >= min.y && pos.y <= max.y
				&& pos.z >= min.z && pos.z <= max.z;
		}

		private static int Chebyshev(Vector3i a, Vector3i b)
		{
			int dx = a.x > b.x ? a.x - b.x : b.x - a.x;
			int dy = a.y > b.y ? a.y - b.y : b.y - a.y;
			int dz = a.z > b.z ? a.z - b.z : b.z - a.z;
			int m = dx > dy ? dx : dy;
			return m > dz ? m : dz;
		}

		private static bool TryGetLandClaim(Vector3i sourcePos, out Vector3i lcbPos)
		{
			World world = GameManager.Instance.World;
			foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> kvp in GameManager.Instance.persistentPlayers.Players)
			{
				if (!world.IsLandProtectionValidForPlayer(kvp.Value))
				{
					continue;
				}

				foreach (Vector3i pos in kvp.Value.GetLandProtectionBlocks())
				{
					if (sourcePos.x >= pos.x - _landClaimRadius
						&& sourcePos.x <= pos.x + _landClaimRadius
						&& sourcePos.z >= pos.z - _landClaimRadius
						&& sourcePos.z <= pos.z + _landClaimRadius)
					{
						lcbPos = pos;
						return true;
					}
				}
			}

			lcbPos = default;
			return false;
		}

		internal struct ChestTarget
		{
			public TileEntity TileEntity;
			public ITileEntityLootable Storage;
			public Vector3i Pos;
			public int Distance;
		}
	}
}
