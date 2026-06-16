using System.Collections.Generic;
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
	public const int cTriesSmaller = 30;

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

	public void Plan(DynamicProperties _properties, int worldSeed)
	{
		MicroStopwatch microStopwatch = new MicroStopwatch(_bStart: true);
		Dictionary<BiomeType, List<Vector2i>> streetTilesByBiome = getStreetTilesByBiome();
		Dictionary<int, TownshipSpawnInfo> townshipSpawnInfos = new Dictionary<int, TownshipSpawnInfo>();
		getTownshipCounts(_properties, townshipSpawnInfos);
		int num = 0;
		List<int> list = new List<int>();
		townshipSpawnInfos.CopyKeysTo(list);
		list.Sort([PublicizedFrom(EAccessModifier.Internal)] (int key1, int key2) => townshipSpawnInfos[key2].max.CompareTo(townshipSpawnInfos[key1].max));
		int num2 = 1982;
		GameRandom gameRandom = GameRandomManager.Instance.CreateGameRandom();
		List<int> list2 = new List<int> { 0, 1, 2, 3 };
		foreach (int item in list)
		{
			if (!WorldBuilderStatic.idToTownshipData.TryGetValue(item, out var value))
			{
				continue;
			}
			string name = value.Name;
			worldBuilder.SetTaskMessage(string.Format(worldBuilder.messageTownPlanning, Application.isEditor ? name : string.Empty));
			gameRandom.SetSeed(worldSeed + num2++);
			bool flag = value.Category == TownshipData.eCategory.Roadside;
			int num3 = worldBuilder.StreetTileMapWidth / 10;
			if (flag)
			{
				num3 = worldBuilder.StreetTileMapWidth / 8;
			}
			int num4 = worldBuilder.StreetTileMapWidth - num3;
			int num5 = worldBuilder.StreetTileMapWidth - num3;
			int num6 = 80;
			int num7 = 30;
			bool flag2 = false;
			TownshipSpawnInfo townshipSpawnInfo = townshipSpawnInfos[item];
			BiomeType biomeType = BiomeType.none;
			for (int num8 = 0; num8 < townshipSpawnInfo.count; num8++)
			{
				int distance = (flag2 ? 1 : (flag ? 3 : townshipSpawnInfo.distance));
				if (biomeType == BiomeType.none)
				{
					biomeType = getBiomeWithMostAvailableSpace(value);
				}
				List<Vector2i> list3 = null;
				BiomeType biomeType2 = biomeType;
				for (int num9 = 0; num9 < 5; num9++)
				{
					if (streetTilesByBiome.TryGetValue(biomeType, out var value2))
					{
						list3 = new List<Vector2i>(value2);
						for (int num10 = list3.Count - 1; num10 >= 0; num10--)
						{
							Vector2i position = list3[num10];
							if (position.x <= num3 || position.y <= num3 || position.x >= num4 || position.y >= num5 || IsTooClose(position, flag, distance))
							{
								list3.RemoveAt(num10);
							}
						}
						if (list3.Count > 0)
						{
							break;
						}
					}
					biomeType = nextBiomeType(biomeType, value);
					if (biomeType == biomeType2)
					{
						break;
					}
				}
				Township township = null;
				List<StreetTile> list4 = null;
				List<StreetTile> list5 = null;
				int num11;
				if (list3 != null && list3.Count > 0)
				{
					list4 = new List<StreetTile>();
					list5 = new List<StreetTile>();
					township = new Township(worldBuilder, value)
					{
						BiomeType = biomeType
					};
					int min = townshipSpawnInfo.min;
					if (townshipSpawnInfo.max >= 10)
					{
						min = (townshipSpawnInfo.min + townshipSpawnInfo.max) / 2;
					}
					num11 = gameRandom.RandomRange(min, townshipSpawnInfo.max + 1);
					min = townshipSpawnInfo.min;
					if (flag2 && min >= 4)
					{
						min = 1;
						num11 = 4;
					}
					while (num11 >= min)
					{
						int num12 = gameRandom.RandomRange(0, list3.Count);
						int num13 = ((!(gameRandom.RandomFloat < 0.5f)) ? 1 : (list3.Count - 1));
						for (int num14 = 0; num14 < list3.Count; num14++)
						{
							Vector2i startPosition = (township.GridCenter = list3[num12]);
							num12 = (num12 + num13) % list3.Count;
							for (int num15 = 0; num15 < 12; num15++)
							{
								GetStreetLayout(startPosition, num11, gameRandom, flag, list4);
								if (list4.Count >= num11 && (!(value.OutskirtDistrictPercent > 0f) || Grow(value, list4, list5, !flag2)))
								{
									num14 = 999999;
									num11 = -1;
									break;
								}
							}
						}
						num11--;
					}
				}
				else
				{
					num11 = 0;
					num6 = 0;
				}
				if (num11 >= 0)
				{
					if (num6 > 0)
					{
						num6--;
						num8--;
						biomeType = nextBiomeType(biomeType, value);
						continue;
					}
					if (townshipSpawnInfo.min >= 4 && num7 > 0)
					{
						num7--;
						num8--;
						flag2 = true;
						biomeType = nextBiomeType(biomeType, value);
						continue;
					}
				}
				num6 = 80;
				num7 = 30;
				flag2 = false;
				if (num11 >= 0)
				{
					biomeType = BiomeType.none;
					continue;
				}
				if (value.OutskirtDistrictPercent > 0f)
				{
					string outskirtDistrict = value.OutskirtDistrict;
					foreach (StreetTile item2 in list5)
					{
						item2.SetTownship(township);
						item2.District = DistrictPlannerStatic.Districts[outskirtDistrict];
						township.Streets[item2.GridPosition] = item2;
					}
				}
				foreach (StreetTile item3 in list4)
				{
					item3.SetTownship(township);
					township.Streets[item3.GridPosition] = item3;
					item3.District = null;
				}
				worldBuilder.DistrictPlanner.PlanTownship(township);
				if (!township.IsRoadside())
				{
					foreach (StreetTile value6 in township.Streets.Values)
					{
						value6.SetExitsToMyTownship();
					}
					foreach (StreetTile value7 in township.Streets.Values)
					{
						if (value7.District.type == District.Type.Gateway)
						{
							continue;
						}
						list2.Shuffle(gameRandom);
						foreach (int item4 in list2)
						{
							StreetTile neighbor = value7.GetNeighbor(item4);
							if (neighbor != null && neighbor.District != null && neighbor.District.type != District.Type.Gateway && neighbor.District != value7.District)
							{
								int num16 = value7.CountDistrictExitsBetween(neighbor);
								int num17 = ((value7.District.type != District.Type.Downtown && neighbor.District.type != District.Type.Downtown) ? 1 : 2);
								if (num16 > num17)
								{
									value7.SetExitUnUsedAndFromNeighbor(item4);
								}
							}
						}
					}
					int num18 = 0;
					foreach (StreetTile value8 in township.Streets.Values)
					{
						if (value8.District.type != District.Type.Gateway)
						{
							int num19 = 1 << (int)value8.District.type;
							if ((num18 & num19) == 0 && value8.ChangeLConnectionToCap(gameRandom))
							{
								num18 |= num19;
							}
						}
					}
				}
				township.CleanupStreets();
				township.ID = num++;
				worldBuilder.Townships.Add(township);
				if (!biomeStats.TryGetValue(biomeType, out var value3))
				{
					value3 = new BiomeStats();
					biomeStats.Add(biomeType, value3);
				}
				value3.townshipCount++;
				if (!value3.counts.TryGetValue(name, out var value4))
				{
					value3.counts.Add(name, 1);
				}
				else
				{
					value3.counts[name] = value4 + 1;
				}
				biomeType = nextBiomeType(biomeType, value);
			}
		}
		Log.Out("TownPlanner Plan {0} in {1}", worldBuilder.Townships.Count, (float)microStopwatch.ElapsedMilliseconds * 0.001f);
		for (int num20 = 0; num20 < 5; num20++)
		{
			BiomeType biomeType3 = (BiomeType)num20;
			if (!biomeStats.TryGetValue(biomeType3, out var value5))
			{
				continue;
			}
			string text = "";
			foreach (KeyValuePair<string, int> count in value5.counts)
			{
				text += $", {count.Key} {count.Value}";
			}
			Log.Out("TownPlanner {0} has {1} townships{2}", biomeType3, value5.townshipCount, text);
		}
		biomeStats.Clear();
		worldBuilder.SetTaskMessage(worldBuilder.messageTownPlanningFinished);
	}

	public void SpawnPrefabs()
	{
		MicroStopwatch microStopwatch = new MicroStopwatch(_bStart: true);
		foreach (Township township in worldBuilder.Townships)
		{
			township.SpawnPrefabs();
		}
		Log.Out($"TownPlanner SpawnPrefabs in {(float)microStopwatch.ElapsedMilliseconds * 0.001f}, r={Rand.Instance.PeekSample():x}");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsTooClose(Vector2i position, bool isRoadsideTownship, int distance)
	{
		foreach (Township township in worldBuilder.Townships)
		{
			bool flag = township.IsRoadside();
			foreach (Vector2i key in township.Streets.Keys)
			{
				if (isRoadsideTownship && flag && Utils.FastAbs(position.x - key.x) < 5f && Utils.FastAbs(position.y - key.y) < 5f)
				{
					return true;
				}
				if (Utils.FastAbs(position.x - key.x) < (float)distance && Utils.FastAbs(position.y - key.y) < (float)distance)
				{
					return true;
				}
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool Grow(TownshipData _data, List<StreetTile> baseTiles, List<StreetTile> finalTiles, bool _cancelIfInvalidNeighbor)
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
					if (_cancelIfInvalidNeighbor)
					{
						return false;
					}
					continue;
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
		GameRandom gameRandom = Rand.Instance.gameRandom;
		foreach (KeyValuePair<int, TownshipData> idToTownshipDatum in WorldBuilderStatic.idToTownshipData)
		{
			TownshipData value = idToTownshipDatum.Value;
			if (value.Category != TownshipData.eCategory.Wilderness)
			{
				string text = value.Name.ToLower();
				float optionalValue = 1f;
				float optionalValue2 = 1f;
				_properties.ParseVec(text, "tiles", ref optionalValue, ref optionalValue2);
				int count = worldBuilder.GetCount(text, worldBuilder.Towns, gameRandom);
				int optionalValue3 = 5;
				_properties.ParseInt(text, "distance", ref optionalValue3);
				if (count >= 1 && optionalValue >= 1f && optionalValue2 >= 1f)
				{
					townshipSpawnInfo.Add(value.Id, new TownshipSpawnInfo((int)optionalValue, (int)optionalValue2, count, optionalValue3));
				}
				num += count;
			}
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public BiomeType nextBiomeType(BiomeType _current, TownshipData _townshipData)
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
	public BiomeType getBiomeWithMostAvailableSpace(TownshipData _townshipData)
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
		StreetTile[] streetTileMap = worldBuilder.StreetTileMap;
		foreach (StreetTile streetTile in streetTileMap)
		{
			if (streetTile.IsValidForStreetTile && streetTile.Township == null && !streetTile.Used && streetTile.BiomeType == _biomeType)
			{
				_biomeStreetTiles.Add(streetTile.GridPosition);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GetStreetLayout(Vector2i startPosition, int townSize, GameRandom rnd, bool isRoadside, List<StreetTile> townTiles)
	{
		townTiles.Clear();
		StreetTile streetTileGrid = worldBuilder.GetStreetTileGrid(startPosition);
		if (!isRoadside)
		{
			foreach (Township township in worldBuilder.Townships)
			{
				if (streetTileGrid.Area.Overlaps(township.BufferArea))
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
				if (streetTileGrid.Area.Overlaps(township2.BufferArea))
				{
					return;
				}
			}
			StreetTile[] neighbors8Way = streetTileGrid.GetNeighbors8Way();
			foreach (StreetTile streetTile in neighbors8Way)
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
		int num = 2;
		if (_pos.x < num || _pos.x >= worldBuilder.StreetTileMapWidth - num || _pos.y < num || _pos.y >= worldBuilder.StreetTileMapWidth - num)
		{
			return null;
		}
		StreetTile streetTileGrid = worldBuilder.GetStreetTileGrid(_pos);
		if (streetTileGrid != null && streetTileGrid.IsValidForStreetTile)
		{
			foreach (Township township in worldBuilder.Townships)
			{
				if (streetTileGrid.Area.Overlaps(township.BufferArea))
				{
					return null;
				}
			}
			return streetTileGrid;
		}
		return null;
	}
}
