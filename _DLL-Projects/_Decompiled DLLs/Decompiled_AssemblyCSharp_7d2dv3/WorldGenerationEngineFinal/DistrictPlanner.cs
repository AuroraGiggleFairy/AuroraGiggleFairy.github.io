using System.Collections.Generic;
using UniLinq;
using UnityEngine;

namespace WorldGenerationEngineFinal;

public class DistrictPlanner
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly WorldBuilder worldBuilder;

	public DistrictPlanner(WorldBuilder _worldBuilder)
	{
		worldBuilder = _worldBuilder;
	}

	public void PlanTownship(Township _township)
	{
		if (_township.IsRoadside())
		{
			if (_township.Data.SpawnGateway)
			{
				GenerateGateway(_township);
			}
			return;
		}
		GenerateDistricts(_township);
		if (!_township.Data.SpawnGateway)
		{
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
	public void GenerateDistricts(Township _township)
	{
		if (_township.Streets.Count == 0)
		{
			return;
		}
		GameRandom gameRandom = GameRandomManager.Instance.CreateGameRandom(worldBuilder.Seed + _township.GridCenter.x + _township.GridCenter.y);
		Dictionary<string, District> dictionary = new Dictionary<string, District>();
		float num = 0f;
		string str = _township.GetTypeName().ToLower();
		foreach (var (key, district2) in DistrictPlannerStatic.Districts)
		{
			if (district2.townships.Test_AnySet(FastTags<TagGroup.Poi>.Parse(str)) && district2.weight > 0f)
			{
				dictionary.Add(key, new District(district2));
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
			return keyValuePair2.Value.spawnOrder;
		}).Select([PublicizedFrom(EAccessModifier.Internal)] (KeyValuePair<string, District> entry) =>
		{
			KeyValuePair<string, District> keyValuePair2 = entry;
			return keyValuePair2.Key;
		}).ToList();
		List<StreetTile> list2 = new List<StreetTile>();
		foreach (StreetTile value4 in _township.Streets.Values)
		{
			if (value4.District == null)
			{
				list2.Add(value4);
			}
		}
		StreetTile streetTile = _township.CalcCenterStreetTile();
		Vector2i centerPos = streetTile.WorldPositionCenter;
		centerPos.x += 5;
		list2.Sort([PublicizedFrom(EAccessModifier.Internal)] (StreetTile a, StreetTile b) =>
		{
			float num6 = Vector2i.Distance(centerPos, a.WorldPositionCenter);
			float value3 = Vector2i.Distance(centerPos, b.WorldPositionCenter);
			return num6.CompareTo(value3);
		});
		int count = list.Count;
		for (int num2 = 0; num2 < count; num2++)
		{
			District district3 = dictionary[list[num2]];
			int num3 = Mathf.RoundToInt((float)list2.Count * district3.weight);
			if (num2 == count - 1)
			{
				num3 = int.MaxValue;
			}
			int num4 = 0;
			for (int num5 = 0; num5 < list2.Count && num4 < num3; num5++)
			{
				StreetTile streetTile2 = list2[num5];
				if (streetTile2.District == null)
				{
					StreetTile streetTile3 = FindFreeWithNeighbor(list2, district3);
					if (streetTile3 != null)
					{
						streetTile2 = streetTile3;
						num5--;
					}
					else if (HasAvoidNeighbor(streetTile2, district3))
					{
						continue;
					}
					num4++;
					streetTile2.District = district3;
					streetTile2.Used = true;
					streetTile2.SetPathingConstraintsForTile(allBlocked: true);
				}
			}
		}
		foreach (StreetTile item2 in list2)
		{
			if (item2.District == null)
			{
				item2.SetTownship(null);
				_township.Streets.Remove(item2.GridPosition);
			}
		}
		GameRandomManager.Instance.FreeGameRandom(gameRandom);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public StreetTile FindFreeWithNeighbor(List<StreetTile> _streetTiles, District _district)
	{
		foreach (StreetTile _streetTile in _streetTiles)
		{
			if (_streetTile.District != _district)
			{
				continue;
			}
			StreetTile[] neighbors = _streetTile.GetNeighbors();
			foreach (StreetTile streetTile in neighbors)
			{
				if (streetTile.District == null && _streetTiles.IndexOf(streetTile) >= 0 && !HasAvoidNeighbor(streetTile, _district))
				{
					return streetTile;
				}
			}
		}
		foreach (StreetTile _streetTile2 in _streetTiles)
		{
			if (_streetTile2.District != _district)
			{
				continue;
			}
			StreetTile[] neighbors = _streetTile2.GetNeighborsDiagonal();
			foreach (StreetTile streetTile2 in neighbors)
			{
				if (streetTile2.District == null && _streetTiles.IndexOf(streetTile2) >= 0 && !HasAvoidNeighbor(streetTile2, _district))
				{
					return streetTile2;
				}
			}
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool HasAvoidNeighbor(StreetTile _st, District _district)
	{
		if (_district.avoidedNeighborDistricts.Count > 0)
		{
			StreetTile[] neighbors = _st.GetNeighbors();
			foreach (StreetTile streetTile in neighbors)
			{
				if (streetTile.District != null && _district.avoidedNeighborDistricts.Contains(streetTile.District.name))
				{
					return true;
				}
			}
		}
		return false;
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
}
