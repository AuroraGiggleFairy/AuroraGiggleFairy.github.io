using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UniLinq;
using UnityEngine;

namespace WorldGenerationEngineFinal;

public class StreetTile
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum RoadShapeTypes
	{
		straight,
		t,
		intersection,
		cap,
		corner
	}

	public enum PrefabRotations
	{
		None,
		One,
		Two,
		Three
	}

	public const int TileSize = 150;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int TileSizeHalf = 75;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDensityRadius = 190f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDensityBase = 6f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDensityMid = 20f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDensityMidScale = 1.3f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDensityBudget = 62f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDensityRetry = 18f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cRadiationEdgeSize = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float maxHeightDiff = 10f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int partDepthLimit = 20;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int poiDepthLimit = 5;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSmoothFullRadius = 1.8f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSmoothFadeRadius = 3.2f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSmoothBoxRadius = 2.2f;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly WorldBuilder worldBuilder;

	public Township Township;

	public District District;

	public readonly List<Vector2i> UsedExitList = new List<Vector2i>();

	public int ConnectedExits;

	public readonly List<Path> ConnectedHighways = new List<Path>();

	public readonly List<PrefabDataInstance> StreetTilePrefabDatas = new List<PrefabDataInstance>();

	public readonly Vector2i GridPosition;

	public readonly Vector2i WorldPosition;

	public readonly Rect Area;

	public bool OverlapsRadiation;

	public bool OverlapsWater;

	public bool OverlapsBiomes;

	public bool HasSteepSlope;

	public bool AllIsWater;

	public bool HasTrader;

	public bool HasFeature;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i WildernessPOICenter;

	[PublicizedFrom(EAccessModifier.Private)]
	public int WildernessPOISize;

	[PublicizedFrom(EAccessModifier.Private)]
	public int WildernessPOIHeight;

	[PublicizedFrom(EAccessModifier.Private)]
	public int RoadShape;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool smoothAround;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Bounds> partBounds = new List<Bounds>();

	public bool Used;

	[PublicizedFrom(EAccessModifier.Private)]
	public PrefabRotations rotations;

	[PublicizedFrom(EAccessModifier.Private)]
	public TranslationData transData;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Vector2i> highwayExits = new List<Vector2i>();

	[PublicizedFrom(EAccessModifier.Private)]
	public StreetTile[] neighbors;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isFullyBlocked;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isPartBlocked;

	public int GroupID
	{
		get
		{
			if (Township == null)
			{
				return -1;
			}
			return Township.ID;
		}
	}

	public bool IsValidForStreetTile
	{
		get
		{
			if (!OverlapsBiomes && !OverlapsWater && !OverlapsRadiation && !HasSteepSlope)
			{
				return TerrainType != TerrainType.mountains;
			}
			return false;
		}
	}

	public bool IsValidForGateway
	{
		get
		{
			if (!OverlapsBiomes && !OverlapsWater && !OverlapsRadiation && !HasSteepSlope && TerrainType != TerrainType.mountains)
			{
				return !HasPrefabs;
			}
			return false;
		}
	}

	public bool IsBlocked
	{
		get
		{
			if (!AllIsWater && !OverlapsWater && !OverlapsRadiation && !HasSteepSlope)
			{
				return TerrainType == TerrainType.mountains;
			}
			return true;
		}
	}

	public bool HasPrefabs
	{
		get
		{
			if (StreetTilePrefabDatas != null)
			{
				return StreetTilePrefabDatas.Count > 0;
			}
			return false;
		}
	}

	public bool HasStreetTilePrefab
	{
		get
		{
			if (Township != null && District != null)
			{
				return HasPrefabs;
			}
			return false;
		}
	}

	public Vector2i WorldPositionCenter
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return WorldPosition + new Vector2i(75, 75);
		}
	}

	public Vector2i WorldPositionMax
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return WorldPosition + new Vector2i(150, 150);
		}
	}

	public BiomeType BiomeType => worldBuilder.GetBiome(WorldPositionCenter);

	public float PositionHeight => worldBuilder.GetHeight(WorldPositionCenter);

	public TerrainType TerrainType => worldBuilder.GetTerrainType(WorldPositionCenter);

	public int RoadExitCount
	{
		get
		{
			int num = 0;
			for (int i = 0; i < RoadExits.Length; i++)
			{
				if (RoadExits[i])
				{
					num++;
				}
			}
			return num;
		}
	}

	public bool[] RoadExits => worldBuilder.StreetTileShared.RoadShapeExitsPerRotation[RoadShape][(int)Rotations];

	public string PrefabName => worldBuilder.StreetTileShared.RoadShapesDistrict[RoadShape];

	public PrefabRotations Rotations
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return rotations;
		}
		set
		{
			if (value != rotations || transData == null)
			{
				rotations = value;
				if (transData != null)
				{
					transData.rotation = (int)rotations * -90;
				}
				else
				{
					transData = new TranslationData(WorldPositionCenter.x, WorldPositionCenter.y, 1f, (int)rotations * -90);
				}
			}
		}
	}

	public bool ContainsHighway => ConnectedHighways.Count > 0;

	public bool NeedsWildernessSmoothing => WildernessPOISize > 0;

	public Vector2i getHighwayExitPositionByDirection(Vector2i dir)
	{
		for (int i = 0; i < 4; i++)
		{
			if (dir == worldBuilder.StreetTileShared.dir4way[i])
			{
				return getHighwayExitPosition(i);
			}
		}
		return Vector2i.zero;
	}

	public Vector2i getHighwayExitPosition(int index)
	{
		if (highwayExits.Count == 0)
		{
			getAllHighwayExits();
		}
		return highwayExits[index];
	}

	public List<Vector2i> getAllHighwayExits()
	{
		if (highwayExits.Count == 0)
		{
			highwayExits.Add(highwayExitFromIndex(0));
			highwayExits.Add(highwayExitFromIndex(1));
			highwayExits.Add(highwayExitFromIndex(2));
			highwayExits.Add(highwayExitFromIndex(3));
		}
		return highwayExits;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i highwayExitFromIndex(int index)
	{
		Vector2i result = default(Vector2i);
		switch (index)
		{
		case 0:
			result.x = WorldPositionCenter.x;
			result.y = WorldPositionMax.y - 1;
			return result;
		case 1:
			result.x = WorldPositionMax.x - 1;
			result.y = WorldPositionCenter.y;
			return result;
		case 2:
			result.x = WorldPositionCenter.x;
			result.y = WorldPosition.y;
			return result;
		default:
			result.x = WorldPosition.x;
			result.y = WorldPositionCenter.y;
			return result;
		}
	}

	public void SetAllExistingNeighborsForGateway()
	{
		for (int i = 0; i < 4; i++)
		{
			if (Township.Streets.ContainsKey(GridPosition + worldBuilder.StreetTileShared.dir4way[i]))
			{
				SetExitUsed(getHighwayExitPosition(i));
			}
		}
	}

	public List<Vector2i> GetHighwayExits(bool isGateway = false)
	{
		List<Vector2i> list = new List<Vector2i>();
		if (!isGateway)
		{
			for (int i = 0; i < 4; i++)
			{
				if (Township.Streets.ContainsKey(GridPosition + worldBuilder.StreetTileShared.dir4way[i]))
				{
					UsedExitList.Add(getHighwayExitPosition(i));
					ConnectedExits |= 1 << i;
				}
			}
		}
		if (UsedExitList.Count == 1)
		{
			int num = -1;
			for (int j = 0; j < 4; j++)
			{
				if ((ConnectedExits & (1 << j)) > 0)
				{
					num = (j + 2) & 3;
					break;
				}
			}
			if (num != -1)
			{
				list.Add(getHighwayExitPosition(num));
			}
			else
			{
				Log.Error("Could not find opposite highway exit!");
			}
		}
		else
		{
			for (int k = 0; k < 4; k++)
			{
				if ((ConnectedExits & (1 << k)) <= 0 && (isGateway || RoadExits[k]))
				{
					list.Add(getHighwayExitPosition(k));
				}
			}
		}
		return list;
	}

	public List<Vector2i> GetAllHighwayExits()
	{
		List<Vector2i> list = new List<Vector2i>();
		for (int i = 0; i < RoadExits.Length; i++)
		{
			list.Add(getHighwayExitPosition(i));
		}
		return list;
	}

	public bool HasExits()
	{
		for (int i = 0; i < RoadExits.Length; i++)
		{
			if (RoadExits[i])
			{
				return true;
			}
		}
		return false;
	}

	public int GetExistingExitCount()
	{
		int num = 0;
		for (int i = 0; i < RoadExits.Length; i++)
		{
			if (RoadExits[i])
			{
				num++;
			}
		}
		return num;
	}

	public StreetTile(WorldBuilder _worldBuilder, Vector2i gridPosition)
	{
		worldBuilder = _worldBuilder;
		GridPosition = gridPosition;
		WorldPosition = GridPosition * 150;
		Area = new Rect(new Vector2(WorldPosition.x, WorldPosition.y), Vector2.one * 150f);
		GameRandom gameRandom = GameRandomManager.Instance.CreateGameRandom(worldBuilder.Seed + WorldPosition.ToString().GetHashCode());
		Rotations = (PrefabRotations)((gameRandom.RandomRange(0, 4) + 1) & 3);
		GameRandomManager.Instance.FreeGameRandom(gameRandom);
		RoadShape = 2;
		if (GridPosition.x < 1 || GridPosition.x >= worldBuilder.StreetTileMapSize - 1)
		{
			OverlapsRadiation = true;
		}
		if (GridPosition.y < 1 || GridPosition.y >= worldBuilder.StreetTileMapSize - 1)
		{
			OverlapsRadiation = true;
		}
		float positionHeight = PositionHeight;
		for (int i = 0; i < worldBuilder.StreetTileShared.dir9way.Length; i++)
		{
			Vector2i vector2i = WorldPositionCenter + worldBuilder.StreetTileShared.dir9way[i] * 75;
			if (worldBuilder.GetRad(vector2i.x, vector2i.y) > 0)
			{
				OverlapsRadiation = true;
			}
			if (Utils.FastAbs(worldBuilder.GetHeight(vector2i.x, vector2i.y) - positionHeight) > 10f)
			{
				HasSteepSlope = true;
			}
		}
		BiomeType biomeType = BiomeType;
		int num = 0;
		int num2 = 0;
		Vector2i worldPositionMax = WorldPositionMax;
		for (int j = WorldPosition.y; j < worldPositionMax.y; j += 3)
		{
			for (int k = WorldPosition.x; k < worldPositionMax.x; k += 3)
			{
				num++;
				if (biomeType != worldBuilder.GetBiome(k, j))
				{
					OverlapsBiomes = true;
				}
				if (worldBuilder.GetWater(k, j) > 0)
				{
					num2++;
					OverlapsWater = true;
				}
			}
		}
		if ((float)num2 / (float)num > 0.9f)
		{
			AllIsWater = true;
		}
	}

	public void UpdateValidity()
	{
		float positionHeight = PositionHeight;
		Vector2i[] dir9way = worldBuilder.StreetTileShared.dir9way;
		foreach (Vector2i vector2i in dir9way)
		{
			Vector2i vector2i2 = WorldPositionCenter + vector2i * 75;
			if (worldBuilder.GetRad(vector2i2.x, vector2i2.y) > 0)
			{
				OverlapsRadiation = true;
			}
			if (Utils.FastAbs(worldBuilder.GetHeight(vector2i2.x, vector2i2.y) - positionHeight) > 10f)
			{
				HasSteepSlope = true;
			}
		}
		BiomeType biomeType = BiomeType;
		Vector2i worldPositionMax = WorldPositionMax;
		for (int j = WorldPosition.y; j < worldPositionMax.y; j += 3)
		{
			for (int k = WorldPosition.x; k < worldPositionMax.x; k += 3)
			{
				if (biomeType != worldBuilder.GetBiome(k, j))
				{
					OverlapsBiomes = true;
				}
				if (worldBuilder.GetWater(k, j) > 0)
				{
					OverlapsWater = true;
				}
			}
		}
	}

	public Stamp[] GetStamps()
	{
		return new Stamp[1]
		{
			new Stamp(worldBuilder, worldBuilder.StampManager.GetStamp(worldBuilder.StreetTileShared.RoadShapes[RoadShape]), transData, _isCustomColor: true, new Color(1f, 0f, 0f, 0f))
		};
	}

	public StreetTile[] GetNeighbors()
	{
		if (neighbors == null)
		{
			neighbors = new StreetTile[4];
			for (int i = 0; i < worldBuilder.StreetTileShared.dir4way.Length; i++)
			{
				neighbors[i] = GetNeighbor(worldBuilder.StreetTileShared.dir4way[i]);
			}
		}
		return neighbors;
	}

	public int GetNeighborCount()
	{
		int num = 0;
		for (int i = 0; i < worldBuilder.StreetTileShared.dir4way.Length; i++)
		{
			if (GetNeighbor(worldBuilder.StreetTileShared.dir4way[i]) != null)
			{
				num++;
			}
		}
		return num;
	}

	public StreetTile[] GetNeighbors8way()
	{
		if (neighbors == null)
		{
			neighbors = new StreetTile[8];
			for (int i = 0; i < worldBuilder.StreetTileShared.dir8way.Length; i++)
			{
				neighbors[i] = GetNeighbor(worldBuilder.StreetTileShared.dir8way[i]);
			}
		}
		return neighbors;
	}

	public StreetTile GetNeighbor(Vector2i direction)
	{
		return worldBuilder.GetStreetTileGrid(GridPosition + direction);
	}

	public bool HasNeighbor(StreetTile otherTile)
	{
		StreetTile[] array = GetNeighbors();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == otherTile)
			{
				return true;
			}
		}
		return false;
	}

	public StreetTile GetNeighborByIndex(int idx)
	{
		if (neighbors == null)
		{
			GetNeighbors();
		}
		if (idx < 0 || idx >= neighbors.Length)
		{
			return null;
		}
		return neighbors[idx];
	}

	public int GetNeighborIndex(StreetTile otherTile)
	{
		if (neighbors == null)
		{
			GetNeighbors();
		}
		for (int i = 0; i < neighbors.Length; i++)
		{
			if (neighbors[i] == otherTile)
			{
				return i;
			}
		}
		return -1;
	}

	public bool HasExitTo(StreetTile otherTile)
	{
		if (Township == null && District == null)
		{
			return false;
		}
		if (otherTile.Township == null && otherTile.District == null)
		{
			return false;
		}
		int neighborIndex;
		if ((neighborIndex = GetNeighborIndex(otherTile)) < 0)
		{
			return false;
		}
		if (neighborIndex >= RoadExits.Length)
		{
			return false;
		}
		return RoadExits[neighborIndex];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int vectorToRotation(Vector2i direction)
	{
		for (int i = 0; i < worldBuilder.StreetTileShared.dir4way.Length; i++)
		{
			if (worldBuilder.StreetTileShared.dir4way[i] == direction)
			{
				return i;
			}
		}
		return -1;
	}

	public void SetPathingConstraintsForTile(bool allBlocked = false)
	{
		if (Township != null && District != null)
		{
			worldBuilder.PathingUtils.AddFullyBlockedArea(Area);
			isFullyBlocked = true;
		}
		else if (allBlocked && !isFullyBlocked && !isPartBlocked)
		{
			worldBuilder.PathingUtils.AddFullyBlockedArea(Area);
			isFullyBlocked = true;
		}
		else if (!allBlocked)
		{
			if (isFullyBlocked)
			{
				worldBuilder.PathingUtils.RemoveFullyBlockedArea(Area);
			}
			worldBuilder.PathingUtils.AddMoveLimitArea(Area);
			isPartBlocked = true;
		}
	}

	public void SetRoadExit(int dir, bool value)
	{
		if ((uint)dir < RoadExits.Length)
		{
			bool[] array = (bool[])RoadExits.Clone();
			array[dir] = value;
			SetRoadExits(array);
		}
	}

	public void SetRoadExits(bool _north, bool _east, bool _south, bool _west)
	{
		SetRoadExits(new bool[4] { _north, _east, _south, _west });
		GetNeighbor(Vector2i.right)?.SetPathingConstraintsForTile(!_east);
		GetNeighbor(Vector2i.left)?.SetPathingConstraintsForTile(!_west);
		GetNeighbor(Vector2i.up)?.SetPathingConstraintsForTile(!_north);
		GetNeighbor(Vector2i.down)?.SetPathingConstraintsForTile(!_south);
	}

	public void SetRoadExits(bool[] _exits)
	{
		PrefabRotations prefabRotations = Rotations;
		int roadShape = RoadShape;
		for (int i = 0; i < worldBuilder.StreetTileShared.RoadShapeExitCounts.Count; i++)
		{
			RoadShape = i;
			for (int j = 0; j < 4; j++)
			{
				Rotations = (PrefabRotations)j;
				if (_exits.SequenceEqual(RoadExits))
				{
					return;
				}
			}
		}
		Rotations = prefabRotations;
		RoadShape = roadShape;
	}

	public bool SetExitUsed(Vector2i exit)
	{
		for (int i = 0; i < RoadExits.Length; i++)
		{
			Vector2i highwayExitPosition = getHighwayExitPosition(i);
			if (highwayExitPosition == exit)
			{
				SetRoadExit(i, value: true);
				ConnectedExits |= 1 << i;
				if (!UsedExitList.Contains(highwayExitPosition))
				{
					UsedExitList.Add(highwayExitPosition);
				}
				return true;
			}
		}
		return false;
	}

	public void SetExitUnUsed(Vector2i exit)
	{
		for (int i = 0; i < RoadExits.Length; i++)
		{
			Vector2i highwayExitPosition = getHighwayExitPosition(i);
			if (highwayExitPosition == exit)
			{
				SetRoadExit(i, value: false);
				ConnectedExits &= ~(1 << i);
				UsedExitList.Remove(highwayExitPosition);
				break;
			}
		}
	}

	public bool SpawnPrefabs()
	{
		if (District == null || District.name == "wilderness")
		{
			if (!ContainsHighway)
			{
				District = DistrictPlannerStatic.Districts["wilderness"];
				if (spawnWildernessPrefab())
				{
					return true;
				}
			}
			District = null;
			return false;
		}
		string streetPrefabName = string.Format(PrefabName, District.prefabName);
		spawnStreetTile(WorldPosition, streetPrefabName, (int)Rotations);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool spawnStreetTile(Vector2i tileMinPositionWorld, string streetPrefabName, int baseRotations)
	{
		bool useExactString = false;
		PrefabData streetTile = worldBuilder.PrefabManager.GetStreetTile(streetPrefabName, WorldPositionCenter, useExactString);
		if (streetTile == null && string.Format(PrefabName, "") != streetPrefabName)
		{
			streetTile = worldBuilder.PrefabManager.GetStreetTile(string.Format(PrefabName, ""), WorldPositionCenter, useExactString);
		}
		if (streetTile == null)
		{
			return false;
		}
		if (worldBuilder.TownshipShared.Height + streetTile.yOffset < 3)
		{
			return false;
		}
		int num = (baseRotations + streetTile.RotationsToNorth) & 3;
		switch (num)
		{
		case 1:
			num = 3;
			break;
		case 3:
			num = 1;
			break;
		}
		Vector3i position = new Vector3i(tileMinPositionWorld.x, worldBuilder.TownshipShared.Height + streetTile.yOffset + 1, tileMinPositionWorld.y) + worldBuilder.PrefabWorldOffset;
		if (worldBuilder.PrefabManager.StreetTilesUsed.TryGetValue(streetTile.Name, out var value))
		{
			worldBuilder.PrefabManager.StreetTilesUsed[streetTile.Name] = value + 1;
		}
		else
		{
			worldBuilder.PrefabManager.StreetTilesUsed.Add(streetTile.Name, 1);
		}
		float totalDensityLeft = 62f;
		if (PrefabManagerStatic.TileMaxDensityScore.TryGetValue(streetTile.Name, out var value2))
		{
			totalDensityLeft = value2;
		}
		AddPrefab(new PrefabDataInstance(worldBuilder.PrefabManager.PrefabInstanceId++, position, (byte)num, streetTile));
		SpawnMarkerPartsAndPrefabs(streetTile, new Vector3i(WorldPosition.x, worldBuilder.TownshipShared.Height + streetTile.yOffset + 1, WorldPosition.y), num, 0, totalDensityLeft);
		smoothAround = true;
		return true;
	}

	public void SmoothWildernessTerrain()
	{
		SmoothTerrainBox(WildernessPOICenter, WildernessPOISize, WildernessPOIHeight);
	}

	public void SmoothTerrainPost()
	{
		if (smoothAround || (Township != null && District != null))
		{
			SmoothTerrainBox(WorldPositionCenter, 150, worldBuilder.TownshipShared.Height);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SmoothTerrainBox(Vector2i _centerPos, int _size, int _height)
	{
		int num = _size / 2 + 2;
		int num2 = (int)((float)num * 2.2f);
		float num3 = num2;
		int num4 = _centerPos.x - num;
		int num5 = _centerPos.x + num;
		int num6 = _centerPos.y - num;
		int num7 = _centerPos.y + num;
		int num8 = Utils.FastMax(num4 - num2, 1);
		int num9 = Utils.FastMax(num6 - num2, 1);
		int num10 = Utils.FastMin(num5 + num2, worldBuilder.WorldSize);
		int num11 = Utils.FastMin(num7 + num2, worldBuilder.WorldSize);
		for (int i = num9; i < num11; i++)
		{
			bool flag = i >= num6 && i <= num7;
			for (int j = num8; j < num10; j++)
			{
				bool flag2 = j >= num4 && j <= num5;
				if (flag2 && flag)
				{
					worldBuilder.SetHeightTrusted(j, i, _height);
					continue;
				}
				int x = (flag2 ? j : ((j < _centerPos.x) ? num4 : num5));
				int y = (flag ? i : ((i < _centerPos.y) ? num6 : num7));
				float num12 = Mathf.Sqrt(distanceSqr(j, i, x, y)) / num3;
				if (num12 < 1f)
				{
					float height = worldBuilder.GetHeight(j, i);
					worldBuilder.SetHeightTrusted(j, i, SmoothStep(_height, height, num12));
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SmoothTerrainCircle(Vector2i _centerPos, int _size, int _height)
	{
		int num = _size / 2;
		float num2 = (float)num * 1.8f;
		int num3 = Mathf.CeilToInt(num2 * num2);
		int num4 = (int)((float)num * 3.2f);
		float num5 = (float)num4 - num2;
		int num6 = Utils.FastMax(_centerPos.x - num4, 1);
		int num7 = Utils.FastMax(_centerPos.y - num4, 1);
		int num8 = Utils.FastMin(_centerPos.x + num4, worldBuilder.WorldSize);
		int num9 = Utils.FastMin(_centerPos.y + num4, worldBuilder.WorldSize);
		for (int i = num7; i < num9; i++)
		{
			for (int j = num6; j < num8; j++)
			{
				int num10 = distanceSqr(j, i, _centerPos.x, _centerPos.y);
				if (num10 <= num3)
				{
					worldBuilder.SetHeightTrusted(j, i, _height);
					continue;
				}
				float num11 = (Mathf.Sqrt(num10) - num2) / num5;
				if (num11 < 1f)
				{
					float height = worldBuilder.GetHeight(j, i);
					worldBuilder.SetHeightTrusted(j, i, SmoothStep(_height, height, num11));
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float SmoothStep(float from, float to, double t)
	{
		t = -2.0 * t * t * t + 3.0 * t * t;
		return (float)((double)to * t + (double)from * (1.0 - t));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool spawnWildernessPrefab()
	{
		GameRandom gameRandom = GameRandomManager.Instance.CreateGameRandom(worldBuilder.Seed + 4096953);
		FastTags<TagGroup.Poi> withoutTags = ((worldBuilder.Towns == WorldBuilder.GenerationSelections.None) ? FastTags<TagGroup.Poi>.none : worldBuilder.StreetTileShared.traderTag);
		PrefabManager prefabManager = worldBuilder.PrefabManager;
		FastTags<TagGroup.Poi> none = FastTags<TagGroup.Poi>.none;
		Vector2i worldPositionCenter = WorldPositionCenter;
		PrefabData wildernessPrefab = prefabManager.GetWildernessPrefab(withoutTags, none, default(Vector2i), default(Vector2i), worldPositionCenter);
		int num = -1;
		while (true)
		{
			num++;
			if (num >= 6)
			{
				break;
			}
			int num2 = (wildernessPrefab.RotationsToNorth + gameRandom.RandomRange(0, 4)) & 3;
			int num3 = wildernessPrefab.size.x;
			int num4 = wildernessPrefab.size.z;
			if (num2 == 1 || num2 == 3)
			{
				num3 = wildernessPrefab.size.z;
				num4 = wildernessPrefab.size.x;
			}
			Vector2i vector2i;
			if (num3 > 150 || num4 > 150)
			{
				vector2i = WorldPositionCenter - new Vector2i((num3 - 150) / 2, (num4 - 150) / 2);
			}
			else
			{
				try
				{
					vector2i = new Vector2i(gameRandom.RandomRange(WorldPosition.x + 10, WorldPosition.x + 150 - num3 - 10), gameRandom.RandomRange(WorldPosition.y + 10, WorldPosition.y + 150 - num4 - 10));
				}
				catch
				{
					vector2i = WorldPositionCenter - new Vector2i(num3 / 2, num4 / 2);
				}
			}
			int num5 = ((num3 > num4) ? num3 : num4);
			Rect rect = new Rect(vector2i.x, vector2i.y, num5, num5);
			new Rect(rect.min - new Vector2(num5, num5) / 2f, rect.size + new Vector2(num5, num5));
			Rect rect2 = new Rect(rect.min - new Vector2(num5, num5) / 2f, rect.size + new Vector2(num5, num5));
			rect2.center = new Vector2(vector2i.x + num4 / 2, vector2i.y + num3 / 2);
			if (rect2.max.x >= (float)worldBuilder.WorldSize || rect2.min.x < 0f || rect2.max.y >= (float)worldBuilder.WorldSize || rect2.min.y < 0f)
			{
				continue;
			}
			BiomeType biome = worldBuilder.GetBiome((int)rect.center.x, (int)rect.center.y);
			int num6 = Mathf.CeilToInt(worldBuilder.GetHeight((int)rect.center.x, (int)rect.center.y));
			List<int> list = new List<int>();
			int num7 = vector2i.x;
			while (true)
			{
				if (num7 < vector2i.x + num3)
				{
					for (int i = vector2i.y; i < vector2i.y + num4; i++)
					{
						if (num7 >= worldBuilder.WorldSize || num7 < 0 || i >= worldBuilder.WorldSize || i < 0 || worldBuilder.GetWater(num7, i) > 0 || biome != worldBuilder.GetBiome(num7, i) || Mathf.Abs(Mathf.CeilToInt(worldBuilder.GetHeight(num7, i)) - num6) > 11)
						{
							goto end_IL_03fb;
						}
						list.Add((int)worldBuilder.GetHeight(num7, i));
					}
					num7++;
					continue;
				}
				num6 = getMedianHeight(list);
				if (num6 + wildernessPrefab.yOffset < 2)
				{
					break;
				}
				Vector3i vector3i = new Vector3i(subHalfWorld(vector2i.x), getHeightCeil(rect.center) + wildernessPrefab.yOffset + 1, subHalfWorld(vector2i.y));
				Vector3i vector3i2 = new Vector3i(subHalfWorld(vector2i.x), getHeightCeil(rect.center), subHalfWorld(vector2i.y));
				int num8 = worldBuilder.PrefabManager.PrefabInstanceId++;
				gameRandom.SetSeed(vector2i.x + vector2i.x * vector2i.y + vector2i.y);
				num2 = gameRandom.RandomRange(0, 4);
				num2 = (num2 + wildernessPrefab.RotationsToNorth) & 3;
				Vector2 vector = new Vector2(vector2i.x + num3 / 2, vector2i.y + num4 / 2);
				switch (num2)
				{
				case 0:
					vector = new Vector2(vector2i.x + num3 / 2, vector2i.y);
					break;
				case 1:
					vector = new Vector2(vector2i.x + num3, vector2i.y + num4 / 2);
					break;
				case 2:
					vector = new Vector2(vector2i.x + num3 / 2, vector2i.y + num4);
					break;
				case 3:
					vector = new Vector2(vector2i.x, vector2i.y + num4 / 2);
					break;
				}
				float num9 = 0f;
				if (wildernessPrefab.POIMarkers != null)
				{
					List<Prefab.Marker> list2 = wildernessPrefab.RotatePOIMarkers(_bLeft: true, num2);
					for (int num10 = list2.Count - 1; num10 >= 0; num10--)
					{
						if (list2[num10].MarkerType != Prefab.Marker.MarkerTypes.RoadExit)
						{
							list2.RemoveAt(num10);
						}
					}
					if (list2.Count > 0)
					{
						int index = gameRandom.RandomRange(0, list2.Count);
						Vector3i start = list2[index].Start;
						int num11 = ((list2[index].Size.x > list2[index].Size.z) ? list2[index].Size.x : list2[index].Size.z);
						num9 = Mathf.Max(num9, (float)num11 / 2f);
						string groupName = list2[index].GroupName;
						Vector2 vector2 = new Vector2((float)start.x + (float)list2[index].Size.x / 2f, (float)start.z + (float)list2[index].Size.z / 2f);
						vector = new Vector2((float)vector2i.x + vector2.x, (float)vector2i.y + vector2.y);
						Vector2 vector3 = vector;
						bool isPrefabPath = false;
						if (list2.Count > 1)
						{
							list2 = wildernessPrefab.POIMarkers.FindAll([PublicizedFrom(EAccessModifier.Internal)] (Prefab.Marker m) => m.MarkerType == Prefab.Marker.MarkerTypes.RoadExit && m.Start != start && m.GroupName == groupName);
							if (list2.Count > 0)
							{
								index = gameRandom.RandomRange(0, list2.Count);
								vector3 = new Vector2((float)(vector2i.x + list2[index].Start.x) + (float)list2[index].Size.x / 2f, (float)(vector2i.y + list2[index].Start.z) + (float)list2[index].Size.z / 2f);
							}
							isPrefabPath = true;
						}
						Path path = new Path(worldBuilder, _isCountryRoad: true, num9);
						path.FinalPathPoints.Add(new Vector2(vector.x, vector.y));
						path.pathPoints3d.Add(new Vector3(vector.x, vector3i2.y, vector.y));
						path.FinalPathPoints.Add(new Vector2(vector3.x, vector3.y));
						path.pathPoints3d.Add(new Vector3(vector3.x, vector3i2.y, vector3.y));
						path.IsPrefabPath = isPrefabPath;
						path.StartPointID = num8;
						path.EndPointID = num8;
						worldBuilder.wildernessPaths.Add(path);
					}
				}
				SpawnMarkerPartsAndPrefabsWilderness(wildernessPrefab, new Vector3i(vector2i.x, Mathf.CeilToInt(num6 + wildernessPrefab.yOffset + 1), vector2i.y), (byte)num2);
				PrefabDataInstance pdi = new PrefabDataInstance(num8, new Vector3i(vector3i.x, num6 + wildernessPrefab.yOffset + 1, vector3i.z), (byte)num2, wildernessPrefab);
				AddPrefab(pdi);
				worldBuilder.WildernessPrefabCount++;
				if (num6 != getHeightCeil(rect.min.x, rect.min.y) || num6 != getHeightCeil(rect.max.x, rect.min.y) || num6 != getHeightCeil(rect.min.x, rect.max.y) || num6 != getHeightCeil(rect.max.x, rect.max.y))
				{
					WildernessPOICenter = new Vector2i(rect.center);
					WildernessPOISize = Mathf.RoundToInt(Mathf.Max(rect.size.x, rect.size.y));
					WildernessPOIHeight = num6;
				}
				if (num9 != 0f)
				{
					worldBuilder.WildernessPlanner.WildernessPathInfos.Add(new WorldBuilder.WildernessPathInfo(new Vector2i(vector), num8, num9, worldBuilder.GetBiome((int)vector.x, (int)vector.y)));
				}
				int num12 = Mathf.FloorToInt(rect.x / 10f) - 1;
				int num13 = Mathf.CeilToInt(rect.xMax / 10f) + 1;
				int num14 = Mathf.FloorToInt(rect.y / 10f) - 1;
				int num15 = Mathf.CeilToInt(rect.yMax / 10f) + 1;
				for (int num16 = num12; num16 < num13; num16++)
				{
					for (int num17 = num14; num17 < num15; num17++)
					{
						if (num16 >= 0 && num16 < worldBuilder.PathingGrid.GetLength(0) && num17 >= 0 && num17 < worldBuilder.PathingGrid.GetLength(1))
						{
							if (num16 == num12 || num16 == num13 - 1 || num17 == num14 || num17 == num15 - 1)
							{
								worldBuilder.PathingUtils.SetPathBlocked(num16, num17, 2);
							}
							else
							{
								worldBuilder.PathingUtils.SetPathBlocked(num16, num17, isBlocked: true);
							}
						}
					}
				}
				num12 = Mathf.FloorToInt(rect.x) - 1;
				num13 = Mathf.CeilToInt(rect.xMax) + 1;
				num14 = Mathf.FloorToInt(rect.y) - 1;
				num15 = Mathf.CeilToInt(rect.yMax) + 1;
				for (int num18 = num12; num18 < num13; num18 += 150)
				{
					for (int num19 = num14; num19 < num15; num19 += 150)
					{
						StreetTile streetTileWorld = worldBuilder.GetStreetTileWorld(num18, num19);
						if (streetTileWorld != null)
						{
							streetTileWorld.Used = true;
						}
					}
				}
				GameRandomManager.Instance.FreeGameRandom(gameRandom);
				return true;
				continue;
				end_IL_03fb:
				break;
			}
		}
		GameRandomManager.Instance.FreeGameRandom(gameRandom);
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddPrefab(PrefabDataInstance pdi)
	{
		StreetTilePrefabDatas.Add(pdi);
		if (Township != null)
		{
			Township.AddPrefab(pdi);
		}
		else
		{
			worldBuilder.PrefabManager.AddUsedPrefabWorld(-1, pdi);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int getMedianHeight(List<int> heights)
	{
		heights.Sort();
		int count = heights.Count;
		int num = count / 2;
		if (count % 2 == 0)
		{
			return (heights[num] + heights[num - 1]) / 2;
		}
		return heights[num];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int getAverageHeight(List<int> heights)
	{
		int num = 0;
		foreach (int height in heights)
		{
			num += height;
		}
		return num / heights.Count;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 getClosestConnectionPosition(Vector2 startPos, int _wildernessId, float _radius = 4f, BiomeType _biome = BiomeType.forest)
	{
		float num = 2.1474836E+09f;
		Vector2 result = Vector2.zero;
		bool flag = false;
		if (worldBuilder.paths.Count > 0)
		{
			foreach (Path path in worldBuilder.paths)
			{
				foreach (Vector2 finalPathPoint in path.FinalPathPoints)
				{
					float num2 = distSqr(startPos, finalPathPoint);
					if (num2 < num)
					{
						num = num2;
						result = finalPathPoint;
						flag = true;
					}
				}
			}
		}
		if (worldBuilder.wildernessPaths.Count > 0)
		{
			foreach (Path wildernessPath in worldBuilder.wildernessPaths)
			{
				if ((!wildernessPath.connectsToHighway && worldBuilder.Townships.Count > 0) || (wildernessPath.IsPrefabPath && worldBuilder.Townships.Count > 0) || wildernessPath.StartPointID == _wildernessId || wildernessPath.EndPointID == _wildernessId || wildernessPath.radius < _radius)
				{
					continue;
				}
				for (int i = 2; i < wildernessPath.FinalPathPoints.Count - 2; i++)
				{
					Vector2 vector = wildernessPath.FinalPathPoints[i];
					float num3 = distSqr(startPos, vector);
					if (num3 < num && _biome == worldBuilder.GetBiome((int)vector.x, (int)vector.y))
					{
						num = num3;
						result = vector;
						flag = true;
					}
				}
			}
		}
		if (flag)
		{
			return result;
		}
		return startPos;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int getClosestConnectionDirection(Vector2 startPos, int _wildernessId, float _radius = 4f, BiomeType _biome = BiomeType.forest)
	{
		Vector2 closestConnectionPosition = getClosestConnectionPosition(startPos, _wildernessId, _radius, _biome);
		closestConnectionPosition -= startPos;
		closestConnectionPosition.Normalize();
		if (closestConnectionPosition.x + closestConnectionPosition.y != 0f)
		{
			if (Mathf.Abs(closestConnectionPosition.x) > Mathf.Abs(closestConnectionPosition.y))
			{
				if (closestConnectionPosition.x > 0f)
				{
					return 1;
				}
				return 3;
			}
			if (closestConnectionPosition.y > 0f)
			{
				return 0;
			}
			return 2;
		}
		return -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int getHeightCeil(float x, float y)
	{
		return Mathf.CeilToInt(worldBuilder.GetHeight(x, y));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int getHeightCeil(Vector2 r)
	{
		return Mathf.CeilToInt(worldBuilder.GetHeight(r));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int subHalfWorld(int pos)
	{
		return pos - worldBuilder.WorldSize / 2;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[PublicizedFrom(EAccessModifier.Private)]
	public int distanceSqr(Vector2i v1, Vector2i v2)
	{
		int num = v1.x - v2.x;
		int num2 = v1.y - v2.y;
		return num * num + num2 * num2;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[PublicizedFrom(EAccessModifier.Private)]
	public int distanceSqr(int x1, int y1, int x2, int y2)
	{
		int num = x1 - x2;
		int num2 = y1 - y2;
		return num * num + num2 * num2;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[PublicizedFrom(EAccessModifier.Private)]
	public float distSqr(Vector2 v1, Vector2 v2)
	{
		float num = v1.x - v2.x;
		float num2 = v1.y - v2.y;
		return num * num + num2 * num2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SpawnMarkerPartsAndPrefabs(PrefabData _parentPrefab, Vector3i _parentPosition, int _parentRotations, int _depth, float totalDensityLeft)
	{
		List<Prefab.Marker> list = _parentPrefab.RotatePOIMarkers(_bLeft: true, _parentRotations);
		if (list.Count == 0)
		{
			return;
		}
		FastTags<TagGroup.Poi> fastTags = FastTags<TagGroup.Poi>.Parse(District.name);
		worldBuilder.PathingUtils.AddFullyBlockedArea(Area);
		Vector3i size = _parentPrefab.size;
		if (_parentRotations % 2 == 1)
		{
			ref int z = ref size.z;
			ref int x = ref size.x;
			int x2 = size.x;
			int z2 = size.z;
			z = x2;
			x = z2;
		}
		List<Prefab.Marker> list2 = list.FindAll([PublicizedFrom(EAccessModifier.Internal)] (Prefab.Marker m) => m.MarkerType == Prefab.Marker.MarkerTypes.POISpawn);
		if (_depth < 5 && list2.Count > 0)
		{
			list2.Sort([PublicizedFrom(EAccessModifier.Internal)] (Prefab.Marker m1, Prefab.Marker m2) => (m2.Size.x + m2.Size.y + m2.Size.z).CompareTo(m1.Size.x + m1.Size.y + m1.Size.z));
			List<string> list3 = new List<string>();
			for (int num = 0; num < list2.Count; num++)
			{
				if (!list3.Contains(list2[num].GroupName))
				{
					list3.Add(list2[num].GroupName);
				}
			}
			Township.rand.SetSeed(Township.ID + (_parentPosition.x * _parentPosition.x + _parentPosition.y * _parentPosition.y));
			Vector2i vector2i5 = default(Vector2i);
			foreach (string groupName in list3)
			{
				List<Prefab.Marker> list4 = (from m in list2
					where m.GroupName == groupName
					orderby Township.rand.RandomFloat descending
					select m).ToList();
				for (int num2 = 0; num2 < list4.Count; num2++)
				{
					Prefab.Marker marker = list4[num2];
					Vector2i vector2i = new Vector2i(marker.Size.x, marker.Size.z);
					Vector2i vector2i2 = new Vector2i(marker.Start.x, marker.Start.z);
					_ = vector2i2 + vector2i;
					Vector2i vector2i3 = vector2i2 + vector2i / 2;
					Vector2i vector2i4 = vector2i;
					if (District.spawnCustomSizePrefabs)
					{
						int num3;
						if (District.name != "gateway" && (num3 = Prefab.Marker.MarkerSizes.IndexOf(new Vector3i(vector2i.x, 0, vector2i.y))) >= 0)
						{
							if (num3 > 0)
							{
								vector2i4 = new Vector2i(Prefab.Marker.MarkerSizes[num3 - 1].x + 1, Prefab.Marker.MarkerSizes[num3 - 1].z + 1);
							}
						}
						else
						{
							vector2i4 = vector2i / 2;
						}
					}
					Vector2i center = new Vector2i(_parentPosition.x + vector2i3.x, _parentPosition.z + vector2i3.y);
					if (_depth == 0)
					{
						int halfWorldSize = worldBuilder.HalfWorldSize;
						Vector2 a = new Vector2(center.x - halfWorldSize, center.y - halfWorldSize);
						float num4 = 0f;
						List<PrefabDataInstance> prefabs = Township.Prefabs;
						for (int num5 = 0; num5 < prefabs.Count; num5++)
						{
							PrefabDataInstance prefabDataInstance = prefabs[num5];
							float densityScore = prefabDataInstance.prefab.DensityScore;
							if (densityScore > 6f)
							{
								Vector2 centerXZV = prefabDataInstance.CenterXZV2;
								if (Vector2.Distance(a, centerXZV) < 190f)
								{
									num4 = ((!(densityScore >= 20f)) ? (num4 + (densityScore - 6f)) : (num4 + densityScore * 1.3f));
								}
							}
						}
						if (num4 > 0f)
						{
							totalDensityLeft = Utils.FastMax(6f, totalDensityLeft - num4);
						}
					}
					PrefabData prefabWithDistrict = worldBuilder.PrefabManager.GetPrefabWithDistrict(District, marker.Tags, vector2i4, vector2i, center, totalDensityLeft, 1f);
					if (prefabWithDistrict == null)
					{
						prefabWithDistrict = worldBuilder.PrefabManager.GetPrefabWithDistrict(District, marker.Tags, vector2i4, vector2i, center, totalDensityLeft + 8f, 0.3f);
						if (prefabWithDistrict == null)
						{
							prefabWithDistrict = worldBuilder.PrefabManager.GetPrefabWithDistrict(District, marker.Tags, vector2i4, vector2i, center, 18f, 0f);
							if (prefabWithDistrict == null)
							{
								Log.Warning("SpawnMarkerPartsAndPrefabs failed {0}, tags {1}, size {2} {3}, totalDensityLeft {4}", District.name, marker.Tags, vector2i4, vector2i, totalDensityLeft);
								continue;
							}
							Log.Warning("SpawnMarkerPartsAndPrefabs retry2 {0}, tags {1}, size {2} {3}, totalDensityLeft {4}, picked {5}, density {6}", District.name, marker.Tags, vector2i4, vector2i, totalDensityLeft, prefabWithDistrict.Name, prefabWithDistrict.DensityScore);
						}
					}
					int num6 = _parentPosition.x + marker.Start.x;
					int num7 = _parentPosition.z + marker.Start.z;
					if (_parentPosition.y + marker.Start.y + prefabWithDistrict.yOffset < 3)
					{
						Log.Error("SpawnMarkerPartsAndPrefabs y low! {0}, pos {1} {2}", prefabWithDistrict.Name, num6, num7);
						continue;
					}
					totalDensityLeft -= prefabWithDistrict.DensityScore;
					if (prefabWithDistrict.Tags.Test_AnySet(worldBuilder.StreetTileShared.traderTag) || prefabWithDistrict.Name.Contains("trader"))
					{
						vector2i5.x = num6 + marker.Size.x / 2;
						vector2i5.y = num7 + marker.Size.z / 2;
						worldBuilder.TraderCenterPositions.Add(vector2i5);
						if (BiomeType == BiomeType.forest)
						{
							worldBuilder.TraderForestCenterPositions.Add(vector2i5);
						}
						HasTrader = true;
						Log.Out("Trader {0}, {1}, {2}, marker {3}, at {4}", prefabWithDistrict.Name, BiomeType, District.name, marker.Name, vector2i5);
					}
					int num8 = marker.Rotations;
					byte b = (byte)((_parentRotations + prefabWithDistrict.RotationsToNorth + num8) & 3);
					int num9 = prefabWithDistrict.size.x;
					int num10 = prefabWithDistrict.size.z;
					if (b == 1 || b == 3)
					{
						int num11 = num9;
						int z2 = num10;
						num10 = num11;
						num9 = z2;
					}
					switch (num8)
					{
					case 2:
						num6 += vector2i.x / 2 - num9 / 2;
						break;
					case 3:
						num7 += vector2i.y / 2 - num10 / 2;
						num6 += vector2i.x;
						num6 -= num9;
						break;
					case 0:
						num6 += vector2i.x / 2 - num9 / 2;
						num7 += vector2i.y;
						num7 -= num10;
						break;
					case 1:
						num7 += vector2i.y / 2 - num10 / 2;
						break;
					}
					Vector3i position = new Vector3i(num6, _parentPosition.y + marker.Start.y + prefabWithDistrict.yOffset, num7) + worldBuilder.PrefabWorldOffset;
					PrefabDataInstance prefabDataInstance2 = new PrefabDataInstance(worldBuilder.PrefabManager.PrefabInstanceId++, position, b, prefabWithDistrict);
					Color color = District.preview_color;
					if (prefabDataInstance2.prefab.Name.StartsWith("remnant_") || prefabDataInstance2.prefab.Name.StartsWith("abandoned_"))
					{
						color.r *= 0.75f;
						color.g *= 0.75f;
						color.b *= 0.75f;
					}
					else if (prefabDataInstance2.prefab.DensityScore < 1f)
					{
						color.r *= 0.4f;
						color.g *= 0.4f;
						color.b *= 0.4f;
					}
					else if (prefabDataInstance2.prefab.Name.StartsWith("trader_"))
					{
						color = new Color(0.6f, 0.3f, 0.3f);
					}
					prefabDataInstance2.previewColor = color;
					Township.AddPrefab(prefabDataInstance2);
					SpawnMarkerPartsAndPrefabs(prefabWithDistrict, new Vector3i(num6, _parentPosition.y + marker.Start.y + prefabWithDistrict.yOffset, num7), b, _depth + 1, totalDensityLeft);
					break;
				}
			}
		}
		List<Prefab.Marker> list5 = list.FindAll([PublicizedFrom(EAccessModifier.Internal)] (Prefab.Marker m) => m.MarkerType == Prefab.Marker.MarkerTypes.PartSpawn);
		if (_depth < 20 && list5.Count > 0)
		{
			List<string> list6 = new List<string>();
			for (int num12 = 0; num12 < list5.Count; num12++)
			{
				if (!list6.Contains(list5[num12].GroupName))
				{
					list6.Add(list5[num12].GroupName);
				}
			}
			Township.rand.SetSeed(Township.ID + (_parentPosition.x * _parentPosition.x + _parentPosition.y * _parentPosition.y) + 1);
			foreach (string groupName2 in list6)
			{
				List<Prefab.Marker> list7 = (from m in list5
					where m.GroupName == groupName2
					orderby Township.rand.RandomFloat descending
					select m).ToList();
				float num13 = 1f;
				if (list7.Count > 1)
				{
					num13 = 0f;
					foreach (Prefab.Marker item in list7)
					{
						num13 += item.PartChanceToSpawn;
					}
				}
				float num14 = 0f;
				foreach (Prefab.Marker item2 in list7)
				{
					num14 += item2.PartChanceToSpawn / num13;
					if (Township.rand.RandomRange(0f, 1f) > num14)
					{
						continue;
					}
					if (!item2.Tags.IsEmpty)
					{
						if (_depth == 0)
						{
							if (!District.tag.Test_AnySet(item2.Tags))
							{
								continue;
							}
						}
						else if (!item2.Tags.IsEmpty && !fastTags.Test_AnySet(item2.Tags))
						{
							continue;
						}
					}
					PrefabData prefabByName = worldBuilder.PrefabManager.GetPrefabByName(item2.PartToSpawn);
					if (prefabByName == null)
					{
						Log.Error("Part to spawn {0} not found!", item2.PartToSpawn);
						continue;
					}
					Vector3i vector3i = new Vector3i(_parentPosition.x + item2.Start.x - worldBuilder.WorldSize / 2, _parentPosition.y + item2.Start.y, _parentPosition.z + item2.Start.z - worldBuilder.WorldSize / 2);
					if (vector3i.y <= 0)
					{
						continue;
					}
					byte b2 = item2.Rotations;
					switch (b2)
					{
					case 1:
						b2 = 3;
						break;
					case 3:
						b2 = 1;
						break;
					}
					byte b3 = (byte)((_parentRotations + prefabByName.RotationsToNorth + b2) % 4);
					Vector3i vector3i2 = prefabByName.size;
					if (b3 == 1 || b3 == 3)
					{
						vector3i2 = new Vector3i(vector3i2.z, vector3i2.y, vector3i2.x);
					}
					Bounds bounds = new Bounds(vector3i + vector3i2 * 0.5f, vector3i2 - Vector3.one);
					foreach (Bounds partBound in partBounds)
					{
						if (!partBound.Intersects(bounds))
						{
							continue;
						}
						goto IL_0cbe;
					}
					Township.AddPrefab(new PrefabDataInstance(worldBuilder.PrefabManager.PrefabInstanceId++, vector3i, b3, prefabByName));
					totalDensityLeft -= prefabByName.DensityScore;
					partBounds.Add(bounds);
					SpawnMarkerPartsAndPrefabs(prefabByName, _parentPosition + item2.Start, b3, _depth + 1, totalDensityLeft);
					break;
					IL_0cbe:;
				}
			}
		}
		if (District == null || !(District.name == "gateway"))
		{
			return;
		}
		list5 = list.FindAll([PublicizedFrom(EAccessModifier.Internal)] (Prefab.Marker m) => m.PartToSpawn.Contains("highway_transition"));
		if (list5.Count <= 0)
		{
			return;
		}
		foreach (Prefab.Marker item3 in list5)
		{
			Vector2 vector = new Vector2(item3.Start.x, item3.Start.z) - new Vector2(size.x / 2, size.z / 2);
			if (Mathf.Abs(vector.x) > Mathf.Abs(vector.y))
			{
				if (vector.x > 0f)
				{
					if (!HasExitTo(GetNeighbor(Vector2i.right)) || GetNeighbor(Vector2i.right).Township != Township)
					{
						continue;
					}
				}
				else if (!HasExitTo(GetNeighbor(Vector2i.left)) || GetNeighbor(Vector2i.left).Township != Township)
				{
					continue;
				}
			}
			else if (vector.y > 0f)
			{
				if (!HasExitTo(GetNeighbor(Vector2i.up)) || GetNeighbor(Vector2i.up).Township != Township)
				{
					continue;
				}
			}
			else if (!HasExitTo(GetNeighbor(Vector2i.down)) || GetNeighbor(Vector2i.down).Township != Township)
			{
				continue;
			}
			PrefabData prefabByName2 = worldBuilder.PrefabManager.GetPrefabByName(item3.PartToSpawn);
			if (prefabByName2 == null)
			{
				continue;
			}
			Vector3i position2 = new Vector3i(_parentPosition.x + item3.Start.x - worldBuilder.WorldSize / 2, _parentPosition.y + item3.Start.y, _parentPosition.z + item3.Start.z - worldBuilder.WorldSize / 2);
			if (position2.y > 0)
			{
				byte b4 = item3.Rotations;
				switch (b4)
				{
				case 1:
					b4 = 3;
					break;
				case 3:
					b4 = 1;
					break;
				}
				byte b5 = (byte)((_parentRotations + prefabByName2.RotationsToNorth + b4) % 4);
				Vector3i size2 = prefabByName2.size;
				if (b5 == 1 || b5 == 3)
				{
					size2 = new Vector3i(size2.z, size2.y, size2.x);
				}
				Township.AddPrefab(new PrefabDataInstance(worldBuilder.PrefabManager.PrefabInstanceId++, position2, b5, prefabByName2));
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SpawnMarkerPartsAndPrefabsWilderness(PrefabData _parentPrefab, Vector3i _parentPosition, int _parentRotations)
	{
		GameRandom gameRandom = GameRandomManager.Instance.CreateGameRandom(_parentPosition.ToString().GetHashCode());
		List<Prefab.Marker> list = _parentPrefab.RotatePOIMarkers(_bLeft: true, _parentRotations);
		List<Prefab.Marker> list2 = list.FindAll([PublicizedFrom(EAccessModifier.Internal)] (Prefab.Marker m) => m.MarkerType == Prefab.Marker.MarkerTypes.POISpawn);
		if (list2.Count > 0)
		{
			for (int num = 0; num < list2.Count; num++)
			{
				Prefab.Marker marker = list2[num];
				Vector2i vector2i = new Vector2i(marker.Size.x, marker.Size.z);
				Vector2i vector2i2 = new Vector2i(marker.Start.x, marker.Start.z) + vector2i / 2;
				Vector2i minSize = vector2i;
				PrefabData wildernessPrefab = worldBuilder.PrefabManager.GetWildernessPrefab(worldBuilder.StreetTileShared.traderTag, marker.Tags, minSize, vector2i, new Vector2i(_parentPosition.x + vector2i2.x, _parentPosition.z + vector2i2.y));
				if (wildernessPrefab != null)
				{
					int num2 = _parentPosition.x + marker.Start.x;
					int num3 = _parentPosition.z + marker.Start.z;
					int num4 = marker.Rotations;
					byte b = (byte)((_parentRotations + wildernessPrefab.RotationsToNorth + num4) & 3);
					int num5 = wildernessPrefab.size.x;
					int num6 = wildernessPrefab.size.z;
					if (b == 1 || b == 3)
					{
						int num7 = num5;
						num5 = num6;
						num6 = num7;
					}
					switch (num4)
					{
					case 2:
						num2 += vector2i.x / 2 - num5 / 2;
						break;
					case 3:
						num3 += vector2i.y / 2 - num6 / 2;
						num2 += vector2i.x;
						num2 -= num5;
						break;
					case 0:
						num2 += vector2i.x / 2 - num5 / 2;
						num3 += vector2i.y;
						num3 -= num6;
						break;
					case 1:
						num3 += vector2i.y / 2 - num6 / 2;
						break;
					}
					PrefabDataInstance pdi = new PrefabDataInstance(_position: new Vector3i(num2 - worldBuilder.WorldSize / 2, _parentPosition.y + marker.Start.y + wildernessPrefab.yOffset, num3 - worldBuilder.WorldSize / 2), _id: worldBuilder.PrefabManager.PrefabInstanceId++, _rotation: b, _prefabData: wildernessPrefab);
					AddPrefab(pdi);
					worldBuilder.WildernessPrefabCount++;
					wildernessPrefab.RotatePOIMarkers(_bLeft: true, b);
					SpawnMarkerPartsAndPrefabsWilderness(wildernessPrefab, new Vector3i(num2, _parentPosition.y + marker.Start.y + wildernessPrefab.yOffset, num3), b);
				}
			}
		}
		List<Prefab.Marker> list3 = list.FindAll([PublicizedFrom(EAccessModifier.Internal)] (Prefab.Marker m) => m.MarkerType == Prefab.Marker.MarkerTypes.PartSpawn);
		if (list3.Count > 0)
		{
			List<string> list4 = new List<string>();
			for (int num8 = 0; num8 < list3.Count; num8++)
			{
				if (!list4.Contains(list3[num8].GroupName))
				{
					list4.Add(list3[num8].GroupName);
				}
			}
			foreach (string groupName in list4)
			{
				List<Prefab.Marker> list5 = list3.FindAll([PublicizedFrom(EAccessModifier.Internal)] (Prefab.Marker m) => m.GroupName == groupName);
				float num9 = 1f;
				if (list5.Count > 1)
				{
					num9 = 0f;
					foreach (Prefab.Marker item in list5)
					{
						num9 += item.PartChanceToSpawn;
					}
				}
				float num10 = 0f;
				foreach (Prefab.Marker item2 in list5)
				{
					num10 += item2.PartChanceToSpawn / num9;
					if (gameRandom.RandomRange(0f, 1f) > num10 || (!item2.Tags.IsEmpty && !worldBuilder.StreetTileShared.wildernessTag.Test_AnySet(item2.Tags)))
					{
						continue;
					}
					PrefabData prefabByName = worldBuilder.PrefabManager.GetPrefabByName(item2.PartToSpawn);
					if (prefabByName == null)
					{
						Log.Error("Part to spawn {0} not found!", item2.PartToSpawn);
						continue;
					}
					Vector3i position = new Vector3i(_parentPosition.x + item2.Start.x - worldBuilder.WorldSize / 2, _parentPosition.y + item2.Start.y, _parentPosition.z + item2.Start.z - worldBuilder.WorldSize / 2);
					byte b2 = item2.Rotations;
					switch (b2)
					{
					case 1:
						b2 = 3;
						break;
					case 3:
						b2 = 1;
						break;
					}
					byte b3 = (byte)((_parentRotations + prefabByName.RotationsToNorth + b2) % 4);
					PrefabDataInstance pdi2 = new PrefabDataInstance(worldBuilder.PrefabManager.PrefabInstanceId++, position, b3, prefabByName);
					AddPrefab(pdi2);
					worldBuilder.WildernessPrefabCount++;
					SpawnMarkerPartsAndPrefabsWilderness(prefabByName, _parentPosition + item2.Start, b3);
					break;
				}
			}
		}
		GameRandomManager.Instance.FreeGameRandom(gameRandom);
	}

	public int GetNumTownshipNeighbors()
	{
		int num = 0;
		StreetTile[] array = GetNeighbors();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].Township == Township)
			{
				num++;
			}
		}
		return num;
	}
}
