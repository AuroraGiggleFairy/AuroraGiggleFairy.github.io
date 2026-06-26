using System;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager
{
	[PublicizedFrom(EAccessModifier.Private)]
	public float timeRoundStarted;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameMode currentGameMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameManager gameManager;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bServer;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bGameStarted;

	[PublicizedFrom(EAccessModifier.Private)]
	public int fragLimitCounter;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lastGameModeID = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lastGameModeString = string.Empty;

	public GameStateManager(GameManager _gameManager)
	{
		bGameStarted = false;
		gameManager = _gameManager;
	}

	public void InitGame(bool _bServer)
	{
		bServer = _bServer;
		GameStats.Set(EnumGameStats.GameState, 1);
		Type type = Type.GetType(GamePrefs.GetString(EnumGamePrefs.GameMode));
		if (type == null)
		{
			type = Type.GetType((string)GamePrefs.GetDefault(EnumGamePrefs.GameMode));
		}
		currentGameMode = (GameMode)Activator.CreateInstance(type);
		GameStats.Set(EnumGameStats.GameModeId, currentGameMode.GetID());
		if (bServer)
		{
			GameStats.Set(EnumGameStats.CurrentRoundIx, 0);
			timeRoundStarted = Time.time;
			currentGameMode.Init();
			currentGameMode.StartRound(GameStats.GetInt(EnumGameStats.CurrentRoundIx));
			bDirty = true;
		}
	}

	public void StartGame()
	{
		bGameStarted = true;
		BacktraceUtils.StartStatisticsUpdate();
	}

	public bool IsGameStarted()
	{
		return bGameStarted;
	}

	public void EndGame()
	{
		GameStats.Set(EnumGameStats.GameState, 0);
		bDirty = true;
		bGameStarted = false;
		bServer = false;
	}

	public void SetBloodMoonDay(int day)
	{
		int num = GameStats.GetInt(EnumGameStats.BloodMoonDay);
		if (day != num)
		{
			GameStats.Set(EnumGameStats.BloodMoonDay, day);
			bDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int nextRound()
	{
		int num = GameStats.GetInt(EnumGameStats.CurrentRoundIx);
		currentGameMode.EndRound(num);
		if (++num >= currentGameMode.GetRoundCount())
		{
			num = 0;
		}
		GameStats.Set(EnumGameStats.CurrentRoundIx, num);
		bDirty = true;
		timeRoundStarted = Time.time;
		return num;
	}

	public bool OnUpdateTick()
	{
		if (bServer)
		{
			if (GameStats.GetBool(EnumGameStats.TimeLimitActive))
			{
				int num = GameStats.GetInt(EnumGameStats.TimeLimitThisRound);
				if (num >= 0 && Time.time - timeRoundStarted >= 1f)
				{
					int num2 = (int)(Time.time - timeRoundStarted);
					timeRoundStarted = Time.time;
					num -= num2;
					GameStats.Set(EnumGameStats.TimeLimitThisRound, num);
					if (num < 0)
					{
						num = 0;
						currentGameMode.StartRound(nextRound());
					}
					bDirty = true;
				}
			}
			if (GameStats.GetBool(EnumGameStats.DayLimitActive) && GameUtils.WorldTimeToDays(gameManager.World.worldTime) > GameStats.GetInt(EnumGameStats.DayLimitThisRound))
			{
				currentGameMode.StartRound(nextRound());
			}
			if (GameStats.GetBool(EnumGameStats.FragLimitActive) && ++fragLimitCounter > 40)
			{
				fragLimitCounter = 0;
				int num3 = GameStats.GetInt(EnumGameStats.FragLimitThisRound);
				for (int i = 0; i < gameManager.World.Players.list.Count; i++)
				{
					if (gameManager.World.Players.list[i].KilledPlayers >= num3)
					{
						currentGameMode.StartRound(nextRound());
						break;
					}
				}
			}
			if (GameTimer.Instance.ticks % 20 == 0L)
			{
				int num4 = 0;
				int num5 = 0;
				List<Entity> list = gameManager.World.Entities.list;
				for (int num6 = list.Count - 1; num6 >= 0; num6--)
				{
					Entity entity = list[num6];
					if (!entity.IsDead())
					{
						EntityClass entityClass = EntityClass.list[entity.entityClass];
						if (entityClass.bIsEnemyEntity)
						{
							num4++;
						}
						else if (entityClass.bIsAnimalEntity)
						{
							num5++;
						}
					}
				}
				GameStats.Set(EnumGameStats.EnemyCount, num4);
				GameStats.Set(EnumGameStats.AnimalCount, num5);
			}
			if (bDirty)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageGameStats>().Setup(GameStats.Instance), _onlyClientsAttachedToAnEntity: true);
				bDirty = false;
			}
		}
		return true;
	}

	public string GetModeName()
	{
		if (currentGameMode == null)
		{
			return string.Empty;
		}
		if (currentGameMode.GetID() != lastGameModeID)
		{
			lastGameModeString = Localization.Get(currentGameMode.GetName());
			lastGameModeID = currentGameMode.GetID();
		}
		return lastGameModeString;
	}

	public GameMode GetGameMode()
	{
		return currentGameMode;
	}
}
