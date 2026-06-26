public class GameModeDeathmatch : GameModeAbstract
{
	public static readonly string TypeName = typeof(GameModeDeathmatch).Name;

	public override string GetName()
	{
		return "gmDeathmatch";
	}

	public override string GetDescription()
	{
		return "gmDeathmatchDesc";
	}

	public override int GetID()
	{
		return 3;
	}

	public override ModeGamePref[] GetSupportedGamePrefsInfo()
	{
		return new ModeGamePref[14]
		{
			new ModeGamePref(EnumGamePrefs.GameDifficulty, GamePrefs.EnumType.Int, 1),
			new ModeGamePref(EnumGamePrefs.MatchLength, GamePrefs.EnumType.Int, 10),
			new ModeGamePref(EnumGamePrefs.FragLimit, GamePrefs.EnumType.Int, 20),
			new ModeGamePref(EnumGamePrefs.DayNightLength, GamePrefs.EnumType.Int, 10),
			new ModeGamePref(EnumGamePrefs.DropOnDeath, GamePrefs.EnumType.Int, 1),
			new ModeGamePref(EnumGamePrefs.DropOnQuit, GamePrefs.EnumType.Int, 1),
			new ModeGamePref(EnumGamePrefs.BloodMoonEnemyCount, GamePrefs.EnumType.Int, 8),
			new ModeGamePref(EnumGamePrefs.EnemySpawnMode, GamePrefs.EnumType.Bool, true),
			new ModeGamePref(EnumGamePrefs.BuildCreate, GamePrefs.EnumType.Bool, false),
			new ModeGamePref(EnumGamePrefs.RebuildMap, GamePrefs.EnumType.Bool, true),
			new ModeGamePref(EnumGamePrefs.ServerIsPublic, GamePrefs.EnumType.Bool, true),
			new ModeGamePref(EnumGamePrefs.ServerMaxPlayerCount, GamePrefs.EnumType.Int, 4),
			new ModeGamePref(EnumGamePrefs.ServerPassword, GamePrefs.EnumType.String, ""),
			new ModeGamePref(EnumGamePrefs.ServerPort, GamePrefs.EnumType.Int, Constants.cDefaultPort)
		};
	}

	public override int GetRoundCount()
	{
		return 4;
	}

	public override void Init()
	{
		base.Init();
		GameStats.Set(EnumGameStats.ShowSpawnWindow, _value: true);
		GameStats.Set(EnumGameStats.TimeLimitActive, _value: false);
		GameStats.Set(EnumGameStats.ShowWindow, "");
		GameStats.Set(EnumGameStats.IsResetMapOnRestart, GamePrefs.GetBool(EnumGamePrefs.RebuildMap));
		GameStats.Set(EnumGameStats.IsSpawnNearOtherPlayer, _value: false);
		GameStats.Set(EnumGameStats.IsSpawnEnemies, GamePrefs.GetBool(EnumGamePrefs.EnemySpawnMode));
		GameStats.Set(EnumGameStats.PlayerKillingMode, 3);
		GameStats.Set(EnumGameStats.ScorePlayerKillMultiplier, 3);
		GameStats.Set(EnumGameStats.ScoreZombieKillMultiplier, 1);
		GameStats.Set(EnumGameStats.ScoreDiedMultiplier, -5);
		GamePrefs.Set(EnumGamePrefs.DynamicSpawner, "");
		GameStats.Set(EnumGameStats.DropOnDeath, GamePrefs.GetInt(EnumGamePrefs.DropOnDeath));
		GameStats.Set(EnumGameStats.DropOnQuit, GamePrefs.GetInt(EnumGamePrefs.DropOnQuit));
		GameStats.Set(EnumGameStats.BloodMoonEnemyCount, GamePrefs.GetInt(EnumGamePrefs.BloodMoonEnemyCount));
		GameStats.Set(EnumGameStats.EnemySpawnMode, GamePrefs.GetBool(EnumGamePrefs.EnemySpawnMode));
	}

	public override void StartRound(int _idx)
	{
		switch (_idx)
		{
		case 0:
			GameStats.Set(EnumGameStats.TimeLimitThisRound, GamePrefs.GetInt(EnumGamePrefs.MatchLength) * 60);
			GameStats.Set(EnumGameStats.TimeLimitActive, GamePrefs.GetInt(EnumGamePrefs.MatchLength) > 0);
			GameStats.Set(EnumGameStats.FragLimitThisRound, GamePrefs.GetInt(EnumGamePrefs.FragLimit));
			GameStats.Set(EnumGameStats.FragLimitActive, GamePrefs.GetInt(EnumGamePrefs.FragLimit) > 0);
			GameStats.Set(EnumGameStats.DayLimitActive, _value: false);
			GameStats.Set(EnumGameStats.GameState, 1);
			break;
		case 1:
			GameStats.Set(EnumGameStats.TimeLimitThisRound, 10);
			GameStats.Set(EnumGameStats.TimeLimitActive, _value: true);
			GameStats.Set(EnumGameStats.FragLimitActive, _value: false);
			GameStats.Set(EnumGameStats.ShowWindow, null);
			GameStats.Set(EnumGameStats.GameState, 2);
			break;
		case 2:
			GameStats.Set(EnumGameStats.TimeLimitThisRound, 2);
			GameStats.Set(EnumGameStats.ShowWindow, XUiC_LoadingScreen.ID);
			break;
		case 3:
			GameStats.Set(EnumGameStats.TimeLimitActive, _value: false);
			GameStats.Set(EnumGameStats.LoadScene, "SceneGame");
			break;
		}
	}

	public override void EndRound(int _idx)
	{
	}
}
