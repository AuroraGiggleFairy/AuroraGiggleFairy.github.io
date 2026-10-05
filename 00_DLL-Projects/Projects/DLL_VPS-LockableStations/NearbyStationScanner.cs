using System.Collections.Generic;
using Platform;
using UnityEngine;

namespace LockableWorkstations
{
	internal static class NearbyStationScanner
	{
		private const int YMin = 0;
		private const int YMax = 253;

		public static int SetLockedNearby(World world, Vector3i origin, bool lockNow, out bool usedLandClaim)
		{
			usedLandClaim = false;
			if (world == null || !TryGetBounds(origin, out Vector3i min, out Vector3i max, out List<Vector3i> containingClaims))
				return 0;

			usedLandClaim = containingClaims != null && containingClaims.Count > 0;
			int radius = GetLandClaimRadius();
			var seen = new HashSet<string>();
			int count = 0;

			int minCx = min.x >> 4;
			int maxCx = max.x >> 4;
			int minCz = min.z >> 4;
			int maxCz = max.z >> 4;
			for (int cx = minCx; cx <= maxCx; cx++)
			{
				for (int cz = minCz; cz <= maxCz; cz++)
				{
					if (!(world.GetChunkSync(cx, cz) is Chunk chunk))
						continue;

					DictionaryList<Vector3i, TileEntity> tes = chunk.GetTileEntities();
					if (tes?.dict == null)
						continue;

					foreach (KeyValuePair<Vector3i, TileEntity> kvp in tes.dict)
					{
						TileEntity te = kvp.Value;
						if (te == null)
							continue;

						Vector3i pos = LockableWorkstationHelpers.ResolveParentIfChild(world, 0, te.ToWorldPos());
						if (!InBounds(pos, min, max))
							continue;

						if (usedLandClaim && !InAnyClaim(pos, containingClaims, radius))
							continue;

						string key = pos.x + "," + pos.y + "," + pos.z;
						if (!seen.Add(key))
							continue;

						if (!LockableWorkstationHelpers.TryGetAdapter(world, 0, pos, out TileEntity tileEntity, out TileEntityLockAdapter adapter))
							continue;

						adapter.SetLocked(lockNow);
						tileEntity.SetModified();
						count++;
					}
				}
			}

			return count;
		}

		private static bool TryGetBounds(Vector3i origin, out Vector3i min, out Vector3i max, out List<Vector3i> containingClaims)
		{
			min = max = default;
			containingClaims = new List<Vector3i>();
			World world = GameManager.Instance?.World;
			if (world == null || !world.GetWorldExtent(out Vector3i worldMin, out Vector3i worldMax))
				return false;

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

		private static int GetLandClaimRadius()
		{
			int size = GameStats.GetInt(EnumGameStats.LandClaimSize);
			if (size < 1)
				size = GamePrefs.GetInt(EnumGamePrefs.LandClaimSize);
			if (size < 1)
				size = 1;

			int radius = (size % 2 == 1 ? size - 1 : size) / 2;
			return radius < 1 ? 1 : radius;
		}

		private static void CollectContainingClaims(Vector3i sourcePos, int radius, List<Vector3i> claims)
		{
			PersistentPlayerList players = GameManager.Instance?.persistentPlayers;
			World world = GameManager.Instance?.World;
			if (players?.Players == null || world == null)
				return;

			foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> kvp in players.Players)
			{
				if (kvp.Value == null || !world.IsLandProtectionValidForPlayer(kvp.Value))
					continue;

				var blocks = kvp.Value.GetLandProtectionBlocks();
				if (blocks == null)
					continue;

				foreach (Vector3i pos in blocks)
				{
					if (InClaim(sourcePos, pos, radius))
						claims.Add(pos);
				}
			}
		}

		private static bool InAnyClaim(Vector3i pos, List<Vector3i> claims, int radius)
		{
			foreach (Vector3i lcb in claims)
			{
				if (InClaim(pos, lcb, radius))
					return true;
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

		private static bool InBounds(Vector3i pos, Vector3i min, Vector3i max)
		{
			return pos.x >= min.x && pos.x <= max.x
				&& pos.y >= min.y && pos.y <= max.y
				&& pos.z >= min.z && pos.z <= max.z;
		}
	}
}
