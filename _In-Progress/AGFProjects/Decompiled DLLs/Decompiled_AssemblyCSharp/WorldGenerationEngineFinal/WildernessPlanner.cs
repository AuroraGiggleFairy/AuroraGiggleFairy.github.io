using System.Collections;
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

	public IEnumerator Plan(DynamicProperties thisWorldProperties, int worldSeed)
	{
		yield return null;
		MicroStopwatch ms = new MicroStopwatch(_bStart: true);
		WildernessPathInfos.Clear();
		int retries = 0;
		List<StreetTile> validTiles = new List<StreetTile>(200);
		int seed = worldSeed + 409651;
		GameRandom rnd = GameRandomManager.Instance.CreateGameRandom(seed);
		int n = 0;
		while (n < 5)
		{
			int count = worldBuilder.GetCount(((BiomeType)n/*cast due to .constrained prefix*/).ToString() + "_wilderness", worldBuilder.Wilderness);
			if (count < 0)
			{
				count = worldBuilder.GetCount("wilderness", worldBuilder.Wilderness);
			}
			int poisLeft = count;
			if (poisLeft < 0)
			{
				poisLeft = 200;
				Log.Warning("No wilderness settings in rwgmixer for this world size, using default count of {0}", poisLeft);
			}
			int poisTotal = poisLeft;
			List<StreetTile> biomeTiles = GetUnusedWildernessTiles((BiomeType)n);
			int num;
			while (poisLeft > 0)
			{
				GetUnusedWildernessTiles(biomeTiles, validTiles);
				if (validTiles.Count == 0)
				{
					break;
				}
				for (int tries = 0; tries < 20; tries = num)
				{
					if (worldBuilder.IsMessageElapsed())
					{
						yield return worldBuilder.SetMessage(string.Format(Localization.Get("xuiRwgWildernessPOIs"), Mathf.FloorToInt(100f * (1f - (float)poisLeft / (float)poisTotal))));
					}
					StreetTile streetTile = validTiles[GetLowBiasedRandom(rnd, validTiles.Count)];
					if (!streetTile.Used && streetTile.SpawnPrefabs())
					{
						streetTile.Used = true;
						break;
					}
					retries++;
					num = tries + 1;
				}
				poisLeft--;
			}
			num = n + 1;
			n = num;
		}
		GameRandomManager.Instance.FreeGameRandom(rnd);
		Log.Out($"WildernessPlanner Plan {worldBuilder.WildernessPrefabCount} prefabs spawned, in {(float)ms.ElapsedMilliseconds * 0.001f}, retries {retries}, r={Rand.Instance.PeekSample():x}");
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
