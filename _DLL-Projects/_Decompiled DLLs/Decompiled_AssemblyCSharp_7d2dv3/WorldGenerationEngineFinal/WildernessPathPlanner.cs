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

	public void Plan(int worldSeed)
	{
		MicroStopwatch microStopwatch = new MicroStopwatch(_bStart: true);
		bool flag = worldBuilder.highwayPaths.Count > 0;
		List<WildernessPlanner.WildernessPathInfo> wildernessPathInfos = worldBuilder.WildernessPlanner.WildernessPathInfos;
		for (int i = 0; i < wildernessPathInfos.Count; i++)
		{
			WildernessPlanner.WildernessPathInfo wildernessPathInfo = wildernessPathInfos[i];
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
				for (int j = 0; j < highwayPath.FinalPathPoints.Length; j += num2)
				{
					Vector2 vector = highwayPath.FinalPathPoints[j];
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
			worldBuilder.SetMessage(string.Format(worldBuilder.messageWildernessPaths, i * 50 / wildernessPathInfos.Count));
		}
		wildernessPathInfos.Sort([PublicizedFrom(EAccessModifier.Internal)] (WildernessPlanner.WildernessPathInfo wp1, WildernessPlanner.WildernessPathInfo wp2) =>
		{
			float num7 = ((wp1.PathRadius < 2.4f) ? wp1.PathRadius : 3f);
			float num8 = ((wp2.PathRadius < 2.4f) ? wp2.PathRadius : 3f);
			return (num7 != num8) ? num8.CompareTo(num7) : wp1.highwayDistance.CompareTo(wp2.highwayDistance);
		});
		for (int num3 = 0; num3 < wildernessPathInfos.Count; num3++)
		{
			WildernessPlanner.WildernessPathInfo wildernessPathInfo2 = wildernessPathInfos[num3];
			if (wildernessPathInfo2.Path != null)
			{
				continue;
			}
			Vector2i vector2i = Vector2i.zero;
			float num4 = float.MaxValue;
			bool connectsToHighway = false;
			if (wildernessPathInfo2.highwayDistance >= 2f && wildernessPathInfo2.highwayDistance < 999999f)
			{
				num4 = wildernessPathInfo2.highwayDistance;
				vector2i.x = (int)wildernessPathInfo2.highwayPoint.x;
				vector2i.y = (int)wildernessPathInfo2.highwayPoint.y;
				connectsToHighway = true;
			}
			foreach (WildernessPlanner.WildernessPathInfo item in wildernessPathInfos)
			{
				if (item == wildernessPathInfo2)
				{
					continue;
				}
				Vector2 _destPoint2;
				int _cost;
				if (item.Path == null)
				{
					if (!flag)
					{
						float num5 = Vector2i.Distance(wildernessPathInfo2.Position, item.Position);
						if (num5 < num4)
						{
							num4 = num5;
							vector2i = item.Position;
						}
					}
				}
				else if (item.Path.connectsToHighway && item.PathRadius >= wildernessPathInfo2.PathRadius && FindShortestPathPointToPathTo(item.Path.FinalPathPoints, wildernessPathInfo2.Position.AsVector2(), out _destPoint2, out _cost))
				{
					float num6 = _cost;
					if (num6 < num4)
					{
						num4 = num6;
						vector2i.x = (int)_destPoint2.x;
						vector2i.y = (int)_destPoint2.y;
					}
				}
			}
			worldBuilder.SetMessage(string.Format(worldBuilder.messageWildernessPaths, num3 * 50 / wildernessPathInfos.Count + 50));
			if (!(num4 > 999999f))
			{
				Path path = new Path(worldBuilder, wildernessPathInfo2.Position, vector2i, wildernessPathInfo2.PathRadius, _isCountryRoad: true);
				if (path.IsValid)
				{
					path.connectsToHighway = connectsToHighway;
					wildernessPathInfo2.Path = path;
					worldBuilder.wildernessPaths.Add(path);
					createTraderSpawnIfAble(path.FinalPathPoints);
				}
				else
				{
					path.Cleanup();
					worldBuilder.AddPreviewLinePlus(wildernessPathInfo2.Position, new Color(1f, 0.3f, 1f), 90);
					worldBuilder.AddPreviewLinePlus(vector2i, new Color(1f, 0.3f, 0.5f), 70);
					Log.Warning($"WildernessPathPlanner Plan index {num3} no path ({wildernessPathInfo2.Position} to {vector2i})");
				}
			}
		}
		Log.Out($"WildernessPathPlanner Plan #{wildernessPathInfos.Count} in {(float)microStopwatch.ElapsedMilliseconds * 0.001f}, r={Rand.Instance.PeekSample():x}");
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
