using System.Collections.Generic;
using UnityEngine;

namespace WorldGenerationEngineFinal;

public class Township
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int BUFFER_DISTANCE = 300;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly WorldBuilder worldBuilder;

	public int ID;

	public BiomeType BiomeType;

	public Rect Area;

	public Rect BufferArea;

	[PublicizedFrom(EAccessModifier.Private)]
	public StreetTile commercialCap;

	[PublicizedFrom(EAccessModifier.Private)]
	public StreetTile ruralCap;

	[PublicizedFrom(EAccessModifier.Private)]
	public int type;

	public Vector2i GridCenter;

	public Dictionary<Vector2i, StreetTile> Streets = new Dictionary<Vector2i, StreetTile>();

	public List<PrefabDataInstance> Prefabs = new List<PrefabDataInstance>();

	public List<StreetTile> Gateways = new List<StreetTile>();

	public Dictionary<Township, int> TownshipConnectionCounts = new Dictionary<Township, int>();

	public GameRandom rand;

	[PublicizedFrom(EAccessModifier.Private)]
	public string typeName;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Vector2i> list = new List<Vector2i>();

	public int Type
	{
		get
		{
			return type;
		}
		set
		{
			type = value;
			typeName = TownshipStatic.NamesByType[type];
		}
	}

	public Township(WorldBuilder _worldBuilder)
	{
		worldBuilder = _worldBuilder;
	}

	public void Cleanup()
	{
		GameRandomManager.Instance.FreeGameRandom(rand);
	}

	public string GetTypeName()
	{
		if (typeName == null)
		{
			typeName = TownshipStatic.NamesByType[Type];
		}
		return typeName;
	}

	public bool IsBig()
	{
		return type == TownshipStatic.TypesByName["citybig"];
	}

	public void SortGatewaysClockwise()
	{
		Gateways.Sort([PublicizedFrom(EAccessModifier.Private)] (StreetTile _t1, StreetTile _t2) =>
		{
			float num = Mathf.Atan2(_t1.GridPosition.y - GridCenter.y, _t1.GridPosition.x - GridCenter.x);
			float value = Mathf.Atan2(_t2.GridPosition.y - GridCenter.y, _t2.GridPosition.x - GridCenter.x);
			return num.CompareTo(value);
		});
	}

	public void CleanupStreets()
	{
		if (Streets == null || Streets.Count == 0)
		{
			Log.Error("No Streets!");
			return;
		}
		rand = GameRandomManager.Instance.CreateGameRandom(worldBuilder.Seed + ID + Streets.Count);
		foreach (StreetTile value in Streets.Values)
		{
			if (value.District == null || value.District.type == District.Type.Gateway)
			{
				continue;
			}
			int num = 0;
			if (commercialCap == null && value.District.type == District.Type.Commercial)
			{
				for (int i = 0; i < worldBuilder.TownshipShared.dir4way.Length; i++)
				{
					StreetTile neighbor = value.GetNeighbor(worldBuilder.TownshipShared.dir4way[i]);
					if (neighbor != null && neighbor.District == value.District)
					{
						num++;
					}
					if (neighbor != null && neighbor == ruralCap)
					{
						num += 2;
					}
				}
				if (num != 1)
				{
					continue;
				}
				for (int j = 0; j < worldBuilder.TownshipShared.dir4way.Length; j++)
				{
					StreetTile neighbor2 = value.GetNeighbor(worldBuilder.TownshipShared.dir4way[j]);
					if (neighbor2 != null && neighbor2.District == value.District)
					{
						value.SetExitUsed(value.getHighwayExitPosition(j));
					}
					else
					{
						value.SetExitUnUsed(value.getHighwayExitPosition(j));
					}
				}
				commercialCap = value;
				continue;
			}
			if (ruralCap == null && value.District.type == District.Type.Rural)
			{
				for (int k = 0; k < worldBuilder.TownshipShared.dir4way.Length; k++)
				{
					StreetTile neighbor3 = value.GetNeighbor(worldBuilder.TownshipShared.dir4way[k]);
					if (neighbor3 != null && neighbor3.District == value.District)
					{
						num++;
					}
					if (neighbor3 != null && neighbor3 == commercialCap)
					{
						num += 2;
					}
				}
				if (num < 1 || num > 2)
				{
					continue;
				}
				bool flag = false;
				for (int l = 0; l < worldBuilder.TownshipShared.dir4way.Length; l++)
				{
					StreetTile neighbor4 = value.GetNeighbor(worldBuilder.TownshipShared.dir4way[l]);
					if (!flag && neighbor4 != null && neighbor4.District == value.District)
					{
						value.SetExitUsed(value.getHighwayExitPosition(l));
						flag = true;
					}
					else
					{
						value.SetExitUnUsed(value.getHighwayExitPosition(l));
					}
				}
				ruralCap = value;
				continue;
			}
			for (int m = 0; m < worldBuilder.TownshipShared.dir4way.Length; m++)
			{
				StreetTile neighbor5 = value.GetNeighbor(worldBuilder.TownshipShared.dir4way[m]);
				if (neighbor5 != null && neighbor5.District != null && neighbor5.District.type == District.Type.Gateway)
				{
					num++;
				}
			}
			if (num < 1)
			{
				continue;
			}
			for (int n = 0; n < worldBuilder.TownshipShared.dir4way.Length; n++)
			{
				StreetTile neighbor6 = value.GetNeighbor(worldBuilder.TownshipShared.dir4way[n]);
				if (neighbor6 != null && (neighbor6.District == value.District || neighbor6.District == DistrictPlannerStatic.Districts["gateway"]))
				{
					value.SetExitUsed(value.getHighwayExitPosition(n));
				}
			}
		}
		cleanupLessThan();
		cleanupGreaterThan();
		cleanupNotEqual();
		cleanupLessThan();
		cleanupGreaterThan();
		cleanupNotEqual();
		int num2 = int.MaxValue;
		int num3 = int.MaxValue;
		int num4 = int.MinValue;
		int num5 = int.MinValue;
		foreach (StreetTile value2 in Streets.Values)
		{
			num2 = Utils.FastMin(num2, value2.WorldPosition.x);
			num3 = Utils.FastMin(num3, value2.WorldPosition.y);
			num4 = Utils.FastMax(num4, value2.WorldPositionMax.x);
			num5 = Utils.FastMax(num5, value2.WorldPositionMax.y);
		}
		Area = new Rect(num2, num3, num4 - num2, num5 - num3);
		BufferArea = new Rect(Area.xMin - 150f, Area.yMin - 150f, Area.width + 300f, Area.height + 300f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void cleanupLessThan()
	{
		foreach (StreetTile value in Streets.Values)
		{
			int roadExitCount = value.RoadExitCount;
			int neighborExitCount = GetNeighborExitCount(value);
			if (value.District.name == "gateway" || value == ruralCap || value == commercialCap || roadExitCount >= neighborExitCount)
			{
				continue;
			}
			for (int i = 0; i < worldBuilder.StreetTileShared.RoadShapeExitCounts.Count; i++)
			{
				if (worldBuilder.StreetTileShared.RoadShapeExitCounts[i] != neighborExitCount)
				{
					continue;
				}
				for (int j = 0; j < worldBuilder.TownshipShared.dir4way.Length; j++)
				{
					StreetTile neighbor = value.GetNeighbor(worldBuilder.TownshipShared.dir4way[j]);
					if (neighbor.Township != value.Township || !neighbor.HasExitTo(value))
					{
						value.SetExitUnUsed(value.getHighwayExitPosition(j));
					}
					else
					{
						value.SetExitUsed(value.getHighwayExitPosition(j));
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void cleanupGreaterThan()
	{
		foreach (StreetTile value in Streets.Values)
		{
			int roadExitCount = value.RoadExitCount;
			int neighborExitCount = GetNeighborExitCount(value);
			if (value.District.name == "gateway" || value == ruralCap || value == commercialCap || roadExitCount <= neighborExitCount)
			{
				continue;
			}
			for (int i = 0; i < worldBuilder.StreetTileShared.RoadShapeExitCounts.Count; i++)
			{
				if (worldBuilder.StreetTileShared.RoadShapeExitCounts[i] != neighborExitCount)
				{
					continue;
				}
				for (int j = 0; j < worldBuilder.TownshipShared.dir4way.Length; j++)
				{
					StreetTile neighbor = value.GetNeighbor(worldBuilder.TownshipShared.dir4way[j]);
					if (neighbor.Township != value.Township || !neighbor.HasExitTo(value))
					{
						value.SetExitUnUsed(value.getHighwayExitPosition(j));
					}
					else
					{
						value.SetExitUsed(value.getHighwayExitPosition(j));
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void cleanupNotEqual()
	{
		foreach (StreetTile value in Streets.Values)
		{
			int roadExitCount = value.RoadExitCount;
			int neighborExitCount = GetNeighborExitCount(value);
			if (value.District.name == "gateway" || value == ruralCap || value == commercialCap || roadExitCount == neighborExitCount)
			{
				continue;
			}
			for (int i = 0; i < worldBuilder.StreetTileShared.RoadShapeExitCounts.Count; i++)
			{
				if (worldBuilder.StreetTileShared.RoadShapeExitCounts[i] != neighborExitCount)
				{
					continue;
				}
				for (int j = 0; j < worldBuilder.TownshipShared.dir4way.Length; j++)
				{
					StreetTile neighbor = value.GetNeighbor(worldBuilder.TownshipShared.dir4way[j]);
					if (neighbor.Township != value.Township || !neighbor.HasExitTo(value))
					{
						value.SetExitUnUsed(value.getHighwayExitPosition(j));
					}
					else
					{
						value.SetExitUsed(value.getHighwayExitPosition(j));
					}
				}
			}
		}
	}

	public void SpawnPrefabs()
	{
		foreach (StreetTile value in Streets.Values)
		{
			if (value == null)
			{
				Log.Error("WorldTileData is null, this shouldn't happen!");
			}
			else
			{
				value.SpawnPrefabs();
			}
		}
		Prefabs.Clear();
	}

	public void AddToUsedPOIList(string name)
	{
		worldBuilder.PrefabManager.AddUsedPrefab(name);
	}

	public List<Vector2i> GetUnusedTownExits(int _gatewayUnusedMax = 4)
	{
		list.Clear();
		if (WorldBuilderStatic.townshipDatas[GetTypeName()].SpawnGateway)
		{
			foreach (StreetTile gateway in Gateways)
			{
				if (gateway.UsedExitList.Count > _gatewayUnusedMax)
				{
					continue;
				}
				foreach (Vector2i highwayExit in gateway.GetHighwayExits(isGateway: true))
				{
					list.Add(highwayExit);
				}
			}
		}
		else
		{
			foreach (StreetTile value in Streets.Values)
			{
				foreach (Vector2i highwayExit2 in value.GetHighwayExits())
				{
					list.Add(highwayExit2);
				}
			}
		}
		return list;
	}

	public void AddPrefab(PrefabDataInstance pdi)
	{
		Prefabs.Add(pdi);
		worldBuilder.PrefabManager.AddUsedPrefabWorld(ID, pdi);
	}

	public List<Vector2i> GetTownExits()
	{
		list.Clear();
		foreach (StreetTile gateway in Gateways)
		{
			foreach (Vector2i highwayExit in gateway.GetHighwayExits(isGateway: true))
			{
				list.Add(highwayExit);
			}
		}
		return list;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetNeighborExitCount(StreetTile current)
	{
		int num = 0;
		int num2 = -1;
		StreetTile[] neighbors = current.GetNeighbors();
		foreach (StreetTile streetTile in neighbors)
		{
			num2++;
			if (streetTile != null && streetTile.District != null && streetTile.Township != null)
			{
				bool flag = current.District.name == "highway";
				bool flag2 = current.District.name == "gateway";
				bool flag3 = streetTile.District.name == "highway";
				bool flag4 = streetTile.District.name == "gateway";
				if ((streetTile.Township == current.Township || ((flag || flag4 || flag2 || flag3) && (!flag2 || flag3) && (!flag || flag4))) && (streetTile.RoadExits[(num2 + 2) & 3] || (flag && flag3)))
				{
					num++;
				}
			}
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool[] GetNeighborExits(StreetTile current)
	{
		bool[] array = new bool[4];
		int num = -1;
		StreetTile[] neighbors = current.GetNeighbors();
		foreach (StreetTile streetTile in neighbors)
		{
			num++;
			if (streetTile != null && streetTile.District != null && streetTile.Township != null)
			{
				bool flag = current.District.name == "highway";
				bool flag2 = current.District.name == "gateway";
				bool flag3 = streetTile.District.name == "highway";
				bool flag4 = streetTile.District.name == "gateway";
				if ((streetTile.Township == current.Township || ((flag || flag4 || flag2 || flag3) && (!flag2 || flag3) && (!flag || flag4))) && (streetTile.HasExitTo(current) || (flag && flag3)))
				{
					array[num] = true;
				}
			}
		}
		return array;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetNeighborCount(Vector2i current)
	{
		int num = 0;
		for (int i = 0; i < worldBuilder.TownshipShared.dir4way.Length; i++)
		{
			Vector2i key = current + worldBuilder.TownshipShared.dir4way[i];
			if (Streets.ContainsKey(key))
			{
				num++;
			}
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetCurrentExitCount(Vector2i current)
	{
		int num = 0;
		for (int i = 0; i < Streets[current].RoadExits.Length; i++)
		{
			if (Streets[current].RoadExits[i])
			{
				num++;
			}
		}
		return num;
	}

	public override string ToString()
	{
		return $"Township {ID} {TownshipStatic.NamesByType[Type]}";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool HasExitWhenRotated(int hasThisDirExit, StreetTile.PrefabRotations _rots, bool[] _exits)
	{
		bool[] array = new bool[4];
		switch (_rots)
		{
		case StreetTile.PrefabRotations.None:
			array[0] = _exits[0];
			array[1] = _exits[1];
			array[2] = _exits[2];
			array[3] = _exits[3];
			break;
		case StreetTile.PrefabRotations.One:
			array[0] = _exits[3];
			array[1] = _exits[0];
			array[2] = _exits[1];
			array[3] = _exits[2];
			break;
		case StreetTile.PrefabRotations.Two:
			array[0] = _exits[2];
			array[1] = _exits[3];
			array[2] = _exits[0];
			array[3] = _exits[1];
			break;
		case StreetTile.PrefabRotations.Three:
			array[0] = _exits[1];
			array[1] = _exits[2];
			array[2] = _exits[3];
			array[3] = _exits[0];
			break;
		}
		return array[hasThisDirExit];
	}
}
