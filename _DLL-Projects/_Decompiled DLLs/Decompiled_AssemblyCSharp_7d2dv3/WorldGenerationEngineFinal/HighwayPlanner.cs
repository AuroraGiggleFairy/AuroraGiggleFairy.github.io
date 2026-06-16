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

	public void Plan(DynamicProperties thisWorldProperties, int worldSeed)
	{
		worldBuilder.SetTaskMessage(worldBuilder.messageHighways);
		MicroStopwatch microStopwatch = new MicroStopwatch(_bStart: true);
		ExitConnections.Clear();
		List<Township> list = worldBuilder.Townships.FindAll([PublicizedFrom(EAccessModifier.Internal)] (Township _township) => _township.Data.SpawnGateway);
		if (list.Count > 0)
		{
			list.Sort([PublicizedFrom(EAccessModifier.Internal)] (Township _t1, Township _t2) => _t2.Streets.Count.CompareTo(_t1.Streets.Count));
			for (int num = 0; num < list.Count; num++)
			{
				Township township = list[num];
				ConnectClosest(township, list);
				if (township.IsBig())
				{
					ConnectSelf(township);
				}
			}
			Log.Out($"HighwayPlanner Plan townships in {(float)microStopwatch.ElapsedMilliseconds * 0.001f}");
			CleanupHighwayConnections(list);
		}
		RunTownshipDirtRoads();
		Log.Out($"HighwayPlanner Plan in {(float)microStopwatch.ElapsedMilliseconds * 0.001f}, r={Rand.Instance.PeekSample():x}");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CleanupHighwayConnections(List<Township> _townships)
	{
		MicroStopwatch microStopwatch = new MicroStopwatch(_bStart: true);
		List<Vector2i> list = new List<Vector2i>();
		List<Path> list2 = new List<Path>();
		foreach (Township _township in _townships)
		{
			worldBuilder.SetTaskMessage(worldBuilder.messageHighwaysConnections);
			if (!_township.Data.SpawnGateway || _township.Gateways.Count == 0)
			{
				continue;
			}
			foreach (StreetTile gateway in _township.Gateways)
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
						list.Add(streetTileWorld.GridPosition);
					}
					list2.Add(connectedHighway);
				}
				list.Add(gateway.GridPosition);
			}
			foreach (Vector2i item in list)
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
			list.Clear();
			foreach (Path item2 in list2)
			{
				item2.RemoveFromStreetTiles();
				worldBuilder.highwayPaths.Remove(item2);
				item2.Cleanup();
			}
			list2.Clear();
		}
		Log.Out($"HighwayPlanner cleanupHighwayConnections in {(float)microStopwatch.ElapsedMilliseconds * 0.001f}, r={Rand.Instance.PeekSample():x}");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RunTownshipDirtRoads()
	{
		MicroStopwatch microStopwatch = new MicroStopwatch(_bStart: true);
		List<Township> list = worldBuilder.Townships.FindAll([PublicizedFrom(EAccessModifier.Internal)] (Township _township) => !_township.Data.SpawnGateway);
		Vector2i vector2i = default(Vector2i);
		for (int num = 0; num < list.Count; num++)
		{
			Township township = list[num];
			string text = string.Format(worldBuilder.messageHighwaysTownship, num + 1, list.Count);
			worldBuilder.SetTaskMessage(text);
			MicroStopwatch microStopwatch2 = new MicroStopwatch(_bStart: true);
			int num2 = 0;
			foreach (Vector2i unusedTownExit in township.GetUnusedTownExits())
			{
				Vector2i endPosition = Vector2i.zero;
				float num3 = float.MaxValue;
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
						float num4 = Vector2i.DistanceSqr(unusedTownExit, vector2i);
						if (num4 < num3)
						{
							if (worldBuilder.PathingUtils.GetPathCost(unusedTownExit, vector2i, isCountryRoad: true) > 0)
							{
								num3 = num4;
								endPosition = vector2i;
							}
							num2++;
							if (num2 % 10 == 0)
							{
								worldBuilder.SetTaskMessage(text + " " + string.Format(worldBuilder.messageHighwaysTownExits, num2));
							}
						}
					}
				}
				worldBuilder.SetTaskMessage(text);
				Path path = new Path(worldBuilder, unusedTownExit, endPosition, 2, _isCountryRoad: true);
				if (path.IsValid)
				{
					foreach (StreetTile value in township.Streets.Values)
					{
						for (int num5 = 0; num5 < 4; num5++)
						{
							if (Vector2i.Distance(value.GetHighwayExitPos(num5), unusedTownExit) < 10f)
							{
								value.SetExitUsed(num5);
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
			Log.Out($"HighwayPlanner runTownshipDirtRoads #{num} unused exits c{num2} in {(float)microStopwatch2.ElapsedMilliseconds * 0.001f}, r={Rand.Instance.PeekSample():x}");
		}
		Log.Out($"HighwayPlanner runTownshipDirtRoads, countryTownships {list.Count}, in {(float)microStopwatch.ElapsedMilliseconds * 0.001f}, r={Rand.Instance.PeekSample():x}");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ConnectClosest(Township _township, List<Township> highwayTownships)
	{
		for (int i = 0; i < _township.Gateways.Count; i++)
		{
			StreetTile gateway = _township.Gateways[i];
			if (gateway.UsedExitList.Count >= 2)
			{
				continue;
			}
			List<Township> list = highwayTownships.FindAll([PublicizedFrom(EAccessModifier.Internal)] (Township t) => (!_township.TownshipConnectionCounts.TryGetValue(t, out var value3) || value3 <= 1) && t.ID != _township.ID && t.Data.SpawnGateway);
			list.Sort([PublicizedFrom(EAccessModifier.Internal)] (Township _t1, Township _t2) => Vector2i.DistanceSqr(gateway.GridPosition, _t1.GridCenter).CompareTo(Vector2i.DistanceSqr(gateway.GridPosition, _t2.GridCenter)));
			Path path = null;
			Township township = null;
			int num = 0;
			foreach (Township item in list)
			{
				GetPathToTownship(gateway, item);
				Path path2 = getPathToTownshipResult;
				if (path2 == null)
				{
					continue;
				}
				getPathToTownshipResult = null;
				_township.TownshipConnectionCounts.TryGetValue(item, out var value);
				if (_township.Streets.Count <= 1 || item.Streets.Count <= 1)
				{
					if (value > 0)
					{
						path2.Cost *= 4;
					}
				}
				else if (value > 0)
				{
					path2.Cost = (int)((float)path2.Cost * 1.6f);
				}
				if (path == null || path2.Cost < path.Cost)
				{
					path = path2;
					township = item;
				}
				if (path != path2)
				{
					path2.Cleanup();
				}
				if (++num >= 3)
				{
					break;
				}
			}
			if (path != null)
			{
				_township.TownshipConnectionCounts.TryGetValue(township, out var value2);
				_township.TownshipConnectionCounts[township] = value2 + 1;
				township.TownshipConnectionCounts.TryGetValue(_township, out value2);
				township.TownshipConnectionCounts[_township] = value2 + 1;
				worldBuilder.highwayPaths.Add(path);
				SetTileExits(path);
				path.CommitPathingMapData();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ConnectSelf(Township _township)
	{
		_township.SortGatewaysClockwise();
		int count = _township.Gateways.Count;
		for (int i = 0; i < count; i++)
		{
			StreetTile streetTile = _township.Gateways[i];
			if (streetTile.UsedExitList.Count >= 4)
			{
				continue;
			}
			StreetTile streetTile2 = _township.Gateways[(i + 1) % count];
			if (streetTile2.UsedExitList.Count >= 4)
			{
				continue;
			}
			int num = int.MaxValue;
			Path path = null;
			foreach (Vector2i highwayExit in streetTile.GetHighwayExits(_isGateway: true))
			{
				foreach (Vector2i highwayExit2 in streetTile2.GetHighwayExits(_isGateway: true))
				{
					Path path2 = new Path(worldBuilder, highwayExit, highwayExit2, 4, _isCountryRoad: false);
					if (path2.IsValid)
					{
						int cost = path2.Cost;
						if (cost < num)
						{
							num = cost;
							path = path2;
						}
					}
					path2.RemoveFromStreetTiles();
					if (path == null || path != path2)
					{
						path2.Cleanup();
					}
				}
				if (worldBuilder.IsMessageElapsed())
				{
					worldBuilder.SetTaskMessage(string.Format(worldBuilder.messageHighwaysTownExitsSelf, Application.isEditor ? streetTile.Township.GetTypeName() : string.Empty));
				}
			}
			if (path != null)
			{
				worldBuilder.highwayPaths.Add(path);
				SetTileExits(path);
				path.CommitPathingMapData();
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
	public void GetPathToTownship(StreetTile gateway, Township otherTownship)
	{
		int num = int.MaxValue;
		Path path = null;
		foreach (Vector2i highwayExit in gateway.GetHighwayExits(_isGateway: true))
		{
			foreach (Vector2i unusedTownExit in otherTownship.GetUnusedTownExits(3))
			{
				Path path2 = new Path(worldBuilder, highwayExit, unusedTownExit, 4, _isCountryRoad: false);
				if (path2.IsValid)
				{
					int cost = path2.Cost;
					if (cost < num)
					{
						num = cost;
						path = path2;
					}
				}
				path2.RemoveFromStreetTiles();
				if (path == null || path != path2)
				{
					path2.Cleanup();
				}
			}
			worldBuilder.SetTaskMessage(string.Format(worldBuilder.messageHighwaysTownExitsOther, Application.isEditor ? gateway.Township.GetTypeName() : string.Empty));
		}
		getPathToTownshipResult = path;
	}
}
