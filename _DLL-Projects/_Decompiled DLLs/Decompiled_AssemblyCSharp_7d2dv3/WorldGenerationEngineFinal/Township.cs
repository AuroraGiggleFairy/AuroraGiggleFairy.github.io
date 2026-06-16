using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace WorldGenerationEngineFinal;

public class Township
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int BUFFER_DISTANCE = 300;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly WorldBuilder worldBuilder;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly TownshipData townshipData;

	public int ID;

	public BiomeType BiomeType;

	public Rect Area;

	public Rect BufferArea;

	[PublicizedFrom(EAccessModifier.Private)]
	public StreetTile commercialCap;

	[PublicizedFrom(EAccessModifier.Private)]
	public StreetTile ruralCap;

	public Vector2i GridCenter;

	public int Height;

	[PublicizedFrom(EAccessModifier.Private)]
	public StreetTile centerMostTile;

	public Dictionary<Vector2i, StreetTile> Streets = new Dictionary<Vector2i, StreetTile>();

	public List<PrefabDataInstance> Prefabs = new List<PrefabDataInstance>();

	public List<StreetTile> Gateways = new List<StreetTile>();

	public Dictionary<Township, int> TownshipConnectionCounts = new Dictionary<Township, int>();

	public GameRandom rand;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Vector2i> list = new List<Vector2i>();

	public TownshipData Data => townshipData;

	public Township(WorldBuilder _worldBuilder, TownshipData _townshipData)
	{
		worldBuilder = _worldBuilder;
		townshipData = _townshipData;
	}

	public void Cleanup()
	{
		GameRandomManager.Instance.FreeGameRandom(rand);
	}

	public string GetTypeName()
	{
		return townshipData.Name;
	}

	public bool IsBig()
	{
		return townshipData.Name == "citybig";
	}

	public bool IsRoadside()
	{
		return townshipData.Category == TownshipData.eCategory.Roadside;
	}

	public bool IsRural()
	{
		return townshipData.Category == TownshipData.eCategory.Rural;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsWilderness()
	{
		return townshipData.Category == TownshipData.eCategory.Wilderness;
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
			Log.Error("CleanupStreets none!");
			return;
		}
		rand = GameRandomManager.Instance.CreateGameRandom(worldBuilder.Seed + ID + Streets.Count);
		int num = int.MaxValue;
		int num2 = int.MaxValue;
		int num3 = int.MinValue;
		int num4 = int.MinValue;
		foreach (StreetTile value in Streets.Values)
		{
			num = Utils.FastMin(num, value.WorldPosition.x);
			num2 = Utils.FastMin(num2, value.WorldPosition.y);
			num3 = Utils.FastMax(num3, value.WorldPositionMax.x);
			num4 = Utils.FastMax(num4, value.WorldPositionMax.y);
		}
		Area = new Rect(num, num2, num3 - num, num4 - num2);
		BufferArea = new Rect(Area.xMin - 150f, Area.yMin - 150f, Area.width + 300f, Area.height + 300f);
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

	public StreetTile CalcCenterStreetTile()
	{
		if (centerMostTile != null)
		{
			return centerMostTile;
		}
		Vector2i zero = Vector2i.zero;
		foreach (StreetTile value in Streets.Values)
		{
			zero += value.GridPosition;
		}
		zero /= Streets.Count;
		int num = int.MaxValue;
		StreetTile result = null;
		foreach (StreetTile value2 in Streets.Values)
		{
			int num2 = Vector2i.DistanceSqrInt(zero, value2.GridPosition);
			if (num2 < num)
			{
				num = num2;
				result = value2;
			}
		}
		centerMostTile = result;
		float num3 = 0f;
		Vector2i worldPositionCenter = centerMostTile.WorldPositionCenter;
		Vector2i pos = default(Vector2i);
		for (int i = -50; i <= 50; i += 50)
		{
			pos.y = worldPositionCenter.y + i;
			for (int j = -50; j <= 50; j += 50)
			{
				pos.x = worldPositionCenter.x + j;
				num3 += worldBuilder.GetHeight(pos);
			}
		}
		Height = Mathf.CeilToInt(num3 / 9f);
		Height += 3;
		if (Streets.Count > 2 && Height < 130)
		{
			if (BiomeType == BiomeType.snow)
			{
				Height += 25;
			}
			else if (BiomeType == BiomeType.wasteland)
			{
				Height += 12;
			}
		}
		return result;
	}

	public void AddToUsedPOIList(string name)
	{
		worldBuilder.PrefabManager.AddUsedPrefab(name);
	}

	public List<Vector2i> GetUnusedTownExits(int _gatewayUnusedMax = 4)
	{
		list.Clear();
		if (townshipData.SpawnGateway)
		{
			foreach (StreetTile gateway in Gateways)
			{
				if (gateway.UsedExitList.Count > _gatewayUnusedMax)
				{
					continue;
				}
				foreach (Vector2i highwayExit in gateway.GetHighwayExits(_isGateway: true))
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

	public override string ToString()
	{
		return $"Township {townshipData.Name}, {townshipData.Category}, {ID}";
	}
}
