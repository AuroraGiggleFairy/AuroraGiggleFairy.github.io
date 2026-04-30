using System.Diagnostics;
using UnityEngine;

public class GameRandomManager
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static GameRandomManager instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public int baseSeed;

	[PublicizedFrom(EAccessModifier.Private)]
	public MemoryPooledObject<GameRandom> pool = new MemoryPooledObject<GameRandom>(30);

	[PublicizedFrom(EAccessModifier.Private)]
	public GameRandom tempRandom;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cLogMax = 10;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float logTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int logCount;

	public static GameRandomManager Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new GameRandomManager();
				instance.SetBaseSeed((int)Stopwatch.GetTimestamp());
				instance.tempRandom = new GameRandom();
			}
			return instance;
		}
	}

	public int BaseSeed => baseSeed;

	public void SetBaseSeed(int _baseSeed)
	{
		baseSeed = _baseSeed;
	}

	public GameRandom CreateGameRandom()
	{
		return CreateGameRandom(baseSeed);
	}

	public GameRandom CreateGameRandom(int _seed)
	{
		GameRandom gameRandom = pool.AllocSync(_bReset: false);
		gameRandom.SetSeed(_seed);
		return gameRandom;
	}

	public void FreeGameRandom(GameRandom _gameRandom)
	{
		if (_gameRandom != null)
		{
			pool.FreeSync(_gameRandom);
		}
	}

	public GameRandom GetTempGameRandom(int _seed)
	{
		tempRandom.SetSeed(_seed);
		return tempRandom;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void log(string _format, params object[] _values)
	{
		float num = -1f;
		if (ThreadManager.IsMainThread())
		{
			num = Time.time;
			if (num != logTime)
			{
				if (logCount > 10)
				{
					Log.Warning("GameRandomManager {0} more...", logCount - 10);
				}
				logTime = num;
				logCount = 0;
			}
			if (++logCount > 10)
			{
				return;
			}
		}
		Log.Warning($"{num.ToCultureInvariantString()} GameRandomManager " + _format, _values);
	}
}
