using System.Collections.Generic;
using UnityEngine;

namespace SortingCart
{
	internal static class NearbyScanner
	{
		private const int YMin = 0;
		private const int YMax = 253;

		public static bool TryGetBounds(Vector3i origin, out Vector3i min, out Vector3i max, out List<Vector3i> containingClaims)
		{
			min = max = default;
			containingClaims = new List<Vector3i>();
			World world = GameManager.Instance?.World;
			if (world == null || !world.GetWorldExtent(out Vector3i worldMin, out Vector3i worldMax))
			{
				return false;
			}

			int radius = GetLandClaimRadius();
			CollectContainingClaims(origin, radius, containingClaims);
			if (containingClaims.Count == 0)
			{
				min.x = origin.x - radius;
				max.x = origin.x + radius;
				min.z = origin.z - radius;
				max.z = origin.z + radius;
			}
			else
			{
				min.x = int.MaxValue;
				max.x = int.MinValue;
				min.z = int.MaxValue;
				max.z = int.MinValue;
				foreach (Vector3i lcb in containingClaims)
				{
					min.x = Mathf.Min(min.x, lcb.x - radius);
					max.x = Mathf.Max(max.x, lcb.x + radius);
					min.z = Mathf.Min(min.z, lcb.z - radius);
					max.z = Mathf.Max(max.z, lcb.z + radius);
				}
			}

			min.y = YMin;
			max.y = YMax;
			min.x = Mathf.Max(min.x, worldMin.x);
			max.x = Mathf.Min(max.x, worldMax.x);
			min.y = Mathf.Max(min.y, worldMin.y, YMin);
			max.y = Mathf.Min(max.y, worldMax.y, YMax);
			min.z = Mathf.Max(min.z, worldMin.z);
			max.z = Mathf.Min(max.z, worldMax.z);
			return min.x <= max.x && min.y <= max.y && min.z <= max.z;
		}

		public static List<ChestTarget> FindPlayerChests(World world, Vector3i origin, Vector3i min, Vector3i max, List<Vector3i> containingClaims)
		{
			var results = new List<ChestTarget>();
			int radius = GetLandClaimRadius();
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

						if (containingClaims != null && containingClaims.Count > 0 && !InAnyClaim(pos, containingClaims, radius))
						{
							continue;
						}

						if (!StorageUtil.TryAsContainer(te, out ITileEntityLootable storage)
							|| !StorageUtil.IsPlayerStorage(storage)
							|| StorageUtil.IsSortingCart(te.blockValue.Block)
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

		private static int GetLandClaimRadius()
		{
			int size = GameStats.GetInt(EnumGameStats.LandClaimSize);
			if (size < 1)
			{
				size = GamePrefs.GetInt(EnumGamePrefs.LandClaimSize);
			}

			if (size < 1)
			{
				size = 1;
			}

			int radius = (size % 2 == 1 ? size - 1 : size) / 2;
			return radius < 1 ? 1 : radius;
		}

		private static void CollectContainingClaims(Vector3i sourcePos, int radius, List<Vector3i> claims)
		{
			PersistentPlayerList players = GameManager.Instance?.persistentPlayers;
			World world = GameManager.Instance?.World;
			if (players?.Players == null || world == null)
			{
				return;
			}

			foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> kvp in players.Players)
			{
				if (kvp.Value == null || !world.IsLandProtectionValidForPlayer(kvp.Value))
				{
					continue;
				}

				var blocks = kvp.Value.GetLandProtectionBlocks();
				if (blocks == null)
				{
					continue;
				}

				foreach (Vector3i pos in blocks)
				{
					if (InClaim(sourcePos, pos, radius))
					{
						claims.Add(pos);
					}
				}
			}
		}

		private static bool InAnyClaim(Vector3i pos, List<Vector3i> claims, int radius)
		{
			foreach (Vector3i lcb in claims)
			{
				if (InClaim(pos, lcb, radius))
				{
					return true;
				}
			}

			return false;
		}

		private static bool InClaim(Vector3i pos, Vector3i lcb, int radius)
		{
			return pos.x >= lcb.x - radius
				&& pos.x <= lcb.x + radius
				&& pos.z >= lcb.z - radius
				&& pos.z <= lcb.z + radius;
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
