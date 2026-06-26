using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldGenerationEngineFinal;

public class HighwayPlanner
{
	public enum CDirs
	{
		North = 0,
		East = 1,
		South = 2,
		West = 3,
		Invalid = -1
	}

	public class ExitConnection
	{
		public StreetTile ParentTile;

		public Vector2i WorldPosition;

		public int ExitDir = -1;

		public Path ConnectedPath;

		public ExitConnection(StreetTile parent, Vector2i worldPos, Path connectedPath = null)
		{
			ParentTile = parent;
			WorldPosition = worldPos;
			ConnectedPath = connectedPath;
			for (int i = 0; i < 4; i++)
			{
				if (parent.getHighwayExitPosition(i) == WorldPosition)
				{
					ExitDir = i;
					break;
				}
			}
			parent.SetExitUsed(WorldPosition);
		}

		public bool SetExitUsedManually()
		{
			if (ParentTile.UsedExitList.Contains(ParentTile.getHighwayExitPosition(ExitDir)) && (ParentTile.ConnectedExits & (1 << ExitDir)) > 0 && ParentTile.RoadExits[ExitDir])
			{
				return true;
			}
			return ParentTile.SetExitUsed(WorldPosition);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly WorldBuilder worldBuilder;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<ExitConnection> ExitConnections = new List<ExitConnection>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Path getPathToTownshipResult;

	public HighwayPlanner(WorldBuilder _worldBuilder)
	{
		worldBuilder = _worldBuilder;
	}

	public IEnumerator Plan(DynamicProperties thisWorldProperties, int worldSeed)
	{
		yield return worldBuilder.SetMessage(Localization.Get("xuiRwgHighways"));
		MicroStopwatch ms = new MicroStopwatch(_bStart: true);
		ExitConnections.Clear();
		foreach (Township township2 in worldBuilder.Townships)
		{
			foreach (StreetTile gateway in township2.Gateways)
			{
				gateway.SetAllExistingNeighborsForGateway();
			}
		}
		List<Township> highwayTownships = worldBuilder.Townships.FindAll([PublicizedFrom(EAccessModifier.Internal)] (Township _township) => WorldBuilderStatic.townshipDatas[_township.GetTypeName()].SpawnGateway);
		if (highwayTownships.Count > 0)
		{
			highwayTownships.Sort([PublicizedFrom(EAccessModifier.Internal)] (Township _t1, Township _t2) => _t2.Streets.Count.CompareTo(_t1.Streets.Count));
			int n = 0;
			while (n < highwayTownships.Count)
			{
				Township township = highwayTownships[n];
				yield return ConnectClosest(township, highwayTownships);
				if (township.IsBig())
				{
					yield return ConnectSelf(township);
				}
				int num = n + 1;
				n = num;
			}
			Log.Out($"HighwayPlanner Plan townships in {(float)ms.ElapsedMilliseconds * 0.001f}");
			yield return cleanupHighwayConnections(highwayTownships);
		}
		yield return runTownshipDirtRoads();
		Log.Out($"HighwayPlanner Plan in {(float)ms.ElapsedMilliseconds * 0.001f}, r={Rand.Instance.PeekSample():x}");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator cleanupHighwayConnections(List<Township> highwayTownships)
	{
		MicroStopwatch ms = new MicroStopwatch(_bStart: true);
		List<Vector2i> tilesToRemove = new List<Vector2i>();
		List<Path> pathsToRemove = new List<Path>();
		foreach (Township highwayTownship in highwayTownships)
		{
			if (highwayTownship.Gateways.Count == 0)
			{
				Log.Error("cleanupHighwayConnections township {0} in {1} has no gateways!", highwayTownship.GetTypeName(), highwayTownship.BiomeType);
				continue;
			}
			foreach (StreetTile gateway in highwayTownship.Gateways)
			{
				for (int i = 0; i < 4; i++)
				{
					StreetTile neighborByIndex = gateway.GetNeighborByIndex(i);
					Vector2i highwayExitPosition = gateway.getHighwayExitPosition(i);
					if (!gateway.UsedExitList.Contains(highwayExitPosition) && (neighborByIndex.Township != gateway.Township || !neighborByIndex.HasExitTo(gateway)))
					{
						gateway.SetExitUnUsed(highwayExitPosition);
					}
				}
			}
		}
		foreach (ExitConnection exitConnection in ExitConnections)
		{
			exitConnection.SetExitUsedManually();
		}
		foreach (Township t in highwayTownships)
		{
			yield return worldBuilder.SetMessage(Localization.Get("xuiRwgHighwaysConnections"));
			if (!WorldBuilderStatic.townshipDatas[t.GetTypeName()].SpawnGateway || t.Gateways.Count == 0)
			{
				continue;
			}
			foreach (StreetTile gateway2 in t.Gateways)
			{
				for (int j = 0; j < 4; j++)
				{
					gateway2.GetNeighborByIndex(j);
					Vector2i highwayExitPosition2 = gateway2.getHighwayExitPosition(j);
					if (!gateway2.UsedExitList.Contains(highwayExitPosition2))
					{
						gateway2.SetExitUnUsed(highwayExitPosition2);
					}
				}
				if (gateway2.UsedExitList.Count >= 2)
				{
					continue;
				}
				for (int k = 0; k < 4; k++)
				{
					StreetTile neighborByIndex2 = gateway2.GetNeighborByIndex(k);
					gateway2.SetExitUnUsed(gateway2.getHighwayExitPosition(k));
					if (neighborByIndex2.Township == gateway2.Township)
					{
						neighborByIndex2.SetExitUnUsed(neighborByIndex2.getHighwayExitPosition(neighborByIndex2.GetNeighborIndex(gateway2)));
					}
				}
				foreach (Path connectedHighway in gateway2.ConnectedHighways)
				{
					StreetTile streetTileWorld;
					if (worldBuilder.GetStreetTileWorld(connectedHighway.StartPosition) == gateway2)
					{
						streetTileWorld = worldBuilder.GetStreetTileWorld(connectedHighway.EndPosition);
						streetTileWorld.SetExitUnUsed(connectedHighway.EndPosition);
					}
					else
					{
						streetTileWorld = worldBuilder.GetStreetTileWorld(connectedHighway.StartPosition);
						streetTileWorld.SetExitUnUsed(connectedHighway.StartPosition);
					}
					if (streetTileWorld.UsedExitList.Count < 2)
					{
						tilesToRemove.Add(streetTileWorld.GridPosition);
					}
					pathsToRemove.Add(connectedHighway);
				}
				tilesToRemove.Add(gateway2.GridPosition);
			}
			foreach (Vector2i item in tilesToRemove)
			{
				StreetTile streetTileGrid = worldBuilder.GetStreetTileGrid(item);
				if (streetTileGrid.Township != null)
				{
					streetTileGrid.Township.Gateways.Remove(streetTileGrid);
					streetTileGrid.Township.Streets.Remove(item);
				}
				streetTileGrid.StreetTilePrefabDatas.Clear();
				streetTileGrid.District = null;
				streetTileGrid.Township = null;
			}
			tilesToRemove.Clear();
			foreach (Path item2 in pathsToRemove)
			{
				item2.Dispose();
				worldBuilder.paths.Remove(item2);
			}
			pathsToRemove.Clear();
		}
		Log.Out($"HighwayPlanner cleanupHighwayConnections in {(float)ms.ElapsedMilliseconds * 0.001f}, r={Rand.Instance.PeekSample():x}");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator runTownshipDirtRoads()
	{
		MicroStopwatch ms = new MicroStopwatch(_bStart: true);
		List<Township> countryTownships = worldBuilder.Townships.FindAll([PublicizedFrom(EAccessModifier.Internal)] (Township _township) => !WorldBuilderStatic.townshipDatas[_township.GetTypeName()].SpawnGateway);
		Vector2i vector2i = default(Vector2i);
		for (int n = 0; n < countryTownships.Count; n++)
		{
			Township t = countryTownships[n];
			string msg = string.Format(Localization.Get("xuiRwgHighwaysTownship"), n + 1, countryTownships.Count);
			yield return worldBuilder.SetMessage(msg);
			MicroStopwatch ms2 = new MicroStopwatch(_bStart: true);
			foreach (StreetTile value in t.Streets.Values)
			{
				if (value.GetNumTownshipNeighbors() != 1)
				{
					continue;
				}
				int num = -1;
				for (int num2 = 0; num2 < 4; num2++)
				{
					StreetTile neighborByIndex = value.GetNeighborByIndex(num2);
					if (neighborByIndex != null && neighborByIndex.Township == value.Township)
					{
						num = num2;
						break;
					}
				}
				switch (num)
				{
				case 0:
					value.SetRoadExit(2, value: true);
					break;
				case 1:
					value.SetRoadExit(3, value: true);
					break;
				case 2:
					value.SetRoadExit(0, value: true);
					break;
				case 3:
					value.SetRoadExit(1, value: true);
					break;
				}
				break;
			}
			ms2.ResetAndRestart();
			int count = 0;
			foreach (Vector2i exit in t.GetUnusedTownExits())
			{
				Vector2i closestPoint = Vector2i.zero;
				float closestDist = float.MaxValue;
				foreach (Path path2 in worldBuilder.paths)
				{
					if (path2.isCountryRoad)
					{
						continue;
					}
					foreach (Vector2 finalPathPoint in path2.FinalPathPoints)
					{
						vector2i.x = Utils.Fastfloor(finalPathPoint.x);
						vector2i.y = Utils.Fastfloor(finalPathPoint.y);
						float num3 = Vector2i.DistanceSqr(exit, vector2i);
						if (num3 < closestDist)
						{
							if (worldBuilder.PathingUtils.HasValidPath(exit, vector2i, isCountryRoad: true))
							{
								closestDist = num3;
								closestPoint = vector2i;
							}
							count++;
							if (worldBuilder.IsMessageElapsed())
							{
								yield return worldBuilder.SetMessage(msg + " " + string.Format(Localization.Get("xuiRwgHighwaysTownExits"), count));
							}
						}
					}
				}
				if (worldBuilder.IsMessageElapsed())
				{
					yield return worldBuilder.SetMessage(msg);
				}
				Path path = new Path(worldBuilder, exit, closestPoint, 2, _isCountryRoad: true);
				if (!path.IsValid)
				{
					continue;
				}
				foreach (StreetTile value2 in t.Streets.Values)
				{
					for (int num4 = 0; num4 < 4; num4++)
					{
						if (Vector2i.Distance(value2.getHighwayExitPosition(num4), exit) < 10f)
						{
							value2.SetExitUsed(exit);
						}
					}
				}
				worldBuilder.paths.Add(path);
			}
			Log.Out($"HighwayPlanner runTownshipDirtRoads #{n} unused exits c{count} in {(float)ms2.ElapsedMilliseconds * 0.001f}, r={Rand.Instance.PeekSample():x}");
		}
		Log.Out($"HighwayPlanner runTownshipDirtRoads, countryTownships {countryTownships.Count}, in {(float)ms.ElapsedMilliseconds * 0.001f}, r={Rand.Instance.PeekSample():x}");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void Shuffle<T>(int seed, ref List<T> list)
	{
		int num = list.Count;
		GameRandom gameRandom = GameRandomManager.Instance.CreateGameRandom(seed);
		while (num > 1)
		{
			num--;
			int index = gameRandom.RandomRange(0, num) % num;
			T value = list[index];
			list[index] = list[num];
			list[num] = value;
		}
		GameRandomManager.Instance.FreeGameRandom(gameRandom);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator ConnectClosest(Township _township, List<Township> highwayTownships)
	{
		for (int n = 0; n < _township.Gateways.Count; n++)
		{
			StreetTile gateway = _township.Gateways[n];
			if (gateway.UsedExitList.Count >= 2)
			{
				continue;
			}
			List<Township> list = highwayTownships.FindAll([PublicizedFrom(EAccessModifier.Internal)] (Township t) => (!_township.TownshipConnectionCounts.TryGetValue(t, out var value3) || value3 <= 1) && t.ID != _township.ID && WorldBuilderStatic.townshipDatas[t.GetTypeName()].SpawnGateway);
			list.Sort([PublicizedFrom(EAccessModifier.Internal)] (Township _t1, Township _t2) => Vector2i.DistanceSqr(gateway.GridPosition, _t1.GridCenter).CompareTo(Vector2i.DistanceSqr(gateway.GridPosition, _t2.GridCenter)));
			Path closePath = null;
			Township closeTownship = null;
			int tries = 0;
			foreach (Township townshipNear in list)
			{
				yield return GetPathToTownship(gateway, townshipNear);
				Path path = getPathToTownshipResult;
				if (path == null)
				{
					continue;
				}
				getPathToTownshipResult = null;
				_township.TownshipConnectionCounts.TryGetValue(townshipNear, out var value);
				if (_township.Streets.Count <= 1 || townshipNear.Streets.Count <= 1)
				{
					if (value > 0)
					{
						path.Cost *= 4;
					}
				}
				else if (value > 0)
				{
					path.Cost = (int)((float)path.Cost * 1.6f);
				}
				if (closePath == null || path.Cost < closePath.Cost)
				{
					closePath = path;
					closeTownship = townshipNear;
				}
				int num = tries + 1;
				tries = num;
				if (num >= 3)
				{
					break;
				}
			}
			if (closePath != null)
			{
				_township.TownshipConnectionCounts.TryGetValue(closeTownship, out var value2);
				_township.TownshipConnectionCounts[closeTownship] = value2 + 1;
				closeTownship.TownshipConnectionCounts.TryGetValue(_township, out value2);
				closeTownship.TownshipConnectionCounts[_township] = value2 + 1;
				worldBuilder.paths.Add(closePath);
				SetTileExits(closePath);
				closePath.commitPathingMapData();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator ConnectSelf(Township _township)
	{
		_township.SortGatewaysClockwise();
		int count = _township.Gateways.Count;
		for (int n = 0; n < count; n++)
		{
			StreetTile gateway = _township.Gateways[n];
			if (gateway.UsedExitList.Count >= 4)
			{
				continue;
			}
			StreetTile gateway2 = _township.Gateways[(n + 1) % count];
			if (gateway2.UsedExitList.Count >= 4)
			{
				continue;
			}
			int closeDist = int.MaxValue;
			Path closePath = null;
			List<Vector2i> highwayExits = gateway.GetHighwayExits(isGateway: true);
			foreach (Vector2i item in highwayExits)
			{
				foreach (Vector2i highwayExit in gateway2.GetHighwayExits(isGateway: true))
				{
					Path path = new Path(worldBuilder, item, highwayExit, 4, _isCountryRoad: false);
					if (path.IsValid)
					{
						int cost = path.Cost;
						if (cost < closeDist)
						{
							closeDist = cost;
							closePath = path;
						}
					}
					path.Dispose();
				}
				if (worldBuilder.IsMessageElapsed())
				{
					yield return worldBuilder.SetMessage(string.Format(Localization.Get("xuiRwgHighwaysTownExitsSelf"), gateway.Township.GetTypeName()));
				}
			}
			if (closePath != null)
			{
				worldBuilder.paths.Add(closePath);
				SetTileExits(closePath);
				closePath.commitPathingMapData();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetTileExits(Path path)
	{
		SetTileExit(path, path.StartPosition);
		SetTileExit(path, path.EndPosition);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetTileExit(Path currentPath, Vector2i exit)
	{
		StreetTile streetTile = worldBuilder.GetStreetTileWorld(exit);
		if (streetTile != null)
		{
			if (streetTile.District != null && streetTile.District.name == "gateway")
			{
				ExitConnections.Add(new ExitConnection(streetTile, exit, currentPath));
				return;
			}
			StreetTile[] neighbors = streetTile.GetNeighbors();
			foreach (StreetTile streetTile2 in neighbors)
			{
				if (streetTile2 != null && streetTile2.District != null && streetTile2.District.name == "gateway")
				{
					ExitConnections.Add(new ExitConnection(streetTile2, exit, currentPath));
					return;
				}
			}
			streetTile = null;
		}
		if (streetTile != null)
		{
			return;
		}
		Township township = null;
		foreach (Township township2 in worldBuilder.Townships)
		{
			if (township2.Area.Contains(exit.AsVector2()))
			{
				township = township2;
				break;
			}
		}
		if (township == null)
		{
			return;
		}
		foreach (StreetTile gateway in township.Gateways)
		{
			for (int j = 0; j < 4; j++)
			{
				if (gateway.getHighwayExitPosition(j) == exit || Vector2i.DistanceSqr(gateway.getHighwayExitPosition(j), exit) < 100f)
				{
					ExitConnections.Add(new ExitConnection(gateway, exit, currentPath));
					return;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int GetDist(Township thisTownship, Township otherTownship, ref int closestDist)
	{
		foreach (Vector2i unusedTownExit in thisTownship.GetUnusedTownExits())
		{
			foreach (Vector2i unusedTownExit2 in otherTownship.GetUnusedTownExits())
			{
				int num = Vector2i.DistanceSqrInt(unusedTownExit, unusedTownExit2);
				if (num < closestDist)
				{
					closestDist = num;
				}
			}
		}
		return closestDist;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator GetPathToTownship(StreetTile gateway, Township otherTownship)
	{
		int closestDist = int.MaxValue;
		Path closestPath = null;
		List<Vector2i> highwayExits = gateway.GetHighwayExits(isGateway: true);
		foreach (Vector2i item in highwayExits)
		{
			foreach (Vector2i unusedTownExit in otherTownship.GetUnusedTownExits(3))
			{
				Path path = new Path(worldBuilder, item, unusedTownExit, 4, _isCountryRoad: false);
				if (path.IsValid)
				{
					int cost = path.Cost;
					if (cost < closestDist)
					{
						closestDist = cost;
						closestPath = path;
					}
				}
				path.Dispose();
			}
			if (worldBuilder.IsMessageElapsed())
			{
				yield return worldBuilder.SetMessage(string.Format(Localization.Get("xuiRwgHighwaysTownExitsOther"), gateway.Township.GetTypeName()));
			}
		}
		getPathToTownshipResult = closestPath;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetClosestPathToTownship(Township thisTownship, Township otherTownship)
	{
		int num = int.MaxValue;
		foreach (Vector2i unusedTownExit in thisTownship.GetUnusedTownExits())
		{
			foreach (Vector2i unusedTownExit2 in otherTownship.GetUnusedTownExits())
			{
				int pathCost = worldBuilder.PathingUtils.GetPathCost(unusedTownExit, unusedTownExit2);
				if (pathCost >= 3 && pathCost < num)
				{
					num = pathCost;
				}
			}
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int distanceSqr(Vector2i pointA, Vector2i pointB)
	{
		Vector2i vector2i = pointA - pointB;
		return vector2i.x * vector2i.x + vector2i.y * vector2i.y;
	}
}
