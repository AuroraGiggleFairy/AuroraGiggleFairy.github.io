using System.Collections;
using System.Collections.Generic;
using UniLinq;
using UnityEngine;

namespace WorldGenerationEngineFinal;

public class WildernessPlanner
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int maxWildernessSpawnTries = 20;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly WorldBuilder worldBuilder;

	public readonly List<WorldBuilder.WildernessPathInfo> WildernessPathInfos = new List<WorldBuilder.WildernessPathInfo>();

	public WildernessPlanner(WorldBuilder _worldBuilder)
	{
		worldBuilder = _worldBuilder;
	}

	public IEnumerator Plan(DynamicProperties thisWorldProperties, int worldSeed)
	{
		yield return null;
		int count = worldBuilder.GetCount("wilderness", worldBuilder.Wilderness);
		int tries = 20;
		MicroStopwatch ms = new MicroStopwatch(_bStart: true);
		int wildernessPOIsLeft = count;
		if (wildernessPOIsLeft == 0)
		{
			wildernessPOIsLeft = 200;
			Log.Warning("No wilderness settings in rwgmixer for this world size, using default count of {0}", wildernessPOIsLeft);
		}
		int totalWildernessPOIs = wildernessPOIsLeft;
		GetUnusedWildernessTiles();
		WildernessPathInfos.Clear();
		int seed = worldSeed + 409651;
		GameRandom rnd = GameRandomManager.Instance.CreateGameRandom(seed);
		while (wildernessPOIsLeft > 0)
		{
			List<StreetTile> validWildernessTiles = GetUnusedWildernessTiles();
			if (validWildernessTiles.Count == 0)
			{
				break;
			}
			if (tries <= 0)
			{
				wildernessPOIsLeft--;
				tries = 20;
			}
			if (worldBuilder.IsMessageElapsed())
			{
				yield return worldBuilder.SetMessage(string.Format(Localization.Get("xuiRwgWildernessPOIs"), Mathf.FloorToInt(100f * (1f - (float)wildernessPOIsLeft / (float)totalWildernessPOIs))));
			}
			StreetTile streetTile = validWildernessTiles[getLowBiasedRandom(rnd, 0, validWildernessTiles.Count)];
			if (!streetTile.Used && streetTile.SpawnPrefabs())
			{
				streetTile.Used = true;
				tries = 0;
			}
			else
			{
				tries--;
			}
		}
		GameRandomManager.Instance.FreeGameRandom(rnd);
		WildernessPathInfos.Sort([PublicizedFrom(EAccessModifier.Internal)] (WorldBuilder.WildernessPathInfo wp1, WorldBuilder.WildernessPathInfo wp2) => wp2.PathRadius.CompareTo(wp1.PathRadius));
		Log.Out($"WildernessPlanner Plan {worldBuilder.WildernessPrefabCount} prefabs spawned, in {(float)ms.ElapsedMilliseconds * 0.001f}, r={Rand.Instance.PeekSample():x}");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int getLowBiasedRandom(GameRandom rnd, int min, int max)
	{
		return Mathf.FloorToInt(Mathf.Abs(rnd.RandomRange(0f, 1f) - rnd.RandomRange(0f, 1f)) * (1f + (float)(max - 1) - (float)min) + (float)min);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<StreetTile> GetUnusedWildernessTiles()
	{
		new Vector2i(worldBuilder.WorldSize / 2, worldBuilder.WorldSize / 2);
		return (from StreetTile st in worldBuilder.StreetTileMap
			where !st.OverlapsRadiation && !st.AllIsWater && (st.District == null || st.District.name == "wilderness") && !st.Used && !hasTownshipNeighbor(st) && !hasPrefabNeighbor(st)
			orderby distanceFromClosestTownship(st) descending
			select st).ToList();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool hasTownshipNeighbor(StreetTile st)
	{
		StreetTile[] neighbors8way = st.GetNeighbors8way();
		foreach (StreetTile streetTile in neighbors8way)
		{
			if (streetTile.Township != null && streetTile.Township.GetTypeName() != "wilderness")
			{
				return true;
			}
		}
		return false;
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
	public static bool hasPrefabNeighbor(StreetTile st)
	{
		StreetTile[] neighbors8way = st.GetNeighbors8way();
		for (int i = 0; i < neighbors8way.Length; i++)
		{
			if (neighbors8way[i].HasPrefabs)
			{
				return true;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 getClosestConnectionPosition(Vector2 startPos, int _wildernessId, float _radius = 4f, BiomeType _biome = BiomeType.forest)
	{
		float num = 2.1474836E+09f;
		Vector2 result = Vector2.zero;
		bool flag = false;
		if (worldBuilder.paths.Count > 0)
		{
			foreach (Path path in worldBuilder.paths)
			{
				foreach (Vector2 finalPathPoint in path.FinalPathPoints)
				{
					float num2 = distanceSqr(startPos, finalPathPoint);
					if (num2 < num)
					{
						num = num2;
						result = finalPathPoint;
						flag = true;
					}
				}
			}
		}
		if (worldBuilder.wildernessPaths.Count > 0)
		{
			foreach (Path wildernessPath in worldBuilder.wildernessPaths)
			{
				if ((wildernessPath.IsPrefabPath && worldBuilder.Townships.Count > 0) || wildernessPath.StartPointID == _wildernessId || wildernessPath.EndPointID == _wildernessId || wildernessPath.radius < _radius)
				{
					continue;
				}
				for (int i = 2; i < wildernessPath.FinalPathPoints.Count - 2; i++)
				{
					Vector2 vector = wildernessPath.FinalPathPoints[i];
					float num3 = distanceSqr(startPos, vector);
					if (num3 < num && _biome == worldBuilder.GetBiome((int)vector.x, (int)vector.y))
					{
						num = num3;
						result = vector;
						flag = true;
					}
				}
			}
		}
		if (flag)
		{
			return result;
		}
		return startPos;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int getClosestConnectionDirection(Vector2 startPos, int _wildernessId, float _radius = 4f, BiomeType _biome = BiomeType.forest)
	{
		Vector2 closestConnectionPosition = getClosestConnectionPosition(startPos, _wildernessId, _radius, _biome);
		closestConnectionPosition -= startPos;
		if (closestConnectionPosition.x + closestConnectionPosition.y != 0f)
		{
			closestConnectionPosition.Normalize();
			if (Mathf.Abs(closestConnectionPosition.x) > Mathf.Abs(closestConnectionPosition.y))
			{
				if (closestConnectionPosition.x > 0f)
				{
					return 1;
				}
				return 3;
			}
			if (closestConnectionPosition.y > 0f)
			{
				return 0;
			}
			return 2;
		}
		return -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float distanceSqr(Vector2 pointA, Vector2 pointB)
	{
		Vector2 vector = pointA - pointB;
		return vector.x * vector.x + vector.y * vector.y;
	}
}
