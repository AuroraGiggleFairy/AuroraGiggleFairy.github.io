using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace WorldGenerationEngineFinal;

public class WildernessPathPlanner
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly WorldBuilder worldBuilder;

	public WildernessPathPlanner(WorldBuilder _worldBuilder)
	{
		worldBuilder = _worldBuilder;
	}

	public IEnumerator Plan(int worldSeed)
	{
		MicroStopwatch ms = new MicroStopwatch(_bStart: true);
		bool hasTowns = worldBuilder.highwayPaths.Count > 0;
		List<WildernessPlanner.WildernessPathInfo> pathInfos = worldBuilder.WildernessPlanner.WildernessPathInfos;
		int n = 0;
		while (n < pathInfos.Count)
		{
			WildernessPlanner.WildernessPathInfo wildernessPathInfo = pathInfos[n];
			Vector2 _startPos = wildernessPathInfo.Position.AsVector2();
			float num = float.MaxValue;
			Vector2 highwayPoint = Vector2.zero;
			foreach (Path highwayPath in worldBuilder.highwayPaths)
			{
				if (!(PathingUtils.FindClosestPathPoint(in highwayPath.FinalPathPoints, in _startPos, out var _destPoint, 5) < 1000000f))
				{
					continue;
				}
				int num2 = Utils.FastMin(5, highwayPath.FinalPathPoints.Length - 1);
				for (int i = 0; i < highwayPath.FinalPathPoints.Length; i += num2)
				{
					Vector2 vector = highwayPath.FinalPathPoints[i];
					if (!((_destPoint - vector).sqrMagnitude > 62500f))
					{
						Vector2i end = new Vector2i(vector);
						int length = worldBuilder.PathingUtils.GetPath(wildernessPathInfo.Position, end, _isCountryRoad: true).Length;
						if (length >= 2 && (float)length < num)
						{
							num = length;
							highwayPoint = vector;
						}
					}
				}
			}
			wildernessPathInfo.highwayDistance = num;
			wildernessPathInfo.highwayPoint = highwayPoint;
			if (worldBuilder.IsMessageElapsed())
			{
				yield return worldBuilder.SetMessage(string.Format(Localization.Get("xuiRwgWildernessPaths"), n * 50 / pathInfos.Count));
			}
			int num3 = n + 1;
			n = num3;
		}
		pathInfos.Sort([PublicizedFrom(EAccessModifier.Internal)] (WildernessPlanner.WildernessPathInfo wp1, WildernessPlanner.WildernessPathInfo wp2) =>
		{
			float num6 = ((wp1.PathRadius < 2.4f) ? wp1.PathRadius : 3f);
			float num7 = ((wp2.PathRadius < 2.4f) ? wp2.PathRadius : 3f);
			return (num6 != num7) ? num7.CompareTo(num6) : wp1.highwayDistance.CompareTo(wp2.highwayDistance);
		});
		n = 0;
		while (n < pathInfos.Count)
		{
			WildernessPlanner.WildernessPathInfo wpi = pathInfos[n];
			if (wpi.Path == null)
			{
				Vector2i closestPoint = Vector2i.zero;
				float closestDist = float.MaxValue;
				bool isHighwayConnected = false;
				if (wpi.highwayDistance >= 2f && wpi.highwayDistance < 999999f)
				{
					closestDist = wpi.highwayDistance;
					closestPoint.x = (int)wpi.highwayPoint.x;
					closestPoint.y = (int)wpi.highwayPoint.y;
					isHighwayConnected = true;
				}
				foreach (WildernessPlanner.WildernessPathInfo item in pathInfos)
				{
					if (item == wpi)
					{
						continue;
					}
					Vector2 _destPoint2;
					int _cost;
					if (item.Path == null)
					{
						if (!hasTowns)
						{
							float num4 = Vector2i.Distance(wpi.Position, item.Position);
							if (num4 < closestDist)
							{
								closestDist = num4;
								closestPoint = item.Position;
							}
						}
					}
					else if (item.Path.connectsToHighway && item.PathRadius >= wpi.PathRadius && FindShortestPathPointToPathTo(item.Path.FinalPathPoints, wpi.Position.AsVector2(), out _destPoint2, out _cost))
					{
						float num5 = _cost;
						if (num5 < closestDist)
						{
							closestDist = num5;
							closestPoint.x = (int)_destPoint2.x;
							closestPoint.y = (int)_destPoint2.y;
						}
					}
				}
				if (worldBuilder.IsMessageElapsed())
				{
					yield return worldBuilder.SetMessage(string.Format(Localization.Get("xuiRwgWildernessPaths"), n * 50 / pathInfos.Count + 50));
				}
				if (!(closestDist > 999999f))
				{
					Path path = new Path(worldBuilder, wpi.Position, closestPoint, wpi.PathRadius, _isCountryRoad: true);
					if (path.IsValid)
					{
						path.connectsToHighway = isHighwayConnected;
						wpi.Path = path;
						worldBuilder.wildernessPaths.Add(path);
						createTraderSpawnIfAble(path.FinalPathPoints);
					}
					else
					{
						path.Cleanup();
						worldBuilder.AddPreviewLinePlus(wpi.Position, new Color(1f, 0.3f, 1f), 90);
						worldBuilder.AddPreviewLinePlus(closestPoint, new Color(1f, 0.3f, 0.5f), 70);
						Log.Warning($"WildernessPathPlanner Plan index {n} no path ({wpi.Position} to {closestPoint})");
					}
				}
			}
			int num3 = n + 1;
			n = num3;
		}
		Log.Out($"WildernessPathPlanner Plan #{pathInfos.Count} in {(float)ms.ElapsedMilliseconds * 0.001f}, r={Rand.Instance.PeekSample():x}");
		yield return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool FindShortestPathPointToPathTo(NativeList<Vector2> _path, Vector2 _startPos, out Vector2 _destPoint, out int _cost)
	{
		_destPoint = Vector2.zero;
		_cost = 0;
		Vector2 _destPoint2 = Vector2.zero;
		if (PathingUtils.FindClosestPathPoint(in _path, in _startPos, out _destPoint2) < 490000f)
		{
			Vector2i pathPoint = worldBuilder.PathingUtils.GetPathPoint(new Vector2i(_startPos), ref _path, _isCountryRoad: true, _isRiver: false, out _cost);
			if (_cost > 0)
			{
				_destPoint.x = pathPoint.x;
				_destPoint.y = pathPoint.y;
				return true;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void createTraderSpawnIfAble(NativeList<Vector2> pathPoints)
	{
		if (pathPoints.Length < 5)
		{
			return;
		}
		if (worldBuilder.ForestBiomeWeight > 0)
		{
			BiomeType biomeType = BiomeType.none;
			for (int i = 2; i < pathPoints.Length - 2; i++)
			{
				biomeType = worldBuilder.GetBiome((int)pathPoints[i].x, (int)pathPoints[i].y);
				if (biomeType == BiomeType.forest)
				{
					break;
				}
			}
			if (biomeType != BiomeType.forest)
			{
				return;
			}
		}
		Vector2i vector2i = default(Vector2i);
		for (int j = 2; j < pathPoints.Length - 2; j++)
		{
			if (worldBuilder.ForestBiomeWeight > 0 && worldBuilder.GetBiome((int)pathPoints[j].x, (int)pathPoints[j].y) != BiomeType.forest)
			{
				continue;
			}
			vector2i.x = (int)pathPoints[j].x;
			vector2i.y = (int)pathPoints[j].y;
			StreetTile streetTileWorld = worldBuilder.GetStreetTileWorld(vector2i);
			if (streetTileWorld == null || !streetTileWorld.HasPrefabs)
			{
				continue;
			}
			bool flag = true;
			foreach (PrefabDataInstance streetTilePrefabData in streetTileWorld.StreetTilePrefabDatas)
			{
				if (streetTilePrefabData.prefab.DifficultyTier > 1)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				worldBuilder.CreatePlayerSpawn(vector2i);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int getMaxTraderDistance()
	{
		return (int)(0.1f * (float)worldBuilder.WorldSize);
	}
}
