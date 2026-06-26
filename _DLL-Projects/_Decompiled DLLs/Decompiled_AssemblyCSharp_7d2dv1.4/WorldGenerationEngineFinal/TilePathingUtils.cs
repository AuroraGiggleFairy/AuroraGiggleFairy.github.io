using System.Collections.Generic;
using UnityEngine;

namespace WorldGenerationEngineFinal;

public class TilePathingUtils
{
	public class PathNode
	{
		public Vector2i position;

		public int pathCost;

		public PathNode next;

		public PathNode nextListElem;

		public PathNode(Vector2i position, int pathCost, PathNode next)
		{
			this.position = position;
			this.pathCost = pathCost;
			this.next = next;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class MinHeap
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public PathNode listHead;

		public bool HasNext()
		{
			return listHead != null;
		}

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
			PathNode nextListElem = listHead;
			while (nextListElem.nextListElem != null && nextListElem.nextListElem.pathCost < item.pathCost)
			{
				nextListElem = nextListElem.nextListElem;
			}
			item.nextListElem = nextListElem.nextListElem;
			nextListElem.nextListElem = item;
		}

		public PathNode ExtractFirst()
		{
			PathNode result = listHead;
			listHead = listHead.nextListElem;
			return result;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly WorldBuilder worldBuilder;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<Vector2i> dir4way = new List<Vector2i>
	{
		new Vector2i(0, 1),
		new Vector2i(-1, 0),
		new Vector2i(1, 0),
		new Vector2i(0, -1)
	};

	public TilePathingUtils(WorldBuilder _worldBuilder)
	{
		worldBuilder = _worldBuilder;
	}

	public List<StreetTile> CreatePath(StreetTile start, StreetTile end, Vector2i dir)
	{
		PathNode pathNode = FindPath(start, end, dir);
		List<StreetTile> list = new List<StreetTile>();
		while (pathNode != null)
		{
			StreetTile streetTileGrid = worldBuilder.GetStreetTileGrid(pathNode.position);
			list.Add(streetTileGrid);
			pathNode = pathNode.next;
		}
		return list;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public PathNode FindPath(StreetTile start, StreetTile end, Vector2i dir)
	{
		MinHeap minHeap = new MinHeap();
		minHeap.Add(new PathNode(start.GridPosition, 0, null));
		bool[,] array = new bool[worldBuilder.StreetTileMap.GetLength(0), worldBuilder.StreetTileMap.GetLength(1)];
		array[start.GridPosition.x, start.GridPosition.y] = true;
		PathNode pathNode = null;
		while (minHeap.HasNext())
		{
			pathNode = minHeap.ExtractFirst();
			Vector2i position = pathNode.position;
			if (position == end.GridPosition)
			{
				return pathNode;
			}
			StreetTile streetTileGrid = worldBuilder.GetStreetTileGrid(position.x, position.y);
			if (streetTileGrid == null)
			{
				continue;
			}
			StreetTile[] neighbors = streetTileGrid.GetNeighbors();
			foreach (StreetTile streetTile in neighbors)
			{
				if (streetTile == null || array[streetTile.GridPosition.x, streetTile.GridPosition.y] || streetTile.OverlapsRadiation || streetTile.OverlapsWater || streetTile.HasSteepSlope || Mathf.CeilToInt(Mathf.Abs(streetTileGrid.PositionHeight - streetTile.PositionHeight)) > 10 || streetTile.TerrainType == TerrainType.mountains)
				{
					continue;
				}
				bool flag = true;
				StreetTile[] neighbors2 = streetTile.GetNeighbors();
				for (int j = 0; j < neighbors2.Length; j++)
				{
					if (neighbors2[j].TerrainType == TerrainType.mountains)
					{
						flag = false;
						break;
					}
				}
				if (flag && (streetTile.Township == null || !(streetTile.District.name != "highway")))
				{
					int num = distanceSqr(streetTile.WorldPosition, end.WorldPosition);
					if (streetTile.District != null && streetTile.District.name == "highway")
					{
						num /= 5;
					}
					int pathCost = pathNode.pathCost + num;
					minHeap.Add(new PathNode(streetTile.GridPosition, pathCost, pathNode));
					array[streetTile.GridPosition.x, streetTile.GridPosition.y] = true;
				}
			}
		}
		Log.Error("Could not find path, outputting what WAS found for testing. \n Desired Start Position {0} \n Desired End Position {1}", start.GridPosition.ToString(), end.GridPosition.ToString());
		return pathNode;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int distanceSqr(Vector2i pointA, Vector2i pointB)
	{
		Vector2i vector2i = pointA - pointB;
		return vector2i.x * vector2i.x + vector2i.y * vector2i.y;
	}
}
