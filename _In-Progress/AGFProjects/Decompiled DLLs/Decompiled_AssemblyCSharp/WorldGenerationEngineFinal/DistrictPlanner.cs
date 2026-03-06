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
		if (!_township.IsRoadside())
		{
			generateDistricts(_township);
		}
		if (!_township.Data.SpawnGateway)
		{
			return;
		}
		if (_township.IsRoadside())
		{
			GenerateGateway(_township);
			return;
		}
		int num = ((!_township.IsBig()) ? 1 : 2);
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				GenerateGatewayDir(_township, j);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void generateDistricts(Township _township)
	{
		if (_township.Streets.Count == 0)
		{
			return;
		}
		GameRandom gameRandom = GameRandomManager.Instance.CreateGameRandom(worldBuilder.Seed + _township.GridCenter.x + _township.GridCenter.y);
		bool flag = _township.CalcCenterStreetTile().BiomeType == BiomeType.wasteland;
		Dictionary<string, District> dictionary = new Dictionary<string, District>();
		float num = 0f;
		string str = _township.GetTypeName().ToLower();
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
		List<string> list = dictionary.OrderByDescending([PublicizedFrom(EAccessModifier.Internal)] (KeyValuePair<string, District> entry) =>
		{
			KeyValuePair<string, District> keyValuePair2 = entry;
			if (!(keyValuePair2.Value.name == "downtown"))
			{
				keyValuePair2 = entry;
				return keyValuePair2.Value.weight;
			}
			return 99f;
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
		list2.Shuffle(worldBuilder.Seed + _township.GridCenter.x + _township.GridCenter.y);
		int count = list.Count;
		for (int num2 = 0; num2 < count; num2++)
		{
			District district3 = dictionary[list[num2]];
			int num3 = Mathf.RoundToInt((float)list2.Count * district3.weight);
			if (num2 == count - 1)
			{
				num3 = int.MaxValue;
			}
			foreach (StreetTile item2 in list2)
			{
				if (item2.District != null)
				{
					continue;
				}
				if (district3.counter >= num3)
				{
					break;
				}
				bool flag2 = true;
				StreetTile[] neighbors8Way = item2.GetNeighbors8Way();
				foreach (StreetTile streetTile in neighbors8Way)
				{
					if (streetTile.District != null && district3.avoidedNeighborDistricts.Contains(streetTile.District.name))
					{
						flag2 = false;
						break;
					}
				}
				if (!flag2)
				{
					flag2 = true;
					neighbors8Way = item2.GetNeighbors();
					foreach (StreetTile streetTile2 in neighbors8Way)
					{
						if (streetTile2.District != null && district3.avoidedNeighborDistricts.Contains(streetTile2.District.name))
						{
							flag2 = false;
							break;
						}
					}
				}
				if (flag2)
				{
					district3.counter++;
					item2.District = district3;
					item2.Used = true;
					item2.SetPathingConstraintsForTile(allBlocked: true);
				}
			}
		}
		foreach (StreetTile item3 in list2)
		{
			if (item3.District == null)
			{
				item3.SetTownship(null);
				_township.Streets.Remove(item3.GridPosition);
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
			value.SetTownship(_township);
			_township.Gateways.Add(value);
			value.SetPathingConstraintsForTile(allBlocked: true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GenerateGatewayDir(Township _township, int _baseDir)
	{
		foreach (StreetTile value in _township.Streets.Values)
		{
			if (value.District != null && value.District.type == District.Type.Gateway)
			{
				continue;
			}
			StreetTile neighbor = value.GetNeighbor(_baseDir);
			if (_township.Streets.ContainsKey(neighbor.GridPosition) || !neighbor.IsValidForGateway)
			{
				continue;
			}
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			for (int i = 0; i < 4; i++)
			{
				StreetTile neighbor2 = neighbor.GetNeighbor(i);
				if (neighbor2 == null || !neighbor2.IsValidForGateway)
				{
					continue;
				}
				num3 |= 1 << i;
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
				neighbor.SetTownship(_township);
				neighbor.District = DistrictPlannerStatic.Districts["gateway"];
				neighbor.Used = true;
				neighbor.SetPathingConstraintsForTile(allBlocked: true);
				StreetTile[] neighbors = neighbor.GetNeighbors();
				for (int j = 0; j < neighbors.Length; j++)
				{
					neighbors[j].SetPathingConstraintsForTile();
				}
				_township.Streets.Add(neighbor.GridPosition, neighbor);
				_township.Gateways.Add(neighbor);
				break;
			}
		}
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
							sortingGroup3.Positions.Shuffle(hashCode);
							directionsRnd.Clear();
							directionsRnd.AddRange(directions4);
							directionsRnd.Shuffle(hashCode);
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
