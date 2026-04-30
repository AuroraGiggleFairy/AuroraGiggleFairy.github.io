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
	public const float cHeightDiffMax = 20f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int partDepthLimit = 20;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int poiDepthLimit = 5;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cTilePadInside = 20;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cTileSizeInside = 110;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSmoothFullRadius = 1.8f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSmoothFadeRadius = 3.2f;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly WorldBuilder worldBuilder;

	public Township Township;

	public District District;

	public readonly List<PrefabDataInstance> StreetTilePrefabDatas = new List<PrefabDataInstance>();

	public readonly Vector2i GridPosition;

	public readonly Vector2i WorldPosition;

	public readonly Vector2i WorldPositionCenter;

	public readonly Rect Area;

	public bool OverlapsRadiation;

	public bool OverlapsBiomes;

	public bool HasSteepSlope;

	public bool AllIsWater;

	public bool HasTrader;

	public bool HasFeature;

	public Vector2i WildernessPOIPos;

	public Vector2i WildernessPOICenter;

	public Vector2i WildernessPOISize;

	public int WildernessPOIHeight;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i[] highwayExitPositions;

	[PublicizedFrom(EAccessModifier.Private)]
	public int RoadShape;

	public int ConnectedExits;

	public readonly List<Vector2i> UsedExitList = new List<Vector2i>();

	public readonly List<Path> ConnectedHighways = new List<Path>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Bounds> partBounds = new List<Bounds>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int rotations;

	[PublicizedFrom(EAccessModifier.Private)]
	public StreetTile[] neighbors4Way;

	[PublicizedFrom(EAccessModifier.Private)]
	public StreetTile[] neighbors8Way;

	public bool Used;

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
			if (GetData().OverlapsWater)
			{
				return false;
			}
			if (!OverlapsBiomes && !OverlapsRadiation && !HasSteepSlope)
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
			if (GetData().OverlapsWater)
			{
				return false;
			}
			if (!OverlapsBiomes && !OverlapsRadiation && !HasSteepSlope && TerrainType != TerrainType.mountains)
			{
				return !HasPrefabs;
			}
			return false;
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
			if (RoadShape < 0)
			{
				return 0;
			}
			return worldBuilder.StreetTileShared.RoadShapeExitCounts[RoadShape];
		}
	}

	public string PrefabName
	{
		get
		{
			if (RoadShape < 0)
			{
				return "none";
			}
			return worldBuilder.StreetTileShared.RoadShapesDistrict[RoadShape];
		}
	}

	public int Rotations
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return rotations;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (value != rotations)
			{
				rotations = value;
			}
		}
	}

	public bool ContainsHighway => ConnectedHighways.Count > 0;

	public bool NeedsWildernessSmoothing => WildernessPOISize.x > 0;

	public void SetTownship(Township _township)
	{
		Township = _township;
		GetData().IsCity = _township != null && !_township.IsWilderness();
	}

	public bool HasRoadExit(int _dir)
	{
		if (RoadShape < 0)
		{
			return false;
		}
		return (worldBuilder.StreetTileShared.RoadShapeExitsPerRotation[RoadShape][Rotations] & (1 << _dir)) > 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetRoadExits(int _shape, int _rotations)
	{
		return worldBuilder.StreetTileShared.RoadShapeExitsPerRotation[_shape][_rotations];
	}

	public StreetTile(WorldBuilder _worldBuilder, Vector2i gridPosition)
	{
		worldBuilder = _worldBuilder;
		GridPosition = gridPosition;
		WorldPosition = GridPosition * 150;
		WorldPositionCenter = WorldPosition + new Vector2i(75, 75);
		Area = new Rect(WorldPosition.AsVector2(), Vector2.one * 150f);
		RoadShape = -1;
		Rotations = 0;
		if (GridPosition.x < 1 || GridPosition.x >= worldBuilder.StreetTileMapWidth - 1)
		{
			OverlapsRadiation = true;
		}
		if (GridPosition.y < 1 || GridPosition.y >= worldBuilder.StreetTileMapWidth - 1)
		{
			OverlapsRadiation = true;
		}
		HighwayExitInit();
		UpdateValidity();
	}

	public void UpdateValidity()
	{
		float positionHeight = PositionHeight;
		Vector2i worldPositionCenter = WorldPositionCenter;
		Vector2i[] dir9way = worldBuilder.StreetTileShared.dir9way;
		foreach (Vector2i vector2i in dir9way)
		{
			Vector2i vector2i2 = worldPositionCenter + vector2i * 75;
			if (worldBuilder.GetRad(vector2i2.x, vector2i2.y) > 0)
			{
				OverlapsRadiation = true;
			}
			if (Utils.FastAbs(worldBuilder.GetHeight(vector2i2.x, vector2i2.y) - positionHeight) > 20f)
			{
				HasSteepSlope = true;
			}
		}
		ref StreetTileData data = ref GetData();
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
				if (worldBuilder.data.GetWater(k, j) > 0)
				{
					num2++;
					data.OverlapsWater = true;
				}
			}
		}
		if ((float)num2 / (float)num > 0.9f)
		{
			AllIsWater = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ref StreetTileData GetData()
	{
		return ref worldBuilder.data.GetStreetTileDataWorld(WorldPosition.x, WorldPosition.y);
	}

	public Stamp GetStamp()
	{
		Color32 customColor = ((District.type == District.Type.Gateway) ? new Color32(byte.MaxValue, 0, 204, byte.MaxValue) : new Color32(byte.MaxValue, 0, 0, byte.MaxValue));
		int num = RoadShape;
		if (num < 0)
		{
			num = 2;
			customColor = Color.black;
		}
		return new Stamp(_transData: new TranslationData(WorldPositionCenter.x, WorldPositionCenter.y, 1f, rotations * -90), _worldBuilder: worldBuilder, _stamp: worldBuilder.StampManager.GetStamp(worldBuilder.StreetTileShared.RoadShapes[num]), _customColor: customColor);
	}

	public StreetTile[] GetNeighbors()
	{
		if (neighbors4Way == null)
		{
			neighbors4Way = new StreetTile[4];
			for (int i = 0; i < 4; i++)
			{
				neighbors4Way[i] = GetNeighbor(worldBuilder.StreetTileShared.dir4way[i]);
			}
		}
		return neighbors4Way;
	}

	public int GetNeighborCount()
	{
		if (neighbors4Way == null)
		{
			GetNeighbors();
		}
		int num = 0;
		for (int i = 0; i < 4; i++)
		{
			if (neighbors4Way[i] != null)
			{
				num++;
			}
		}
		return num;
	}

	public StreetTile[] GetNeighbors8Way()
	{
		if (neighbors8Way == null)
		{
			neighbors8Way = new StreetTile[8];
			for (int i = 0; i < 8; i++)
			{
				neighbors8Way[i] = GetNeighbor(worldBuilder.StreetTileShared.dir8way[i]);
			}
		}
		return neighbors8Way;
	}

	public StreetTile GetNeighbor(Vector2i direction)
	{
		return worldBuilder.GetStreetTileGrid(GridPosition + direction);
	}

	public bool HasNeighbor(StreetTile otherTile)
	{
		StreetTile[] neighbors = GetNeighbors();
		for (int i = 0; i < neighbors.Length; i++)
		{
			if (neighbors[i] == otherTile)
			{
				return true;
			}
		}
		return false;
	}

	public StreetTile GetNeighbor(int _dir)
	{
		if (neighbors4Way == null)
		{
			GetNeighbors();
		}
		return neighbors4Way[_dir];
	}

	public int GetNeighborDir(StreetTile otherTile)
	{
		if (neighbors4Way == null)
		{
			GetNeighbors();
		}
		for (int i = 0; i < 4; i++)
		{
			if (neighbors4Way[i] == otherTile)
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
		int neighborDir = GetNeighborDir(otherTile);
		if (neighborDir < 0)
		{
			return false;
		}
		return HasRoadExit(neighborDir);
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

	[PublicizedFrom(EAccessModifier.Private)]
	public void HighwayExitInit()
	{
		highwayExitPositions = new Vector2i[4];
		for (int i = 0; i < 4; i++)
		{
			highwayExitPositions[i] = HighwayExitFromDir(i);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i HighwayExitFromDir(int _dir)
	{
		Vector2i result = default(Vector2i);
		switch (_dir)
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

	public int GetHighwayExitDir(Vector2i _worldPos)
	{
		for (int i = 0; i < 4; i++)
		{
			if (GetHighwayExitPos(i) == _worldPos)
			{
				return i;
			}
		}
		return 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector2i GetHighwayExitPos(int _dir)
	{
		return highwayExitPositions[_dir];
	}

	public List<Vector2i> GetHighwayExits(bool _isGateway = false)
	{
		List<Vector2i> list = new List<Vector2i>();
		if (UsedExitList.Count == 1)
		{
			int num = -1;
			for (int i = 0; i < 4; i++)
			{
				if ((ConnectedExits & (1 << i)) > 0)
				{
					num = (i + 2) & 3;
					break;
				}
			}
			if (num != -1)
			{
				list.Add(GetHighwayExitPos(num));
			}
			else
			{
				Log.Error("Could not find opposite highway exit!");
			}
		}
		else
		{
			for (int j = 0; j < 4; j++)
			{
				if ((ConnectedExits & (1 << j)) <= 0)
				{
					list.Add(GetHighwayExitPos(j));
				}
			}
		}
		return list;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetRoadShape(int _exits)
	{
		int count = worldBuilder.StreetTileShared.RoadShapeExitCounts.Count;
		for (int i = 0; i < count; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				if (_exits == GetRoadExits(i, j))
				{
					RoadShape = i;
					Rotations = j;
					return;
				}
			}
		}
		RoadShape = -1;
		Rotations = 0;
	}

	public int GetExitUsedCount()
	{
		return UsedExitList.Count();
	}

	public void SetExits(int _exits)
	{
		ConnectedExits = _exits;
		SetRoadShape(_exits);
		UsedExitList.Clear();
		for (int i = 0; i < 4; i++)
		{
			if ((_exits & (1 << i)) > 0)
			{
				Vector2i highwayExitPos = GetHighwayExitPos(i);
				UsedExitList.Add(highwayExitPos);
			}
		}
	}

	public void SetExitUsed(int _dir)
	{
		ConnectedExits |= 1 << _dir;
		SetExits(ConnectedExits);
	}

	public bool SetExitUsed(Vector2i exit)
	{
		for (int i = 0; i < 4; i++)
		{
			if (GetHighwayExitPos(i) == exit)
			{
				SetExitUsed(i);
				return true;
			}
		}
		return false;
	}

	public void SetExitUnUsed(int _dir)
	{
		ConnectedExits &= ~(1 << _dir);
		SetExits(ConnectedExits);
	}

	public void SetExitUnUsed(Vector2i _exit)
	{
		for (int i = 0; i < 4; i++)
		{
			if (GetHighwayExitPos(i) == _exit)
			{
				SetExitUnUsed(i);
				break;
			}
		}
	}

	public void AddHighway(Path _path)
	{
		if (!ConnectedHighways.Contains(_path))
		{
			ConnectedHighways.Add(_path);
			GetData().ConnectedHighwayCount = ConnectedHighways.Count;
		}
	}

	public void RemoveHighway(Path _path)
	{
		if (ConnectedHighways.Remove(_path))
		{
			GetData().ConnectedHighwayCount = ConnectedHighways.Count;
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
		spawnStreetTile(WorldPosition, streetPrefabName, Rotations);
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
		int num = Township.Height + streetTile.yOffset;
		if (num < 3)
		{
			return false;
		}
		int num2 = (baseRotations + streetTile.RotationsToNorth) & 3;
		switch (num2)
		{
		case 1:
			num2 = 3;
			break;
		case 3:
			num2 = 1;
			break;
		}
		Vector3i position = new Vector3i(tileMinPositionWorld.x, num, tileMinPositionWorld.y) + worldBuilder.PrefabWorldOffset;
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
		AddPrefab(new PrefabDataInstance(worldBuilder.PrefabManager.PrefabInstanceId++, position, (byte)num2, streetTile));
		SpawnMarkerPartsAndPrefabs(streetTile, new Vector3i(WorldPosition.x, num, WorldPosition.y), num2, 0, totalDensityLeft);
		return true;
	}

	public void SmoothWildernessTerrain()
	{
		SmoothTerrainRect(WildernessPOIPos, WildernessPOISize.x, WildernessPOISize.y, WildernessPOIHeight, 18);
	}

	public void SmoothTownshipTerrain()
	{
		if (Township != null && District != null)
		{
			Township.CalcCenterStreetTile();
			int fadeRange = ((Township.Streets.Count <= 2) ? 50 : 110);
			SmoothTerrainRect(WorldPosition, 150, 150, Township.Height, fadeRange);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SmoothTerrainRect(Vector2i _startPos, int _sizeX, int _sizeY, int _height, int _fadeRange)
	{
		int num = _fadeRange + 1;
		float num2 = _fadeRange;
		int x = _startPos.x;
		int num3 = _startPos.x + _sizeX;
		int y = _startPos.y;
		int num4 = _startPos.y + _sizeY;
		int num5 = Utils.FastMax(x - num, 1);
		int num6 = Utils.FastMax(y - num, 1);
		int num7 = Utils.FastMin(num3 + num, worldBuilder.WorldSize);
		int num8 = Utils.FastMin(num4 + num, worldBuilder.WorldSize);
		for (int i = num6; i < num8; i++)
		{
			bool flag = i >= y && i <= num4;
			int y2 = (flag ? i : ((i < _startPos.y) ? y : num4));
			for (int j = num5; j < num7; j++)
			{
				bool flag2 = j >= x && j <= num3;
				if (flag2 && flag)
				{
					worldBuilder.SetHeightTrusted(j, i, _height);
					continue;
				}
				int x2 = (flag2 ? j : ((j < _startPos.x) ? x : num3));
				float num9 = Mathf.Sqrt(distanceSqr(j, i, x2, y2)) / num2;
				if (num9 < 1f)
				{
					float height = worldBuilder.GetHeight(j, i);
					worldBuilder.SetHeightTrusted(j, i, SmoothStep(_height, height, num9));
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
		GameRandom gameRandom = GameRandomManager.Instance.CreateGameRandom(worldBuilder.Seed + 4096953 + GridPosition.x + GridPosition.y * 200);
		FastTags<TagGroup.Poi> withoutTags = ((worldBuilder.Towns == WorldBuilder.GenerationSelections.None) ? FastTags<TagGroup.Poi>.none : worldBuilder.StreetTileShared.traderTag);
		PrefabManager prefabManager = worldBuilder.PrefabManager;
		FastTags<TagGroup.Poi> none = FastTags<TagGroup.Poi>.none;
		Vector2i worldPositionCenter = WorldPositionCenter;
		PrefabData wildernessPrefab = prefabManager.GetWildernessPrefab(withoutTags, none, default(Vector2i), default(Vector2i), worldPositionCenter);
		for (int i = 0; i < 6; i++)
		{
			if (spawnWildernessPrefab(wildernessPrefab, gameRandom))
			{
				GameRandomManager.Instance.FreeGameRandom(gameRandom);
				return true;
			}
		}
		GameRandomManager.Instance.FreeGameRandom(gameRandom);
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool spawnWildernessPrefab(PrefabData prefab, GameRandom rndm)
	{
		int num = (prefab.RotationsToNorth + rndm.RandomRange(0, 4)) & 3;
		int num2 = prefab.size.x;
		int num3 = prefab.size.z;
		if (num == 1 || num == 3)
		{
			num2 = prefab.size.z;
			num3 = prefab.size.x;
		}
		Vector2i wildernessPOIPos = default(Vector2i);
		if (num2 >= 110 || num3 >= 110)
		{
			wildernessPOIPos = WorldPositionCenter - new Vector2i(num2 / 2, num3 / 2);
			if (num2 > 150 || num3 > 150)
			{
				Log.Warning("RWG spawnWildernessPrefab {0}, overflows TileSize {1}", prefab.Name, 150);
			}
		}
		else
		{
			wildernessPOIPos.x = WorldPosition.x + 20 + rndm.RandomRange(110 - num2);
			wildernessPOIPos.y = WorldPosition.y + 20 + rndm.RandomRange(110 - num3);
		}
		if (wildernessPOIPos.x < 0 || wildernessPOIPos.x + num2 > worldBuilder.WorldSize)
		{
			return false;
		}
		if (wildernessPOIPos.y < 0 || wildernessPOIPos.y + num3 > worldBuilder.WorldSize)
		{
			return false;
		}
		Vector2i wildernessPOICenter = default(Vector2i);
		wildernessPOICenter.x = wildernessPOIPos.x + num2 / 2;
		wildernessPOICenter.y = wildernessPOIPos.y + num3 / 2;
		Vector2i vector2i = default(Vector2i);
		vector2i.x = wildernessPOIPos.x + num2 - 1;
		vector2i.y = wildernessPOIPos.y + num3 - 1;
		BiomeType biome = worldBuilder.GetBiome(wildernessPOICenter.x, wildernessPOICenter.y);
		int num4 = Mathf.CeilToInt(worldBuilder.GetHeight(wildernessPOICenter.x, wildernessPOICenter.y));
		List<int> list = new List<int>();
		for (int i = wildernessPOIPos.y; i < wildernessPOIPos.y + num3; i++)
		{
			for (int j = wildernessPOIPos.x; j < wildernessPOIPos.x + num2; j++)
			{
				if (worldBuilder.data.GetWater(j, i) > 0)
				{
					return false;
				}
				if (biome != worldBuilder.GetBiome(j, i))
				{
					return false;
				}
				int num5 = Mathf.CeilToInt(worldBuilder.GetHeight(j, i));
				if (Utils.FastAbsInt(num5 - num4) > 11)
				{
					return false;
				}
				list.Add(num5);
			}
		}
		num4 = getMedianHeight(list);
		if (num4 + prefab.yOffset < 2)
		{
			return false;
		}
		int heightCeil = getHeightCeil(wildernessPOICenter.x, wildernessPOICenter.y);
		Vector3i vector3i = new Vector3i(subHalfWorld(wildernessPOIPos.x), heightCeil, subHalfWorld(wildernessPOIPos.y));
		int id = worldBuilder.PrefabManager.PrefabInstanceId++;
		rndm.SetSeed(wildernessPOIPos.x + wildernessPOIPos.x * wildernessPOIPos.y + wildernessPOIPos.y);
		if (prefab.POIMarkers != null)
		{
			List<Prefab.Marker> list2 = prefab.RotatePOIMarkers(_bLeft: true, num);
			for (int num6 = list2.Count - 1; num6 >= 0; num6--)
			{
				if (list2[num6].MarkerType != Prefab.Marker.MarkerTypes.RoadExit)
				{
					list2.RemoveAt(num6);
				}
			}
			if (list2.Count > 0)
			{
				int index = rndm.RandomRange(0, list2.Count);
				float pathRadius = (float)Utils.FastMax(list2[index].Size.x, list2[index].Size.z) * 0.5f;
				Vector3i start = list2[index].Start;
				Vector2 vector = new Vector2((float)start.x + (float)list2[index].Size.x / 2f, (float)start.z + (float)list2[index].Size.z / 2f);
				Vector2 vector2 = new Vector2((float)wildernessPOIPos.x + vector.x, (float)wildernessPOIPos.y + vector.y);
				worldBuilder.WildernessPlanner.AddPathInfo(new Vector2i(vector2), pathRadius);
			}
		}
		int y = num4 + prefab.yOffset;
		SpawnMarkerPartsAndPrefabsWilderness(prefab, new Vector3i(wildernessPOIPos.x, y, wildernessPOIPos.y), (byte)num);
		PrefabDataInstance pdi = new PrefabDataInstance(id, new Vector3i(vector3i.x, y, vector3i.z), (byte)num, prefab);
		AddPrefab(pdi);
		worldBuilder.WildernessPrefabCount++;
		WildernessPOIPos = wildernessPOIPos;
		WildernessPOICenter = wildernessPOICenter;
		WildernessPOISize.x = num2;
		WildernessPOISize.y = num3;
		WildernessPOIHeight = num4;
		int num7 = wildernessPOIPos.x / 10;
		int num8 = vector2i.x / 10;
		int num9 = wildernessPOIPos.y / 10;
		int num10 = vector2i.y / 10;
		int num11 = num7 - 2;
		int num12 = num8 + 2;
		int num13 = num9 - 2;
		int num14 = num10 + 2;
		for (int k = num13; k <= num14; k++)
		{
			if (k < 0 || k >= worldBuilder.data.PathTileGridWidth)
			{
				continue;
			}
			for (int l = num11; l <= num12; l++)
			{
				if (l >= 0 && l < worldBuilder.data.PathTileGridWidth)
				{
					if (l >= num7 && l <= num8 && k >= num9 && k <= num10)
					{
						worldBuilder.PathingUtils.SetPathBlocked(l, k, isBlocked: true);
					}
					else if (l == num11 || l == num12 || k == num13 || k == num14)
					{
						worldBuilder.PathingUtils.SetPathBlocked(l, k, 2);
					}
					else
					{
						worldBuilder.PathingUtils.SetPathBlocked(l, k, 5);
					}
				}
			}
		}
		num11 = wildernessPOIPos.x;
		num12 = vector2i.x;
		num13 = wildernessPOIPos.y;
		num14 = vector2i.y;
		for (int m = num13; m <= num14; m += 150)
		{
			for (int n = num11; n <= num12; n += 150)
			{
				StreetTile streetTileWorld = worldBuilder.GetStreetTileWorld(n, m);
				if (streetTileWorld != null)
				{
					streetTileWorld.Used = true;
				}
			}
		}
		return true;
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
	public int getHeightCeil(int x, int y)
	{
		return Mathf.CeilToInt(worldBuilder.GetHeight(x, y));
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
		StreetTile[] neighbors = GetNeighbors();
		for (int i = 0; i < neighbors.Length; i++)
		{
			if (neighbors[i].Township == Township)
			{
				num++;
			}
		}
		return num;
	}
}
