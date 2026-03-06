using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldGenerationEngineFinal;

public class HighwayPlanner
{
	public class ExitConnection
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public StreetTile ParentTile;

		[PublicizedFrom(EAccessModifier.Private)]
		public int ExitDir;

		public ExitConnection(StreetTile parent, Vector2i worldPos, Path connectedPath = null)
		{
			ParentTile = parent;
			ExitDir = parent.GetHighwayExitDir(worldPos);
			parent.SetExitUsed(ExitDir);
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
		List<Township> highwayTownships = worldBuilder.Townships.FindAll([PublicizedFrom(EAccessModifier.Internal)] (Township _township) => _township.Data.SpawnGateway);
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
	public IEnumerator cleanupHighwayConnections(List<Township> _townships)
	{
		MicroStopwatch ms = new MicroStopwatch(_bStart: true);
		List<Vector2i> tilesToRemove = new List<Vector2i>();
		List<Path> pathsToRemove = new List<Path>();
		foreach (Township t in _townships)
		{
			yield return worldBuilder.SetMessage(Localization.Get("xuiRwgHighwaysConnections"));
			if (!t.Data.SpawnGateway || t.Gateways.Count == 0)
			{
				continue;
			}
			foreach (StreetTile gateway in t.Gateways)
			{
				if (gateway.UsedExitList.Count >= 2)
				{
					continue;
				}
				Log.Warning("HighwayPlanner gateway has only {0} connections , {1}", gateway.UsedExitList.Count, gateway.PrefabName);
				for (int i = 0; i < 4; i++)
				{
					gateway.SetExitUnUsed(i);
					StreetTile neighbor = gateway.GetNeighbor(i);
					if (neighbor.Township == gateway.Township)
					{
						neighbor.SetExitUnUsed((i + 2) & 3);
					}
				}
				foreach (Path connectedHighway in gateway.ConnectedHighways)
				{
					StreetTile streetTileWorld;
					if (worldBuilder.GetStreetTileWorld(connectedHighway.StartPosition) == gateway)
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
				tilesToRemove.Add(gateway.GridPosition);
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
				streetTileGrid.SetTownship(null);
			}
			tilesToRemove.Clear();
			foreach (Path item2 in pathsToRemove)
			{
				item2.RemoveFromStreetTiles();
				worldBuilder.highwayPaths.Remove(item2);
				item2.Cleanup();
			}
			pathsToRemove.Clear();
		}
		Log.Out($"HighwayPlanner cleanupHighwayConnections in {(float)ms.ElapsedMilliseconds * 0.001f}, r={Rand.Instance.PeekSample():x}");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator runTownshipDirtRoads()
	{
		MicroStopwatch ms = new MicroStopwatch(_bStart: true);
		List<Township> countryTownships = worldBuilder.Townships.FindAll([PublicizedFrom(EAccessModifier.Internal)] (Township _township) => !_township.Data.SpawnGateway);
		Vector2i vector2i = default(Vector2i);
		for (int n = 0; n < countryTownships.Count; n++)
		{
			Township t = countryTownships[n];
			string msg = string.Format(Localization.Get("xuiRwgHighwaysTownship"), n + 1, countryTownships.Count);
			yield return worldBuilder.SetMessage(msg);
			MicroStopwatch ms2 = new MicroStopwatch(_bStart: true);
			int count = 0;
			foreach (Vector2i exit in t.GetUnusedTownExits())
			{
				Vector2i closestPoint = Vector2i.zero;
				float closestDist = float.MaxValue;
				foreach (Path highwayPath in worldBuilder.highwayPaths)
				{
					if (highwayPath.isCountryRoad)
					{
						continue;
					}
					foreach (Vector2 finalPathPoint in highwayPath.FinalPathPoints)
					{
						vector2i.x = Utils.Fastfloor(finalPathPoint.x);
						vector2i.y = Utils.Fastfloor(finalPathPoint.y);
						float num = Vector2i.DistanceSqr(exit, vector2i);
						if (num < closestDist)
						{
							if (worldBuilder.PathingUtils.GetPathCost(exit, vector2i, isCountryRoad: true) > 0)
							{
								closestDist = num;
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
				if (path.IsValid)
				{
					foreach (StreetTile value in t.Streets.Values)
					{
						for (int num2 = 0; num2 < 4; num2++)
						{
							if (Vector2i.Distance(value.GetHighwayExitPos(num2), exit) < 10f)
							{
								value.SetExitUsed(num2);
							}
						}
					}
					worldBuilder.highwayPaths.Add(path);
				}
				else
				{
					path.Cleanup();
				}
			}
			Log.Out($"HighwayPlanner runTownshipDirtRoads #{n} unused exits c{count} in {(float)ms2.ElapsedMilliseconds * 0.001f}, r={Rand.Instance.PeekSample():x}");
		}
		Log.Out($"HighwayPlanner runTownshipDirtRoads, countryTownships {countryTownships.Count}, in {(float)ms.ElapsedMilliseconds * 0.001f}, r={Rand.Instance.PeekSample():x}");
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
			List<Township> list = highwayTownships.FindAll([PublicizedFrom(EAccessModifier.Internal)] (Township t) => (!_township.TownshipConnectionCounts.TryGetValue(t, out var value3) || value3 <= 1) && t.ID != _township.ID && t.Data.SpawnGateway);
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
				if (closePath != path)
				{
					path.Cleanup();
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
				worldBuilder.highwayPaths.Add(closePath);
				SetTileExits(closePath);
				closePath.CommitPathingMapData();
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
			List<Vector2i> highwayExits = gateway.GetHighwayExits(_isGateway: true);
			foreach (Vector2i item in highwayExits)
			{
				foreach (Vector2i highwayExit in gateway2.GetHighwayExits(_isGateway: true))
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
					path.RemoveFromStreetTiles();
					if (closePath == null || closePath != path)
					{
						path.Cleanup();
					}
				}
				if (worldBuilder.IsMessageElapsed())
				{
					yield return worldBuilder.SetMessage(string.Format(Localization.Get("xuiRwgHighwaysTownExitsSelf"), Application.isEditor ? gateway.Township.GetTypeName() : string.Empty));
				}
			}
			if (closePath != null)
			{
				worldBuilder.highwayPaths.Add(closePath);
				SetTileExits(closePath);
				closePath.CommitPathingMapData();
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
				if (gateway.GetHighwayExitPos(j) == exit || Vector2i.DistanceSqr(gateway.GetHighwayExitPos(j), exit) < 100f)
				{
					ExitConnections.Add(new ExitConnection(gateway, exit, currentPath));
					return;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator GetPathToTownship(StreetTile gateway, Township otherTownship)
	{
		int closeDist = int.MaxValue;
		Path closePath = null;
		List<Vector2i> highwayExits = gateway.GetHighwayExits(_isGateway: true);
		foreach (Vector2i item in highwayExits)
		{
			foreach (Vector2i unusedTownExit in otherTownship.GetUnusedTownExits(3))
			{
				Path path = new Path(worldBuilder, item, unusedTownExit, 4, _isCountryRoad: false);
				if (path.IsValid)
				{
					int cost = path.Cost;
					if (cost < closeDist)
					{
						closeDist = cost;
						closePath = path;
					}
				}
				path.RemoveFromStreetTiles();
				if (closePath == null || closePath != path)
				{
					path.Cleanup();
				}
			}
			if (worldBuilder.IsMessageElapsed())
			{
				yield return worldBuilder.SetMessage(string.Format(Localization.Get("xuiRwgHighwaysTownExitsOther"), Application.isEditor ? gateway.Township.GetTypeName() : string.Empty));
			}
		}
		getPathToTownshipResult = closePath;
	}
}
