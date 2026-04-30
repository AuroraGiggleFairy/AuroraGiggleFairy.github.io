using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class GameStageDefinition
{
	public class Stage
	{
		public readonly int stageNum;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly List<SpawnGroup> spawnGroups = new List<SpawnGroup>();

		public int Count => spawnGroups.Count;

		public Stage(int _stageNum)
		{
			stageNum = _stageNum;
		}

		public void AddSpawnGroup(SpawnGroup spawn)
		{
			spawnGroups.Add(spawn);
		}

		public SpawnGroup GetSpawnGroup(int index)
		{
			if (index >= 0 && index < spawnGroups.Count)
			{
				return spawnGroups[index];
			}
			return null;
		}
	}

	public class SpawnGroup
	{
		public readonly string groupName;

		public readonly ushort spawnCount;

		public readonly ushort maxAlive;

		public readonly ushort interval;

		public readonly ushort duration;

		public SpawnGroup(string _groupName, int _spawnCount, int _maxAlive, int _interval, int _duration)
		{
			groupName = _groupName;
			spawnCount = (ushort)_spawnCount;
			maxAlive = (ushort)_maxAlive;
			interval = (ushort)_interval;
			duration = (ushort)_duration;
		}
	}

	public static float DifficultyBonus = 1f;

	public static float StartingWeight = 1f;

	public static float DiminishingReturns = 0.5f;

	public static long DaysAliveChangeWhenKilled = 2L;

	public static int LootBonusEvery;

	public static int LootBonusMaxCount;

	public static float LootBonusScale;

	public static int LootWanderingBonusEvery;

	public static float LootWanderingBonusScale;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, GameStageDefinition> gameStages = new Dictionary<string, GameStageDefinition>();

	public string name;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Stage> stages = new List<Stage>();

	public GameStageDefinition(string _name)
	{
		name = _name;
	}

	public static void AddGameStage(GameStageDefinition gameStage)
	{
		gameStage.SortStages();
		gameStages.Add(gameStage.name, gameStage);
	}

	public static GameStageDefinition GetGameStage(string name)
	{
		return gameStages[name];
	}

	public static bool TryGetGameStage(string name, out GameStageDefinition definition)
	{
		return gameStages.TryGetValue(name, out definition);
	}

	public static void Clear()
	{
		gameStages.Clear();
	}

	public void AddStage(Stage stage)
	{
		stages.Add(stage);
	}

	public void SortStages()
	{
		stages.Sort([PublicizedFrom(EAccessModifier.Internal)] (Stage x, Stage y) =>
		{
			int stageNum = x.stageNum;
			return stageNum.CompareTo(y.stageNum);
		});
	}

	public Stage GetStage(int stage)
	{
		if (stages.Count < 1)
		{
			return null;
		}
		if (stage < stages[0].stageNum)
		{
			return null;
		}
		int boundIndex = GetBoundIndex(stages, [PublicizedFrom(EAccessModifier.Internal)] (Stage s) => s.stageNum <= stage);
		boundIndex = Mathf.Clamp(boundIndex, 0, stages.Count - 1);
		return stages[boundIndex];
	}

	public static int GetBoundIndex<T>(IList<T> sortedList, Func<T, bool> f)
	{
		int num = 0;
		int num2 = sortedList.Count;
		while (num + 1 < num2)
		{
			int num3 = (num + num2) / 2;
			if (f(sortedList[num3]))
			{
				num = num3;
			}
			else
			{
				num2 = num3;
			}
		}
		if (num2 < sortedList.Count && f(sortedList[num2]))
		{
			num = num2;
		}
		return num;
	}

	public static int CalcPartyLevel(List<int> playerGameStages)
	{
		float num = 0f;
		playerGameStages.Sort();
		float num2 = StartingWeight;
		for (int num3 = playerGameStages.Count - 1; num3 >= 0; num3--)
		{
			num += (float)playerGameStages[num3] * num2;
			num2 *= DiminishingReturns;
		}
		return Mathf.FloorToInt(num);
	}

	public static int CalcGameStageAround(EntityPlayer player)
	{
		List<EntityPlayer> list = new List<EntityPlayer>();
		player.world.GetPlayersAround(player.position, 100f, list);
		List<int> list2 = new List<int>();
		for (int i = 0; i < list.Count; i++)
		{
			EntityPlayer entityPlayer = list[i];
			if (entityPlayer.prefab == player.prefab)
			{
				list2.Add(entityPlayer.gameStage);
			}
		}
		return CalcPartyLevel(list2);
	}
}
