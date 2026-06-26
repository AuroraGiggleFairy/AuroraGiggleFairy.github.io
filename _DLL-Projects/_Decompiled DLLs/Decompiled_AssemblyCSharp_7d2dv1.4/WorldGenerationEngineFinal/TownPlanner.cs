using System.Collections;
using System.Collections.Generic;
using UniLinq;
using UnityEngine;

namespace WorldGenerationEngineFinal;

public class TownPlanner
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class BiomeStats
	{
		public int townshipCount;

		public Dictionary<string, int> counts = new Dictionary<string, int>();
	}

	public class TownshipSpawnInfo
	{
		public int min;

		public int max;

		public int count;

		public int distance;

		public TownshipSpawnInfo(int _min, int _max, int _count, int _distance)
		{
			min = _min;
			max = _max;
			count = _count;
			distance = _distance;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cTriesPerTownshipSpawnInfo = 80;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly WorldBuilder worldBuilder;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<BiomeType, BiomeStats> biomeStats = new Dictionary<BiomeType, BiomeStats>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Vector2i[] dir4way = new Vector2i[4]
	{
		Vector2i.up,
		Vector2i.right,
		Vector2i.down,
		Vector2i.left
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Vector2i[] dir8way = new Vector2i[8]
	{
		Vector2i.up,
		Vector2i.up + Vector2i.right,
		Vector2i.right,
		Vector2i.right + Vector2i.down,
		Vector2i.down,
		Vector2i.down + Vector2i.left,
		Vector2i.left,
		Vector2i.left + Vector2i.up
	};

	public TownPlanner(WorldBuilder _worldBuilder)
	{
		worldBuilder = _worldBuilder;
	}

	public IEnumerator Plan(DynamicProperties _properties, int worldSeed)
	{
		MicroStopwatch ms = new MicroStopwatch(_bStart: true);
		Dictionary<BiomeType, List<Vector2i>> biomeStreetTiles = getStreetTilesByBiome();
		Dictionary<int, TownshipSpawnInfo> townshipSpawnInfos = new Dictionary<int, TownshipSpawnInfo>();
		getTownshipCounts(_properties, townshipSpawnInfos);
		int townID = 0;
		int roadsideId = TownshipStatic.TypesByName["roadside"];
		List<int> list = new List<int>();
		townshipSpawnInfos.CopyKeysTo(list);
		list.Sort([PublicizedFrom(EAccessModifier.Internal)] (int key1, int key2) => townshipSpawnInfos[key2].max.CompareTo(townshipSpawnInfos[key1].max));
		int rndAdd = 1982;
		GameRandom rnd = GameRandomManager.Instance.CreateGameRandom();
		foreach (int townshipTypeId in list)
		{
			if (!TownshipStatic.NamesByType.TryGetValue(townshipTypeId, out var townshipTypeName) || !WorldBuilderStatic.townshipDatas.TryGetValue(townshipTypeName, out var townshipData))
			{
				continue;
			}
			yield return worldBuilder.SetMessage(string.Format(Localization.Get("xuiRwgTownPlanning"), townshipTypeName));
			rnd.SetSeed(worldSeed + rndAdd++);
			bool flag = townshipTypeId == roadsideId;
			int num = worldBuilder.StreetTileMapSize / 10;
			if (flag)
			{
				num = worldBuilder.StreetTileMapSize / 8;
			}
			int num2 = worldBuilder.StreetTileMap.GetLength(0) - num;
			int num3 = worldBuilder.StreetTileMap.GetLength(1) - num;
			int num4 = 80;
			TownshipSpawnInfo townshipSpawnInfo = townshipSpawnInfos[townshipTypeId];
			BiomeType biomeType = BiomeType.none;
			int num5 = 0;
			for (int num6 = 0; num6 < townshipSpawnInfo.count; num6++)
			{
				if (biomeType == BiomeType.none)
				{
					biomeType = getBiomeWithMostAvailableSpace(townshipData);
				}
				List<Vector2i> list2 = null;
				BiomeType biomeType2 = biomeType;
				for (int num7 = 0; num7 < 5; num7++)
				{
					if (biomeStreetTiles.TryGetValue(biomeType, out var value))
					{
						list2 = new List<Vector2i>(value);
						for (int num8 = list2.Count - 1; num8 >= 0; num8--)
						{
							Vector2i position = list2[num8];
							if (position.x <= num || position.y <= num || position.x >= num2 || position.y >= num3 || tooClose(position, flag, townshipSpawnInfo))
							{
								list2.RemoveAt(num8);
							}
						}
						if (list2.Count > 0)
						{
							break;
						}
					}
					biomeType = nextBiomeType(biomeType, townshipData);
					if (biomeType == biomeType2)
					{
						break;
					}
				}
				if (list2 == null || list2.Count == 0)
				{
					break;
				}
				List<StreetTile> list3 = new List<StreetTile>();
				List<StreetTile> list4 = new List<StreetTile>();
				Township township = new Township(worldBuilder)
				{
					Type = townshipTypeId,
					BiomeType = biomeType
				};
				int num9 = townshipSpawnInfo.min;
				if (townshipSpawnInfo.max >= 10)
				{
					num9 = (townshipSpawnInfo.min + townshipSpawnInfo.max) / 2;
				}
				int v = rnd.RandomRange(num9, townshipSpawnInfo.max + 1);
				v = Utils.FastMax(1, v);
				if (num5 == 0)
				{
					Utils.FastMax(1, num9 - 5);
				}
				while (v >= townshipSpawnInfo.min)
				{
					int num10 = rnd.RandomRange(0, list2.Count);
					int num11 = ((!(rnd.RandomFloat < 0.5f)) ? 1 : (list2.Count - 1));
					for (int num12 = 0; num12 < list2.Count; num12++)
					{
						Vector2i startPosition = (township.GridCenter = list2[num10]);
						num10 = (num10 + num11) % list2.Count;
						for (int num13 = 0; num13 < 12; num13++)
						{
							getStreetLayout(startPosition, v, rnd, flag, list3);
							if (list3.Count >= v && (!(townshipData.OutskirtDistrictPercent > 0f) || grow(townshipData, list3, list4)))
							{
								num12 = 999999;
								v = -1;
								break;
							}
						}
					}
					v--;
				}
				if (v >= 0)
				{
					if (num4 > 0)
					{
						num4--;
						num6--;
						biomeType = nextBiomeType(biomeType, townshipData);
					}
					else
					{
						num4 = 80;
						biomeType = BiomeType.none;
					}
					continue;
				}
				if (townshipData.OutskirtDistrictPercent > 0f)
				{
					string outskirtDistrict = WorldBuilderStatic.townshipDatas[TownshipStatic.NamesByType[townshipTypeId]].OutskirtDistrict;
					foreach (StreetTile item in list4)
					{
						item.Township = township;
						item.District = DistrictPlannerStatic.Districts[outskirtDistrict];
						township.Streets[item.GridPosition] = item;
					}
				}
				foreach (StreetTile item2 in list3)
				{
					item2.Township = township;
					township.Streets[item2.GridPosition] = item2;
					item2.District = null;
				}
				worldBuilder.DistrictPlanner.PlanTownship(township);
				foreach (StreetTile value5 in township.Streets.Values)
				{
					if (value5.District.type == District.Type.Gateway || value5.District.name == "roadside")
					{
						continue;
					}
					foreach (Vector2i item3 in dir4way.OrderBy([PublicizedFrom(EAccessModifier.Internal)] (Vector2i d2) => rnd.RandomRange(0, 100)).ToList())
					{
						StreetTile neighbor = value5.GetNeighbor(item3);
						if (neighbor != null)
						{
							bool flag2 = true;
							if (neighbor.District != null && neighbor.District.type != District.Type.Gateway && neighbor.District != value5.District && townshipData.OutskirtDistrictPercent >= 0.6f)
							{
								flag2 = rnd.RandomFloat < 0.5f;
							}
							if (neighbor.Township != value5.Township)
							{
								flag2 = false;
							}
							if (flag2)
							{
								value5.SetExitUsed(value5.getHighwayExitPositionByDirection(item3));
							}
							else
							{
								value5.SetExitUnUsed(value5.getHighwayExitPositionByDirection(item3));
							}
						}
					}
				}
				township.CleanupStreets();
				township.ID = townID++;
				worldBuilder.Townships.Add(township);
				num5++;
				if (!biomeStats.TryGetValue(biomeType, out var value2))
				{
					value2 = new BiomeStats();
					biomeStats.Add(biomeType, value2);
				}
				value2.townshipCount++;
				if (!value2.counts.TryGetValue(townshipTypeName, out var value3))
				{
					value2.counts.Add(townshipTypeName, 1);
				}
				else
				{
					value2.counts[townshipTypeName] = value3 + 1;
				}
				biomeType = nextBiomeType(biomeType, townshipData);
				num4 = 80;
			}
			townshipTypeName = null;
			townshipData = null;
		}
		Log.Out("TownPlanner Plan {0} in {1}", worldBuilder.Townships.Count, (float)ms.ElapsedMilliseconds * 0.001f);
		for (int num14 = 0; num14 < 5; num14++)
		{
			BiomeType biomeType3 = (BiomeType)num14;
			if (!biomeStats.TryGetValue(biomeType3, out var value4))
			{
				continue;
			}
			string text = "";
			foreach (KeyValuePair<string, int> count in value4.counts)
			{
				text += $", {count.Key} {count.Value}";
			}
			Log.Out("TownPlanner {0} has {1} townships{2}", biomeType3, value4.townshipCount, text);
		}
		biomeStats.Clear();
		yield return worldBuilder.SetMessage(Localization.Get("xuiRwgTownPlanningFinished"));
	}

	public IEnumerator SpawnPrefabs()
	{
		yield return null;
		MicroStopwatch ms = new MicroStopwatch(_bStart: true);
		MicroStopwatch msReset = new MicroStopwatch(_bStart: true);
		foreach (Township township in worldBuilder.Townships)
		{
			township.SpawnPrefabs();
			if (msReset.ElapsedMilliseconds > 500)
			{
				yield return null;
				msReset.ResetAndRestart();
			}
		}
		Log.Out($"TownPlanner SpawnPrefabs in {(float)ms.ElapsedMilliseconds * 0.001f}, r={Rand.Instance.PeekSample():x}");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool tooClose(Vector2i position, bool isRoadsideTownship, TownshipSpawnInfo tsSpawnInfo)
	{
		int num = (isRoadsideTownship ? 3 : tsSpawnInfo.distance);
		int num2 = 5;
		foreach (Township township in worldBuilder.Townships)
		{
			bool flag = township.Type == TownshipStatic.TypesByName["roadside"];
			foreach (Vector2i key in township.Streets.Keys)
			{
				if (isRoadsideTownship && flag && Utils.FastAbs(position.x - key.x) < (float)num2 && Utils.FastAbs(position.y - key.y) < (float)num2)
				{
					return true;
				}
				if (Utils.FastAbs(position.x - key.x) < (float)num && Utils.FastAbs(position.y - key.y) < (float)num)
				{
					return true;
				}
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool grow(WorldBuilder.TownshipData _data, List<StreetTile> baseTiles, List<StreetTile> finalTiles)
	{
		finalTiles.Clear();
		Vector2i[] array = ((_data.OutskirtDistrictPercent < 1f) ? dir4way : dir8way);
		foreach (StreetTile baseTile in baseTiles)
		{
			Vector2i[] array2 = array;
			foreach (Vector2i vector2i in array2)
			{
				StreetTile streetTileGrid = worldBuilder.GetStreetTileGrid(baseTile.GridPosition + vector2i);
				if (streetTileGrid == null || baseTiles.Contains(streetTileGrid) || finalTiles.Contains(streetTileGrid))
				{
					continue;
				}
				if (!streetTileGrid.IsValidForStreetTile)
				{
					return false;
				}
				StreetTile[] neighbors = streetTileGrid.GetNeighbors();
				for (int j = 0; j < neighbors.Length; j++)
				{
					if (neighbors[j].Township != null)
					{
						return false;
					}
				}
				finalTiles.Add(streetTileGrid);
			}
		}
		if (_data.OutskirtDistrictPercent < 1f)
		{
			float num = 1f - _data.OutskirtDistrictPercent;
			float num2 = 0f;
			for (int num3 = finalTiles.Count - 1; num3 >= 0; num3--)
			{
				num2 += num;
				if (num2 >= 1f)
				{
					num2 -= 1f;
					finalTiles.RemoveAt(num3);
				}
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int getTownshipCounts(DynamicProperties _properties, Dictionary<int, TownshipSpawnInfo> townshipSpawnInfo)
	{
		int num = 0;
		foreach (string key in TownshipStatic.TypesByName.Keys)
		{
			if (!(key == "wilderness") && !(key == "highway"))
			{
				string text = key.ToLower();
				float optionalValue = 1f;
				float optionalValue2 = 1f;
				_properties.ParseVec($"{text}.tiles", ref optionalValue, ref optionalValue2);
				int count = worldBuilder.GetCount(text, worldBuilder.Towns);
				int optionalValue3 = 5;
				_properties.ParseInt($"{text}.distance", ref optionalValue3);
				if (count >= 1 && optionalValue >= 1f && optionalValue2 >= 1f)
				{
					townshipSpawnInfo.Add(TownshipStatic.TypesByName[key], new TownshipSpawnInfo((int)optionalValue, (int)optionalValue2, count, optionalValue3));
				}
				num += count;
			}
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public BiomeType nextBiomeType(BiomeType _current, WorldBuilder.TownshipData _townshipData)
	{
		int num = (int)_current;
		for (int i = 0; i < 4; i++)
		{
			num = (num + 1) % 5;
			if (_townshipData.Biomes.IsEmpty || _townshipData.Biomes.Test_Bit(worldBuilder.biomeTagBits[num]))
			{
				return (BiomeType)num;
			}
		}
		return BiomeType.none;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public BiomeType getBiomeWithMostAvailableSpace(WorldBuilder.TownshipData _townshipData)
	{
		int num = 0;
		int num2 = 255;
		List<Vector2i> list = new List<Vector2i>();
		for (int i = 0; i < 5; i++)
		{
			if (_townshipData.Biomes.IsEmpty || _townshipData.Biomes.Test_Bit(worldBuilder.biomeTagBits[i]))
			{
				getStreetTilesForBiome((BiomeType)i, list);
				if (list.Count > num)
				{
					num = list.Count;
					num2 = i;
				}
			}
		}
		return (BiomeType)num2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<BiomeType, List<Vector2i>> getStreetTilesByBiome()
	{
		Dictionary<BiomeType, List<Vector2i>> dictionary = new Dictionary<BiomeType, List<Vector2i>>();
		for (int i = 0; i < 5; i++)
		{
			List<Vector2i> list = new List<Vector2i>();
			getStreetTilesForBiome((BiomeType)i, list);
			if (list.Count > 0)
			{
				dictionary.Add((BiomeType)i, list);
			}
		}
		return dictionary;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void getStreetTilesForBiome(BiomeType _biomeType, List<Vector2i> _biomeStreetTiles)
	{
		_biomeStreetTiles.Clear();
		StreetTile[,] streetTileMap = worldBuilder.StreetTileMap;
		foreach (StreetTile streetTile in streetTileMap)
		{
			if (streetTile.IsValidForStreetTile && streetTile.Township == null && !streetTile.Used && streetTile.BiomeType == _biomeType)
			{
				_biomeStreetTiles.Add(streetTile.GridPosition);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<TileGroup> getStreetTileGroups()
	{
		List<TileGroup> list = new List<TileGroup>();
		Dictionary<Vector2i, int> dictionary = new Dictionary<Vector2i, int>();
		StreetTile[,] streetTileMap = worldBuilder.StreetTileMap;
		foreach (StreetTile streetTile in streetTileMap)
		{
			int num = list.Count;
			if (!streetTile.IsValidForStreetTile)
			{
				continue;
			}
			for (int k = 0; k < dir4way.Length; k++)
			{
				if (streetTile.GridPosition.x + dir4way[k].x >= 0 && streetTile.GridPosition.x + dir4way[k].x < worldBuilder.StreetTileMap.GetLength(0) && streetTile.GridPosition.y + dir4way[k].y >= 0 && streetTile.GridPosition.y + dir4way[k].y < worldBuilder.StreetTileMap.GetLength(1))
				{
					Vector2i key = streetTile.GridPosition + dir4way[k];
					if (worldBuilder.StreetTileMap[key.x, key.y].BiomeType == streetTile.BiomeType && worldBuilder.StreetTileMap[key.x, key.y].IsValidForStreetTile && dictionary.TryGetValue(key, out var value))
					{
						num = value;
					}
				}
			}
			dictionary[streetTile.GridPosition] = num;
			if (num == list.Count)
			{
				list.Add(new TileGroup
				{
					Biome = streetTile.BiomeType
				});
			}
			list[num].Positions.Add(streetTile.GridPosition);
		}
		return list;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void getStreetLayout(Vector2i startPosition, int townSize, GameRandom rnd, bool isRoadside, List<StreetTile> townTiles)
	{
		townTiles.Clear();
		StreetTile streetTileGrid = worldBuilder.GetStreetTileGrid(startPosition);
		if (!isRoadside)
		{
			foreach (Township township in worldBuilder.Townships)
			{
				if (streetTileGrid.Area.Overlaps(township.BufferArea) || township.BufferArea.Contains(streetTileGrid.Area.center) || township.BufferArea.Overlaps(streetTileGrid.Area))
				{
					return;
				}
			}
		}
		else
		{
			if (streetTileGrid.Township != null || streetTileGrid.District != null)
			{
				return;
			}
			foreach (Township township2 in worldBuilder.Townships)
			{
				if (streetTileGrid.Area.Overlaps(township2.BufferArea) || township2.BufferArea.Contains(streetTileGrid.Area.center) || township2.BufferArea.Overlaps(streetTileGrid.Area))
				{
					return;
				}
			}
			StreetTile[] neighbors8way = streetTileGrid.GetNeighbors8way();
			foreach (StreetTile streetTile in neighbors8way)
			{
				if (streetTile.Township != null || streetTile.District != null)
				{
					return;
				}
			}
		}
		if (townSize == 1)
		{
			townTiles.Add(streetTileGrid);
			return;
		}
		List<StreetTile> list = new List<StreetTile>();
		if (townSize <= 16)
		{
			townTiles.Add(streetTileGrid);
			list.Add(streetTileGrid);
		}
		else
		{
			int num = (int)Mathf.Sqrt(townSize) - 1;
			int num2 = num / -2;
			int num3 = num / 2 - 1;
			Vector2i pos = default(Vector2i);
			for (int j = num2; j <= num3; j++)
			{
				pos.y = startPosition.y + j;
				bool flag = j == num2 || j == num3;
				for (int k = num2; k <= num3; k++)
				{
					pos.x = startPosition.x + k;
					StreetTile streetTile2 = StreetLayoutCheckPos(pos);
					if (streetTile2 == null)
					{
						townTiles.Clear();
						return;
					}
					townTiles.Add(streetTile2);
					if (flag || k == num2 || k == num3)
					{
						list.Add(streetTile2);
					}
				}
			}
			if (townTiles.Count >= townSize)
			{
				return;
			}
		}
		while (list.Count > 0)
		{
			int index = rnd.RandomRange(0, list.Count);
			StreetTile streetTile3 = list[index];
			list.RemoveAt(index);
			int num4 = rnd.RandomRange(4);
			for (int l = 0; l < 4; l++)
			{
				Vector2i pos2 = streetTile3.GridPosition + dir4way[(l + num4) & 3];
				StreetTile streetTile4 = StreetLayoutCheckPos(pos2);
				if (streetTile4 != null && !townTiles.Contains(streetTile4))
				{
					townTiles.Add(streetTile4);
					if (townTiles.Count >= townSize)
					{
						return;
					}
					list.Add(streetTile4);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public StreetTile StreetLayoutCheckPos(Vector2i _pos)
	{
		StreetTile streetTileGrid = worldBuilder.GetStreetTileGrid(_pos);
		if (streetTileGrid != null && streetTileGrid.IsValidForStreetTile)
		{
			foreach (Township township in worldBuilder.Townships)
			{
				if (streetTileGrid.Area.Overlaps(township.BufferArea) || township.BufferArea.Contains(streetTileGrid.Area.center) || township.BufferArea.Overlaps(streetTileGrid.Area))
				{
					return null;
				}
			}
			return streetTileGrid;
		}
		return null;
	}
}
