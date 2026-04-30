public abstract class GameModeAbstract : GameMode
{
	public override void Init()
	{
		GameStats.Set(EnumGameStats.IsSpawnEnemies, GamePrefs.GetBool(EnumGamePrefs.EnemySpawnMode));
		GameStats.Set(EnumGameStats.PlayerKillingMode, GamePrefs.GetInt(EnumGamePrefs.PlayerKillingMode));
		GameStats.Set(EnumGameStats.ShowAllPlayersOnMap, _value: false);
		GameStats.Set(EnumGameStats.ShowFriendPlayerOnMap, GamePrefs.GetBool(EnumGamePrefs.ShowFriendPlayerOnMap));
		GameStats.Set(EnumGameStats.IsResetMapOnRestart, _value: false);
		GameStats.Set(EnumGameStats.IsFlyingEnabled, GamePrefs.GetBool(EnumGamePrefs.BuildCreate));
		GameStats.Set(EnumGameStats.IsCreativeMenuEnabled, GamePrefs.GetBool(EnumGamePrefs.BuildCreate));
		GameStats.Set(EnumGameStats.IsTeleportEnabled, _value: false);
		GameStats.Set(EnumGameStats.IsPlayerDamageEnabled, _value: true);
		GameStats.Set(EnumGameStats.IsPlayerCollisionEnabled, _value: true);
		GameStats.Set(EnumGameStats.TimeOfDayIncPerSec, 24000 / (GamePrefs.GetInt(EnumGamePrefs.DayNightLength) * 60));
		GameStats.Set(EnumGameStats.DayLimitActive, _value: false);
		GameStats.Set(EnumGameStats.TimeLimitActive, _value: false);
		GameStats.Set(EnumGameStats.FragLimitActive, _value: false);
		GameStats.Set(EnumGameStats.GameDifficulty, GamePrefs.GetInt(EnumGamePrefs.GameDifficulty));
		GameStats.Set(EnumGameStats.GameDifficultyBonus, GameStageDefinition.DifficultyBonus);
		GameStats.Set(EnumGameStats.BlockDamagePlayer, GamePrefs.GetInt(EnumGamePrefs.BlockDamagePlayer));
		GameStats.Set(EnumGameStats.XPMultiplier, GamePrefs.GetInt(EnumGamePrefs.XPMultiplier));
		GameStats.Set(EnumGameStats.BloodMoonWarning, GamePrefs.GetInt(EnumGamePrefs.BloodMoonWarning));
		GameStats.Set(EnumGameStats.DayLightLength, GamePrefs.GetInt(EnumGamePrefs.DayLightLength));
		GameStats.Set(EnumGameStats.OptionsPOICulling, GamePrefs.GetInt(EnumGamePrefs.OptionsPOICulling));
		GameStats.Set(EnumGameStats.AllowedViewDistance, GamePrefs.GetInt(EnumGamePrefs.OptionsGfxViewDistance));
		GameStats.Set(EnumGameStats.QuestProgressionDailyLimit, GamePrefs.GetInt(EnumGamePrefs.QuestProgressionDailyLimit));
		GameStats.Set(EnumGameStats.StormFreq, GamePrefs.GetInt(EnumGamePrefs.StormFreq));
		GameStats.Set(EnumGameStats.JarRefund, GamePrefs.GetInt(EnumGamePrefs.JarRefund));
	}

	public override void ResetGamePrefs()
	{
		ModeGamePref[] supportedGamePrefsInfo = GetSupportedGamePrefsInfo();
		for (int i = 0; i < supportedGamePrefsInfo.GetLength(0); i++)
		{
			EnumGamePrefs gamePref = supportedGamePrefsInfo[i].GamePref;
			switch (supportedGamePrefsInfo[i].ValueType)
			{
			case GamePrefs.EnumType.Int:
				GamePrefs.Set(gamePref, (int)supportedGamePrefsInfo[i].DefaultValue);
				break;
			case GamePrefs.EnumType.String:
				GamePrefs.Set(gamePref, (string)supportedGamePrefsInfo[i].DefaultValue);
				break;
			case GamePrefs.EnumType.Bool:
				GamePrefs.Set(gamePref, (bool)supportedGamePrefsInfo[i].DefaultValue);
				break;
			}
		}
		Init();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public GameModeAbstract()
	{
	}
}
