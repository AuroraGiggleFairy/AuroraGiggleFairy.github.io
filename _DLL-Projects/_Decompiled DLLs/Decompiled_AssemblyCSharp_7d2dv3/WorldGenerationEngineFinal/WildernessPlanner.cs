using System.Collections.Generic;
using UniLinq;
using UnityEngine;

namespace WorldGenerationEngineFinal;

public class WildernessPlanner
{
	public class WildernessPathInfo
	{
		public Vector2i Position;

		public float PathRadius;

		public Path Path;

		public float highwayDistance;

		public Vector2 highwayPoint;

		public WildernessPathInfo(Vector2i _startPos, float _pathRadius)
		{
			Position = _startPos;
			PathRadius = _pathRadius;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cWildernessSpawnTries = 20;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly WorldBuilder worldBuilder;

	public readonly List<WildernessPathInfo> WildernessPathInfos = new List<WildernessPathInfo>();

	public WildernessPlanner(WorldBuilder _worldBuilder)
	{
		worldBuilder = _worldBuilder;
	}

	public void AddPathInfo(Vector2i _startPos, float _pathRadius)
	{
		WildernessPathInfos.Add(new WildernessPathInfo(new Vector2i(_startPos), _pathRadius));
	}

	public void Plan(DynamicProperties thisWorldProperties, int worldSeed)
	{
		MicroStopwatch microStopwatch = new MicroStopwatch(_bStart: true);
		WildernessPathInfos.Clear();
		int num = 0;
		List<StreetTile> list = new List<StreetTile>(200);
		int seed = worldSeed + 409651;
		GameRandom gameRandom = GameRandomManager.Instance.CreateGameRandom(seed);
		for (int i = 0; i < 5; i++)
		{
			int count = worldBuilder.GetCount(((BiomeType)i/*cast due to .constrained prefix*/).ToString() + "_wilderness", worldBuilder.Wilderness);
			if (count < 0)
			{
				count = worldBuilder.GetCount("wilderness", worldBuilder.Wilderness);
			}
			int num2 = count;
			if (num2 < 0)
			{
				num2 = 200;
				Log.Warning("No wilderness settings in rwgmixer for this world size, using default count of {0}", num2);
			}
			int num3 = num2;
			List<StreetTile> unusedWildernessTiles = GetUnusedWildernessTiles((BiomeType)i);
			while (num2 > 0)
			{
				GetUnusedWildernessTiles(unusedWildernessTiles, list);
				if (list.Count == 0)
				{
					break;
				}
				worldBuilder.SetTaskMessage(string.Format(worldBuilder.messageWildernessPOIs, Mathf.FloorToInt(100f * (1f - (float)num2 / (float)num3))));
				for (int j = 0; j < 20; j++)
				{
					StreetTile streetTile = list[GetLowBiasedRandom(gameRandom, list.Count)];
					if (!streetTile.Used && streetTile.SpawnPrefabs())
					{
						streetTile.Used = true;
						break;
					}
					num++;
				}
				num2--;
			}
		}
		GameRandomManager.Instance.FreeGameRandom(gameRandom);
		Log.Out($"WildernessPlanner Plan {worldBuilder.WildernessPrefabCount} prefabs spawned, in {(float)microStopwatch.ElapsedMilliseconds * 0.001f}, retries {num}, r={Rand.Instance.PeekSample():x}");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int GetLowBiasedRandom(GameRandom rnd, int max)
	{
		float randomFloat = rnd.RandomFloat;
		return (int)(randomFloat * randomFloat * (float)max);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<StreetTile> GetUnusedWildernessTiles(BiomeType _biome)
	{
		return (from StreetTile st in worldBuilder.StreetTileMap
			where !st.OverlapsRadiation && !st.AllIsWater && (st.District == null || st.District.name == "wilderness") && !st.Used && st.BiomeType == _biome
			select st).ToList();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GetUnusedWildernessTiles(List<StreetTile> _list, List<StreetTile> _resultList)
	{
		IOrderedEnumerable<StreetTile> collection = from StreetTile st in _list
			where !st.Used
			orderby distanceFromClosestTownship(st) descending
			select st;
		_resultList.Clear();
		_resultList.AddRange(collection);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int distanceFromClosestTownship(StreetTile st)
	{
		int num = int.MaxValue;
		foreach (Township township in worldBuilder.Townships)
		{
			int num2 = Vector2i.DistanceSqrInt(st.WorldPositionCenter, worldBuilder.GetStreetTileGrid(township.GridCenter).WorldPositionCenter);
			if (num2 < num)
			{
				num = num2;
			}
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float distanceSqr(Vector2 pointA, Vector2 pointB)
	{
		Vector2 vector = pointA - pointB;
		return vector.x * vector.x + vector.y * vector.y;
	}
}
