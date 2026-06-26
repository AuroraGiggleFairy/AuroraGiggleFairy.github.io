using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldGenerationEngineFinal;

public class WildernessPathPlanner
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class WildernessConnectionNode
	{
		public WildernessConnectionNode next;

		public WorldBuilder.WildernessPathInfo PathInfo;

		public Path Path;

		public float Distance;

		public WildernessConnectionNode(WorldBuilder.WildernessPathInfo wpi)
		{
			PathInfo = wpi;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly WorldBuilder worldBuilder;

	public WildernessPathPlanner(WorldBuilder _worldBuilder)
	{
		worldBuilder = _worldBuilder;
	}

	public IEnumerator Plan(int worldSeed)
	{
		MicroStopwatch microStopwatch = new MicroStopwatch(_bStart: true);
		bool flag = worldBuilder.paths.Count > 0;
		for (int i = 0; i < worldBuilder.WildernessPlanner.WildernessPathInfos.Count; i++)
		{
			WorldBuilder.WildernessPathInfo wildernessPathInfo = worldBuilder.WildernessPlanner.WildernessPathInfos[i];
			if (wildernessPathInfo.Path != null)
			{
				continue;
			}
			Vector2i endPosition = Vector2i.zero;
			float num = float.MaxValue;
			bool connectsToHighway = false;
			foreach (Path path2 in worldBuilder.paths)
			{
				foreach (Vector2 finalPathPoint in path2.FinalPathPoints)
				{
					float num2 = Vector2i.Distance(wildernessPathInfo.Position, new Vector2i(finalPathPoint));
					if (num2 < num)
					{
						num = num2;
						endPosition.x = (int)finalPathPoint.x;
						endPosition.y = (int)finalPathPoint.y;
						connectsToHighway = true;
					}
				}
			}
			foreach (WorldBuilder.WildernessPathInfo wildernessPathInfo2 in worldBuilder.WildernessPlanner.WildernessPathInfos)
			{
				if (wildernessPathInfo2 == wildernessPathInfo)
				{
					continue;
				}
				if (wildernessPathInfo2.Path == null)
				{
					if (!flag)
					{
						float num3 = Vector2i.Distance(wildernessPathInfo.Position, wildernessPathInfo2.Position);
						if (num3 < num)
						{
							num = num3;
							endPosition = wildernessPathInfo2.Position;
						}
					}
				}
				else
				{
					if (!wildernessPathInfo2.Path.connectsToHighway)
					{
						continue;
					}
					foreach (Vector2 finalPathPoint2 in wildernessPathInfo2.Path.FinalPathPoints)
					{
						float num4 = Vector2i.Distance(wildernessPathInfo.Position, new Vector2i(finalPathPoint2));
						if (num4 < num)
						{
							num = num4;
							endPosition.x = (int)finalPathPoint2.x;
							endPosition.y = (int)finalPathPoint2.y;
						}
					}
				}
			}
			if (!(num >= float.MaxValue))
			{
				worldBuilder.IsMessageElapsed();
				Path path = new Path(worldBuilder, wildernessPathInfo.Position, endPosition, wildernessPathInfo.PathRadius, _isCountryRoad: true);
				if (path.IsValid)
				{
					path.connectsToHighway = connectsToHighway;
					wildernessPathInfo.Path = path;
					worldBuilder.wildernessPaths.Add(path);
					createTraderSpawnIfAble(path.FinalPathPoints);
				}
			}
		}
		Log.Out($"WildernessPathPlanner Plan #{worldBuilder.WildernessPlanner.WildernessPathInfos.Count} in {(float)microStopwatch.ElapsedMilliseconds * 0.001f}, r={Rand.Instance.PeekSample():x}");
		yield return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void createTraderSpawnIfAble(List<Vector2> pathPoints)
	{
		if (pathPoints.Count < 5)
		{
			return;
		}
		if (worldBuilder.ForestBiomeWeight > 0)
		{
			BiomeType biomeType = BiomeType.none;
			for (int i = 2; i < pathPoints.Count - 2; i++)
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
		for (int j = 2; j < pathPoints.Count - 2; j++)
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

	public IEnumerator Plan2(int worldSeed)
	{
		yield return worldBuilder.SetMessage(Localization.Get("xuiRwgWildernessPaths"));
		List<List<WorldBuilder.WildernessPathInfo>> list = new List<List<WorldBuilder.WildernessPathInfo>>();
		for (byte b = 0; b < 4; b++)
		{
			list.Add(new List<WorldBuilder.WildernessPathInfo>());
			foreach (WorldBuilder.WildernessPathInfo wildernessPathInfo in worldBuilder.WildernessPlanner.WildernessPathInfos)
			{
				if ((uint)wildernessPathInfo.Biome == b)
				{
					list[b].Add(wildernessPathInfo);
				}
			}
		}
		for (byte b2 = 0; b2 < list.Count; b2++)
		{
			List<WorldBuilder.WildernessPathInfo> list2 = list[b2];
			Shuffle(worldSeed + "CountryRoadPlanner.Plan".GetHashCode(), ref list2);
			if (list2.Count != 0)
			{
				for (WildernessConnectionNode wildernessConnectionNode = primsAlgo(list2[0]); wildernessConnectionNode != null; wildernessConnectionNode = wildernessConnectionNode.next)
				{
					if (wildernessConnectionNode.Path != null)
					{
						worldBuilder.wildernessPaths.Add(wildernessConnectionNode.Path);
					}
					if (wildernessConnectionNode.next == null)
					{
						break;
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public WildernessConnectionNode primsAlgo(WorldBuilder.WildernessPathInfo startingWildernessPOI, bool onlyNonConnected = false)
	{
		List<WorldBuilder.WildernessPathInfo> list = new List<WorldBuilder.WildernessPathInfo>();
		WildernessConnectionNode wildernessConnectionNode = new WildernessConnectionNode(startingWildernessPOI);
		WildernessConnectionNode wildernessConnectionNode2 = wildernessConnectionNode;
		Vector2i endPosition = Vector2i.zero;
		while (wildernessConnectionNode2 != null)
		{
			int num = 262144;
			bool flag = false;
			WorldBuilder.WildernessPathInfo wildernessPathInfo = null;
			Vector2i position = wildernessConnectionNode2.PathInfo.Position;
			foreach (WorldBuilder.WildernessPathInfo wildernessPathInfo2 in worldBuilder.WildernessPlanner.WildernessPathInfos)
			{
				if (!list.Contains(wildernessPathInfo2))
				{
					int num2 = Vector2i.DistanceSqrInt(wildernessPathInfo2.Position, position);
					if (num2 < num && worldBuilder.PathingUtils.HasValidPath(position, wildernessPathInfo2.Position, isCountryRoad: true))
					{
						endPosition = wildernessPathInfo2.Position;
						num = num2;
						wildernessPathInfo = wildernessPathInfo2;
						flag = true;
					}
				}
			}
			if (!flag)
			{
				wildernessConnectionNode2 = wildernessConnectionNode2.next;
				continue;
			}
			wildernessConnectionNode2.Path = new Path(worldBuilder, position, endPosition, wildernessConnectionNode2.PathInfo.PathRadius, _isCountryRoad: true);
			if (!wildernessConnectionNode2.Path.IsValid)
			{
				wildernessConnectionNode2.Path = null;
				wildernessConnectionNode2 = wildernessConnectionNode2.next;
			}
			else
			{
				list.Add(wildernessPathInfo);
				wildernessConnectionNode2.next = new WildernessConnectionNode(wildernessPathInfo);
				wildernessConnectionNode2 = wildernessConnectionNode2.next;
			}
		}
		return wildernessConnectionNode;
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
}
