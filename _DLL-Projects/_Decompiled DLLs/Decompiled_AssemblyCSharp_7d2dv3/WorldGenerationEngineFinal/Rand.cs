using UnityEngine;

namespace WorldGenerationEngineFinal;

public class Rand
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static Rand instance;

	public GameRandom gameRandom;

	public static Rand Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new Rand();
			}
			return instance;
		}
	}

	public Rand()
	{
		gameRandom = GameRandomManager.Instance.CreateGameRandom();
	}

	public Rand(int seed)
	{
		gameRandom = GameRandomManager.Instance.CreateGameRandom();
		SetSeed(seed);
	}

	public void Cleanup()
	{
		GameRandomManager.Instance.FreeGameRandom(gameRandom);
		instance = null;
	}

	public void Free()
	{
		GameRandomManager.Instance.FreeGameRandom(gameRandom);
	}

	public void SetSeed(int seed)
	{
		gameRandom.SetSeed(seed);
	}

	public float Float()
	{
		return gameRandom.RandomFloat;
	}

	public int Int()
	{
		return gameRandom.RandomInt;
	}

	public int Range(int min, int max)
	{
		return gameRandom.RandomRange(min, max);
	}

	public int Range(int max)
	{
		return gameRandom.RandomRange(max);
	}

	public float Range(float min, float max)
	{
		return gameRandom.RandomRange(min, max);
	}

	public int Angle()
	{
		return gameRandom.RandomRange(360);
	}

	public Vector2 RandomOnUnitCircle()
	{
		return gameRandom.RandomOnUnitCircle;
	}

	public int PeekSample()
	{
		return gameRandom.PeekSample();
	}
}
