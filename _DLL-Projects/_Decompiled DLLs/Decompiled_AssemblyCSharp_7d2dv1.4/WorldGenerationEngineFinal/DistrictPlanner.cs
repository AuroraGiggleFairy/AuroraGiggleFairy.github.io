using System.Collections.Generic;
using UniLinq;
using UnityEngine;

namespace WorldGenerationEngineFinal;

public class DistrictPlanner
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class SortingGroup
	{
		public District District;

		public List<Vector2i> Positions = new List<Vector2i>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly WorldBuilder worldBuilder;

	public Dictionary<string, DynamicProperties> Properties = new Dictionary<string, DynamicProperties>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<SortingGroup> districtGroups = new List<SortingGroup>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<Vector2i, int> groups = new Dictionary<Vector2i, int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, SortingGroup> biggestDistrictGroups = new Dictionary<string, SortingGroup>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<Vector2i> directionsRnd = new List<Vector2i>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i[] directions4 = new Vector2i[4]
	{
		new Vector2i(0, 1),
		new Vector2i(1, 0),
		new Vector2i(0, -1),
		new Vector2i(-1, 0)
	};

	public DistrictPlanner(WorldBuilder _worldBuilder)
	{
		worldBuilder = _worldBuilder;
	}

	public void PlanTownship(Township _township)
	{
		if (_township.Type != TownshipStatic.TypesByName["roadside"])
		{
			generateDistricts(_township);
		}
		if (!WorldBuilderStatic.townshipDatas[_township.GetTypeName()].SpawnGateway)
		{
			return;
		}
		if (_township.Type == TownshipStatic.TypesByName["roadside"])
		{
			GenerateGateway(_township);
			return;
		}
		int num = ((!_township.IsBig()) ? 1 : 2);
		for (int i = 0; i < num; i++)
		{
			Vector2i[] array = directions4;
			foreach (Vector2i direction in array)
			{
				GenerateGatewayDir(_township, direction);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public StreetTile getCenterStreetTile(Township _township)
	{
		Vector2i zero = Vector2i.zero;
		foreach (StreetTile value in _township.Streets.Values)
		{
			zero += value.GridPosition;
		}
		zero /= _township.Streets.Count;
		int num = int.MaxValue;
		StreetTile result = null;
		foreach (StreetTile value2 in _township.Streets.Values)
		{
			int num2 = Vector2i.DistanceSqrInt(zero, value2.GridPosition);
			if (num2 < num)
			{
				num = num2;
				result = value2;
			}
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void generateDistricts(Township _township)
	{
		if (_township.Streets.Count == 0)
		{
			return;
		}
		GameRandom gameRandom = GameRandomManager.Instance.CreateGameRandom(worldBuilder.Seed + _township.GridCenter.x + _township.GridCenter.y);
		StreetTile centerStreetTile = getCenterStreetTile(_township);
		if (worldBuilder.TownshipShared.Height == int.MinValue)
		{
			worldBuilder.TownshipShared.Height = Mathf.FloorToInt(centerStreetTile.PositionHeight);
		}
		bool flag = centerStreetTile.BiomeType == BiomeType.wasteland;
		Dictionary<string, District> dictionary = new Dictionary<string, District>();
		float num = 0f;
		string str = TownshipStatic.NamesByType[_township.Type].ToLower();
		foreach (var (text2, district2) in DistrictPlannerStatic.Districts)
		{
			if ((flag || !text2.Contains("wasteland")) && district2.townships.Test_AnySet(FastTags<TagGroup.Poi>.Parse(str)) && district2.weight != 0f)
			{
				dictionary.Add(text2, new District(district2));
				num += district2.weight;
			}
		}
		foreach (KeyValuePair<string, District> item in dictionary)
		{
			District value = item.Value;
			value.weight /= num;
			foreach (string avoidedNeighborDistrict in value.avoidedNeighborDistricts)
			{
				if (dictionary.TryGetValue(avoidedNeighborDistrict, out var value2) && !value2.avoidedNeighborDistricts.Contains(item.Key))
				{
					value2.avoidedNeighborDistricts.Add(item.Key);
				}
			}
		}
		List<string> list = dictionary.OrderBy([PublicizedFrom(EAccessModifier.Internal)] (KeyValuePair<string, District> entry) =>
		{
			KeyValuePair<string, District> keyValuePair2 = entry;
			if (!(keyValuePair2.Value.name == "downtown"))
			{
				keyValuePair2 = entry;
				return keyValuePair2.Value.weight;
			}
			return 0f;
		}).Select([PublicizedFrom(EAccessModifier.Internal)] (KeyValuePair<string, District> entry) =>
		{
			KeyValuePair<string, District> keyValuePair2 = entry;
			return keyValuePair2.Key;
		}).ToList();
		List<StreetTile> list2 = new List<StreetTile>();
		foreach (StreetTile value3 in _township.Streets.Values)
		{
			if (value3.District != null)
			{
				value3.District.counter++;
			}
			else
			{
				list2.Add(value3);
			}
		}
		Shuffle(worldBuilder.Seed + _township.GridCenter.x + _township.GridCenter.y, list2);
		foreach (string item2 in list)
		{
			District district3 = dictionary[item2];
			int num2 = Mathf.CeilToInt((float)list2.Count * district3.weight);
			foreach (StreetTile item3 in list2)
			{
				if (item3.District != null)
				{
					continue;
				}
				if (district3.counter >= num2)
				{
					break;
				}
				bool flag2 = true;
				StreetTile[] neighbors = item3.GetNeighbors();
				foreach (StreetTile streetTile in neighbors)
				{
					if (streetTile.District != null && district3.avoidedNeighborDistricts.Contains(streetTile.District.name))
					{
						flag2 = false;
						break;
					}
				}
				if (flag2)
				{
					district3.counter++;
					item3.District = district3;
					item3.Used = true;
					item3.SetPathingConstraintsForTile(allBlocked: true);
				}
			}
		}
		foreach (StreetTile item4 in list2)
		{
			if (item4.District == null)
			{
				item4.Township = null;
				_township.Streets.Remove(item4.GridPosition);
			}
		}
		GroupDistricts(_township, dictionary);
		GameRandomManager.Instance.FreeGameRandom(gameRandom);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GenerateGateway(Township _township)
	{
		foreach (StreetTile value in _township.Streets.Values)
		{
			value.District = DistrictPlannerStatic.Districts["gateway"];
			value.Township = _township;
			_township.Gateways.Add(value);
			value.SetPathingConstraintsForTile(allBlocked: true);
			value.SetRoadExits(_north: true, _east: true, _south: true, _west: true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GenerateGatewayDir(Township _township, Vector2i _direction)
	{
		foreach (StreetTile value in _township.Streets.Values)
		{
			if (value.District != null && value.District.name == "gateway")
			{
				continue;
			}
			StreetTile neighbor = value.GetNeighbor(_direction);
			if (_township.Streets.ContainsKey(neighbor.GridPosition) || !neighbor.IsValidForGateway)
			{
				continue;
			}
			int num = 0;
			int num2 = 0;
			int num3 = -1;
			bool[] array = new bool[4];
			Vector2i[] array2 = directions4;
			foreach (Vector2i direction in array2)
			{
				num3++;
				StreetTile neighbor2 = neighbor.GetNeighbor(direction);
				if (neighbor2 == null || !neighbor2.IsValidForGateway)
				{
					continue;
				}
				array[num3] = true;
				num2++;
				if (neighbor2.Township != null)
				{
					num++;
					if (num > 1)
					{
						break;
					}
				}
			}
			if (num == 1 && num2 >= 2)
			{
				neighbor.District = DistrictPlannerStatic.Districts["gateway"];
				neighbor.Township = _township;
				neighbor.Used = true;
				neighbor.SetRoadExits(array);
				neighbor.SetPathingConstraintsForTile(allBlocked: true);
				StreetTile[] neighbors = neighbor.GetNeighbors();
				for (int i = 0; i < neighbors.Length; i++)
				{
					neighbors[i].SetPathingConstraintsForTile();
				}
				_township.Streets.Add(neighbor.GridPosition, neighbor);
				_township.Gateways.Add(neighbor);
				break;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void Shuffle<T>(int seed, List<T> list)
	{
		int num = list.Count;
		GameRandom gameRandom = GameRandomManager.Instance.CreateGameRandom(seed);
		while (num > 1)
		{
			num--;
			int num2 = gameRandom.RandomRange(0, num) % num;
			int index = num2;
			int index2 = num;
			T val = list[num];
			T val2 = list[num2];
			T val3 = (list[index] = val);
			val3 = (list[index2] = val2);
		}
		GameRandomManager.Instance.FreeGameRandom(gameRandom);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GroupDistricts(Township _township, Dictionary<string, District> districtList)
	{
		for (int i = 0; i < 100; i++)
		{
			districtGroups.Clear();
			groups.Clear();
			foreach (Vector2i key6 in _township.Streets.Keys)
			{
				District district = _township.Streets[key6].District;
				int num = districtGroups.Count;
				for (int j = 0; j < directions4.Length; j++)
				{
					Vector2i key = key6 + directions4[j];
					if (_township.Streets.TryGetValue(key, out var value) && value.District == district && groups.TryGetValue(key, out var value2))
					{
						num = value2;
					}
				}
				groups[key6] = num;
				if (num == districtGroups.Count)
				{
					districtGroups.Add(new SortingGroup
					{
						District = district
					});
				}
				districtGroups[num].Positions.Add(key6);
			}
			if (districtGroups.Count <= districtList.Count)
			{
				break;
			}
			biggestDistrictGroups.Clear();
			string key2;
			District value3;
			foreach (KeyValuePair<string, District> district4 in districtList)
			{
				district4.Deconstruct(out key2, out value3);
				string key3 = key2;
				District district2 = value3;
				int num2 = int.MinValue;
				SortingGroup sortingGroup = null;
				for (int k = 0; k < districtGroups.Count; k++)
				{
					SortingGroup sortingGroup2 = districtGroups[k];
					if (sortingGroup2.District == district2 && sortingGroup2.Positions.Count > num2)
					{
						num2 = sortingGroup2.Positions.Count;
						sortingGroup = sortingGroup2;
					}
				}
				if (sortingGroup != null)
				{
					biggestDistrictGroups.Add(key3, sortingGroup);
				}
			}
			foreach (KeyValuePair<string, District> district5 in districtList)
			{
				district5.Deconstruct(out key2, out value3);
				string key4 = key2;
				District value4 = value3;
				List<SortingGroup> list = districtGroups.FindAll([PublicizedFrom(EAccessModifier.Internal)] (SortingGroup _group) => _group.District == value4);
				if (list.Count == 0)
				{
					continue;
				}
				list.Sort([PublicizedFrom(EAccessModifier.Internal)] (SortingGroup _groupA, SortingGroup _groupB) => _groupB.Positions.Count.CompareTo(_groupA.Positions.Count));
				SortingGroup sortingGroup3 = biggestDistrictGroups[key4];
				for (int num3 = 0; num3 < list.Count; num3++)
				{
					if (sortingGroup3 == list[num3])
					{
						continue;
					}
					for (int num4 = 0; num4 < list[num3].Positions.Count; num4++)
					{
						Vector2i vector2i = list[num3].Positions[num4];
						if (_township.Streets[vector2i].District != value4)
						{
							continue;
						}
						for (int num5 = 0; num5 < directions4.Length; num5++)
						{
							Vector2i key5 = vector2i + directions4[num5];
							District district3 = null;
							Vector2i vector2i2 = default(Vector2i);
							if (!_township.Streets.TryGetValue(key5, out var value5))
							{
								continue;
							}
							district3 = value5.District;
							if (district3.type == District.Type.Downtown || district3.type == District.Type.Gateway || district3.type == District.Type.Rural)
							{
								continue;
							}
							int hashCode = key5.ToString().GetHashCode();
							Shuffle(hashCode, sortingGroup3.Positions);
							directionsRnd.Clear();
							directionsRnd.AddRange(directions4);
							Shuffle(hashCode, directionsRnd);
							foreach (Vector2i position in sortingGroup3.Positions)
							{
								foreach (Vector2i item in directionsRnd)
								{
									Vector2i vector2i3 = position + item;
									if (!_township.Streets.TryGetValue(vector2i3, out var value6) || value6.District != district3)
									{
										continue;
									}
									bool flag = true;
									StreetTile[] neighbors = value6.GetNeighbors();
									foreach (StreetTile streetTile in neighbors)
									{
										if (streetTile.District != null && value4.avoidedNeighborDistricts.Contains(streetTile.District.name))
										{
											flag = false;
											break;
										}
									}
									if (flag)
									{
										value6.District = value4;
										_township.Streets[vector2i].District = district3;
										vector2i2 = vector2i3;
										break;
									}
								}
								if (vector2i2 != default(Vector2i))
								{
									break;
								}
							}
							if (vector2i2 != default(Vector2i))
							{
								sortingGroup3.Positions.Add(vector2i2);
								break;
							}
						}
					}
				}
			}
		}
		districtGroups.Clear();
		groups.Clear();
		biggestDistrictGroups.Clear();
		directionsRnd.Clear();
	}
}
