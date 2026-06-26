using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace WorldGenerationEngineFinal;

public class PathingUtils
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class MinHeap
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public PathNode listHead;

		public void Add(PathNode item)
		{
			if (listHead == null)
			{
				listHead = item;
				return;
			}
			if (listHead.next == null && item.pathCost <= listHead.pathCost)
			{
				item.nextListElem = listHead;
				listHead = item;
				return;
			}
			PathNode pathNode = listHead;
			PathNode nextListElem = pathNode.nextListElem;
			while (nextListElem != null && nextListElem.pathCost < item.pathCost)
			{
				pathNode = nextListElem;
				nextListElem = pathNode.nextListElem;
			}
			item.nextListElem = nextListElem;
			pathNode.nextListElem = item;
		}

		public PathNode ExtractFirst()
		{
			PathNode pathNode = listHead;
			if (pathNode != null)
			{
				listHead = listHead.nextListElem;
			}
			return pathNode;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class MinHeapBinned
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly PathNode[] nodeBins;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int cBins = 32768;

		[PublicizedFrom(EAccessModifier.Private)]
		public const float cScale = 0.07f;

		[PublicizedFrom(EAccessModifier.Private)]
		public int lowBin = 32768;

		[PublicizedFrom(EAccessModifier.Private)]
		public int highBin;

		public MinHeapBinned(PathNode[] _nodeBins)
		{
			nodeBins = _nodeBins;
			if (nodeBins == null)
			{
				nodeBins = new PathNode[32768];
			}
			else
			{
				Array.Clear(nodeBins, 0, 32768);
			}
		}

		public PathNode ExtractFirst()
		{
			if (lowBin <= highBin)
			{
				PathNode pathNode = nodeBins[lowBin];
				nodeBins[lowBin] = pathNode.nextListElem;
				if (pathNode.nextListElem == null)
				{
					while (++lowBin <= highBin && nodeBins[lowBin] == null)
					{
					}
					if (lowBin > highBin)
					{
						lowBin = 32768;
						highBin = 0;
					}
				}
				return pathNode;
			}
			return null;
		}

		public void Add(PathNode item)
		{
			int num = (int)(item.pathCost * 0.07f);
			if (num >= 32768)
			{
				num = 32767;
			}
			if (num < lowBin)
			{
				lowBin = num;
			}
			if (num > highBin)
			{
				highBin = num;
			}
			PathNode pathNode = nodeBins[num];
			if (pathNode == null)
			{
				nodeBins[num] = item;
				return;
			}
			if (pathNode.next == null && item.pathCost <= pathNode.pathCost)
			{
				item.nextListElem = pathNode;
				nodeBins[num] = item;
				return;
			}
			PathNode pathNode2 = pathNode;
			PathNode nextListElem = pathNode2.nextListElem;
			while (nextListElem != null && nextListElem.pathCost < item.pathCost)
			{
				pathNode2 = nextListElem;
				nextListElem = pathNode2.nextListElem;
			}
			item.nextListElem = nextListElem;
			pathNode2.nextListElem = item;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum PathNodeType
	{
		Free = 0,
		Road = 1,
		Prefab = 2,
		CityLimits = 4,
		Blocked = 8
	}

	public const int PATHING_GRID_TILE_SIZE = 10;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cRoadCountryMaxStepH = 6.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cRoadHighwayMaxStepH = 11f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cHeightCostScale = 10f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cNormalNeighborsCount = 8;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Vector2i[] normalNeighbors = new Vector2i[8]
	{
		new Vector2i(0, 1),
		new Vector2i(1, 1),
		new Vector2i(1, 0),
		new Vector2i(1, -1),
		new Vector2i(0, -1),
		new Vector2i(-1, -1),
		new Vector2i(-1, 0),
		new Vector2i(-1, 1)
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Vector2i[] normalNeighbors4way = new Vector2i[4]
	{
		new Vector2i(0, 1),
		new Vector2i(1, 0),
		new Vector2i(0, -1),
		new Vector2i(-1, 0)
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly WorldBuilder worldBuilder;

	[PublicizedFrom(EAccessModifier.Private)]
	public sbyte[] pathingGrid;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i pathTilePosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Vector2i> path = new List<Vector2i>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool[,] cachedClosedList;

	[PublicizedFrom(EAccessModifier.Private)]
	public PathNodePool nodePool = new PathNodePool(100000);

	[PublicizedFrom(EAccessModifier.Private)]
	public PathNode[] nodeBins;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i wPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public int pathingGridSize;

	public PathingUtils(WorldBuilder _worldBuilder)
	{
		worldBuilder = _worldBuilder;
	}

	public bool HasValidPath(Vector2i start, Vector2i end, bool isCountryRoad = false)
	{
		Vector2i boundsMin = new Vector2i(Mathf.Min(start.x, end.x), Mathf.Min(start.y, end.y)) / 10;
		boundsMin.x = Mathf.Max(boundsMin.x - 15, 0);
		boundsMin.y = Mathf.Max(boundsMin.y - 15, 0);
		Vector2i boundsMax = new Vector2i(Mathf.Max(start.x, end.x), Mathf.Max(start.y, end.y)) / 10;
		boundsMax.x = Mathf.Min(boundsMax.x + 15, worldBuilder.WorldSize / 10 - 1);
		boundsMax.y = Mathf.Min(boundsMax.y + 15, worldBuilder.WorldSize / 10 - 1);
		bool result = FindDetailedPath(start / 10, end / 10, isCountryRoad, isRiver: false, boundsMin, boundsMax) != null;
		nodePool.ReturnAll();
		return result;
	}

	public int GetPathCost(Vector2i start, Vector2i end, bool isCountryRoad = false)
	{
		PathNode pathNode = FindDetailedPath(start / 10, end / 10, isCountryRoad);
		int num = 0;
		while (pathNode != null)
		{
			num++;
			pathNode = pathNode.next;
		}
		nodePool.ReturnAll();
		return num;
	}

	public List<Vector2i> GetPath(Vector2i start, Vector2i end, bool isCountryRoad)
	{
		PathNode pathNode = FindDetailedPath(start / 10, end / 10, isCountryRoad, isRiver: false, new Vector2i(Mathf.Min(start.x, end.x), Mathf.Min(start.y, end.y)) / 10, new Vector2i(Mathf.Max(start.x, end.x), Mathf.Max(start.y, end.y)) / 10);
		path.Clear();
		while (pathNode != null)
		{
			pathTilePosition = pathNode.position * 10 + PathNode.offset;
			path.Add(pathTilePosition);
			pathNode = pathNode.next;
		}
		nodePool.ReturnAll();
		return path;
	}

	public List<Vector2i> GetPath(Path p, Vector2i start, Vector2i end)
	{
		PathNode pathNode = FindDetailedPath(start / 10, end / 10, p.isCountryRoad, p.isRiver, new Vector2i(Mathf.Min(start.x, end.x), Mathf.Min(start.y, end.y)) / 10, new Vector2i(Mathf.Max(start.x, end.x), Mathf.Max(start.y, end.y)) / 10);
		path.Clear();
		while (pathNode != null)
		{
			pathTilePosition = pathNode.position * 10 + PathNode.offset;
			path.Add(pathTilePosition);
			pathNode = pathNode.next;
		}
		nodePool.ReturnAll();
		return path;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public PathNode FindDetailedPath(Vector2i startPos, Vector2i endPos, bool _isCountryRoad, bool isRiver, Vector2i boundsMin, Vector2i boundsMax, int padding = 200)
	{
		int num = worldBuilder.WorldSize / 10 + 1;
		if (cachedClosedList == null || cachedClosedList.GetLength(0) != num)
		{
			cachedClosedList = new bool[num, num];
		}
		MinHeapBinned minHeapBinned = new MinHeapBinned(nodeBins);
		bool[,] array = cachedClosedList;
		Array.Clear(array, 0, array.Length);
		PathNode pathNode = nodePool.Alloc();
		pathNode.Set(startPos, 0f, null);
		minHeapBinned.Add(pathNode);
		array[startPos.x, startPos.y] = true;
		if (!InBounds(startPos))
		{
			return null;
		}
		if (!InBounds(endPos))
		{
			return null;
		}
		int num2 = Mathf.Max(0, boundsMin.x - padding);
		int num3 = Mathf.Max(0, boundsMin.y - padding);
		int num4 = Mathf.Min(boundsMax.x + padding, array.GetLength(0) - 1);
		int num5 = Mathf.Min(boundsMax.y + padding, array.GetLength(1) - 1);
		float num6 = (_isCountryRoad ? 6.5f : 11f);
		PathNode pathNode2;
		while ((pathNode2 = minHeapBinned.ExtractFirst()) != null)
		{
			Vector2i position = pathNode2.position;
			if (position == endPos)
			{
				return pathNode2;
			}
			for (int i = 0; i < 8; i++)
			{
				Vector2i vector2i = normalNeighbors[i];
				Vector2i vector2i2 = pathNode2.position + vector2i;
				if (vector2i2.x < num2 || vector2i2.y < num3 || vector2i2.x >= num4 || vector2i2.y >= num5 || array[vector2i2.x, vector2i2.y])
				{
					continue;
				}
				if (vector2i2 != endPos && vector2i2 != startPos && IsBlocked(vector2i2.x, vector2i2.y, isRiver))
				{
					array[vector2i2.x, vector2i2.y] = true;
					continue;
				}
				float num7 = Utils.FastAbs(GetHeight(position) - GetHeight(vector2i2));
				if (num7 > num6)
				{
					continue;
				}
				num7 *= 10f;
				float num8 = Vector2i.Distance(vector2i2, endPos) + num7;
				if (!_isCountryRoad)
				{
					StreetTile streetTileWorld = worldBuilder.GetStreetTileWorld(vector2i2 * 10);
					if (streetTileWorld != null && streetTileWorld.ContainsHighway)
					{
						if (streetTileWorld.ConnectedHighways.Count > 2)
						{
							continue;
						}
						if ((vector2i2.x != endPos.x || vector2i2.y != endPos.y) && (vector2i2.x != startPos.x || vector2i2.y != startPos.y))
						{
							PathTile pathTile = worldBuilder.PathingGrid[vector2i2.x, vector2i2.y];
							bool flag = pathTile != null && pathTile.TileState == PathTile.PathTileStates.Highway;
							if (vector2i.x != 0 && vector2i.y != 0)
							{
								for (int j = 0; j < 2; j++)
								{
									Vector2i vector2i3 = j switch
									{
										0 => new Vector2i(-vector2i.x, 0), 
										1 => new Vector2i(0, -vector2i.y), 
										_ => throw new IndexOutOfRangeException("FindDetailedPath direction loop iterating past defined Vectors"), 
									};
									if (IsBlocked(vector2i2.x + vector2i3.x, vector2i2.y + vector2i3.y))
									{
										flag = true;
										continue;
									}
									PathTile pathTile2 = worldBuilder.PathingGrid[vector2i2.x + vector2i3.x, vector2i2.y + vector2i3.y];
									if (pathTile2 != null && pathTile2.TileState == PathTile.PathTileStates.Highway)
									{
										flag = true;
									}
								}
							}
							if (flag)
							{
								continue;
							}
						}
						num8 *= 2f;
					}
				}
				if (vector2i.x != 0 && vector2i.y != 0)
				{
					num8 *= 1.2f;
				}
				if (pathingGrid != null)
				{
					int num9 = pathingGrid[vector2i2.x + vector2i2.y * pathingGridSize];
					if (num9 > 0)
					{
						num8 *= (float)num9;
					}
				}
				array[vector2i2.x, vector2i2.y] = true;
				PathNode pathNode3 = nodePool.Alloc();
				pathNode3.Set(vector2i2, pathNode2.pathCost + num8, pathNode2);
				minHeapBinned.Add(pathNode3);
			}
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public PathNode FindDetailedPath(Vector2i startIndex, Vector2i endIndex, bool isCountryRoad, bool isRiver = false)
	{
		return FindDetailedPath(startIndex, endIndex, isCountryRoad, isRiver, Vector2i.zero, Vector2i.one * (worldBuilder.WorldSize / 10 + 1));
	}

	public bool IsBlocked(int pathX, int pathY, bool isRiver = false)
	{
		if (IsPathBlocked(pathX, pathY))
		{
			return true;
		}
		Vector2i vector2i = pathPositionToWorldCenter(pathX, pathY);
		if (!InWorldBounds(vector2i.x, vector2i.y))
		{
			return true;
		}
		StreetTile streetTileWorld = worldBuilder.GetStreetTileWorld(vector2i.x, vector2i.y);
		if (InCityLimits(streetTileWorld))
		{
			return true;
		}
		if (IsRadiation(streetTileWorld))
		{
			return true;
		}
		if (!isRiver && IsWater(pathX, pathY))
		{
			return true;
		}
		return false;
	}

	public bool InBounds(Vector2i pos)
	{
		return InBounds(pos.x, pos.y);
	}

	public bool InBounds(int pathX, int pathY)
	{
		Vector2i vector2i = pathPositionToWorldCenter(pathX, pathY);
		if ((uint)vector2i.x < worldBuilder.WorldSize)
		{
			return (uint)vector2i.y < worldBuilder.WorldSize;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool InWorldBounds(int x, int y)
	{
		if ((uint)x < worldBuilder.WorldSize)
		{
			return (uint)y < worldBuilder.WorldSize;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsRadiation(StreetTile st)
	{
		if (st == null || st.OverlapsRadiation)
		{
			return worldBuilder.GetRad(wPos.x, wPos.y) > 0;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool InCityLimits(StreetTile st)
	{
		if (st != null && st.Township != null && st.Township.Type != TownshipStatic.TypesByName["wilderness"])
		{
			return true;
		}
		return false;
	}

	public bool IsWater(Vector2i pos)
	{
		return IsWater(pos.x, pos.y);
	}

	public bool IsWater(int pathX, int pathY)
	{
		Vector2i pos = pathPositionToWorldMin(pathX, pathY);
		StreetTile streetTileWorld = worldBuilder.GetStreetTileWorld(pos);
		if (streetTileWorld == null || streetTileWorld.OverlapsWater)
		{
			for (int i = pos.y; i < pos.y + 10; i++)
			{
				for (int j = pos.x; j < pos.x + 10; j++)
				{
					if (worldBuilder.GetWater(j, i) > 0)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float GetHeight(Vector2i pos)
	{
		return GetHeight(pos.x, pos.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float GetHeight(int pathX, int pathY)
	{
		return worldBuilder.GetHeight(pathPositionToWorldCenter(pathX, pathY));
	}

	public BiomeType GetBiome(Vector2i pos)
	{
		return GetBiome(pos.x, pos.y);
	}

	public BiomeType GetBiome(int pathX, int pathY)
	{
		return worldBuilder.GetBiome(pathPositionToWorldCenter(pathX, pathY));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i pathPositionToWorldCenter(int pathX, int pathY)
	{
		wPos.x = pathX * 10 + 5;
		wPos.y = pathY * 10 + 5;
		return wPos;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i pathPositionToWorldMin(int pathX, int pathY)
	{
		wPos.x = pathX * 10;
		wPos.y = pathY * 10;
		return wPos;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i pathPositionToWorldMax(int pathX, int pathY)
	{
		return new Vector2i(pathX * 10 + 10, pathY * 10 + 10);
	}

	public void AddPrefabRect(Rect r)
	{
		for (int i = (int)r.yMin; (float)i < r.yMax; i += 10)
		{
			for (int j = (int)r.xMin; (float)j < r.xMax; j += 10)
			{
				SetPathBlocked(j / 10, i / 10, isBlocked: true);
			}
		}
	}

	public void AddMoveLimitArea(Rect r)
	{
		int num = (int)r.xMin;
		int num2 = (int)r.yMin;
		num /= 10;
		num2 /= 10;
		for (int i = 0; i < 15; i++)
		{
			for (int j = 0; j < 15; j++)
			{
				if (j != 7 && i != 7)
				{
					SetPathBlocked(num + j, num2 + i, isBlocked: true);
				}
			}
		}
	}

	public void RemoveFullyBlockedArea(Rect r)
	{
		int num = (int)r.xMin;
		int num2 = (int)r.yMin;
		num /= 10;
		num2 /= 10;
		for (int i = 0; i < 15; i++)
		{
			for (int j = 0; j < 15; j++)
			{
				SetPathBlocked(num + j, num2 + i, isBlocked: false);
			}
		}
	}

	public void AddFullyBlockedArea(Rect r)
	{
		int num = (int)(r.xMin + 0.5f);
		int num2 = (int)(r.yMin + 0.5f);
		num /= 10;
		num2 /= 10;
		for (int i = 0; i < 15; i++)
		{
			for (int j = 0; j < 15; j++)
			{
				SetPathBlocked(num + j, num2 + i, isBlocked: true);
			}
		}
	}

	public void SetPathBlocked(Vector2i pos, bool isBlocked)
	{
		SetPathBlocked(pos.x, pos.y, isBlocked);
	}

	public void SetPathBlocked(int x, int y, bool isBlocked)
	{
		SetPathBlocked(x, y, (sbyte)(isBlocked ? sbyte.MinValue : 0));
	}

	public void SetPathBlocked(int x, int y, sbyte costMult)
	{
		if (pathingGrid == null)
		{
			SetupPathingGrid();
		}
		if ((uint)x < pathingGridSize && (uint)y < pathingGridSize)
		{
			pathingGrid[x + y * pathingGridSize] = costMult;
		}
	}

	public bool IsPathBlocked(Vector2i pos)
	{
		return IsPathBlocked(pos.x, pos.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsPathBlocked(int x, int y)
	{
		if ((uint)x >= pathingGridSize || (uint)y >= pathingGridSize)
		{
			return false;
		}
		return pathingGrid[x + y * pathingGridSize] == sbyte.MinValue;
	}

	public bool IsPointOnHighwayWorld(int x, int y)
	{
		if (worldBuilder.PathingGrid[x / 10, y / 10] != null)
		{
			return worldBuilder.PathingGrid[x, y].TileState == PathTile.PathTileStates.Highway;
		}
		return false;
	}

	public bool IsPointOnCountryRoadWorld(int x, int y)
	{
		if (worldBuilder.PathingGrid[x / 10, y / 10] != null)
		{
			return worldBuilder.PathingGrid[x, y].TileState == PathTile.PathTileStates.Country;
		}
		return false;
	}

	public void SetupPathingGrid()
	{
		pathingGridSize = worldBuilder.WorldSize / 10;
		pathingGrid = new sbyte[pathingGridSize * pathingGridSize];
	}

	public void Cleanup()
	{
		cachedClosedList = null;
		pathingGrid = null;
		pathingGridSize = 0;
		nodeBins = null;
		nodePool.Cleanup();
	}
}
