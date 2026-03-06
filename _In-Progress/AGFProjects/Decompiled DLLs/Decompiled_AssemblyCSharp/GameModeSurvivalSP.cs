using UnityEngine.Scripting;

[Preserve]
public class GameModeSurvivalSP : GameModeAbstract
{
	public static readonly string TypeName = typeof(GameModeSurvivalSP).Name;

	public override string GetName()
	{
		return "gmSurvivalSP";
	}

	public override string GetDescription()
	{
		return "gmSurvivalSPDesc";
	}

	public override int GetID()
	{
		return 6;
	}

	public override ModeGamePref[] GetSupportedGamePrefsInfo()
	{
		return new ModeGamePref[15]
		{
			new ModeGamePref(EnumGamePrefs.GameDifficulty, GamePrefs.EnumType.Int, 1),
			new ModeGamePref(EnumGamePrefs.DayNightLength, GamePrefs.EnumType.Int, 60),
			new ModeGamePref(EnumGamePrefs.DayLightLength, GamePrefs.EnumType.Int, 18),
			new ModeGamePref(EnumGamePrefs.LootAbundance, GamePrefs.EnumType.Int, 100),
			new ModeGamePref(EnumGamePrefs.LootRespawnDays, GamePrefs.EnumType.Int, 7),
			new ModeGamePref(EnumGamePrefs.DropOnDeath, GamePrefs.EnumType.Int, 0),
			new ModeGamePref(EnumGamePrefs.BloodMoonEnemyCount, GamePrefs.EnumType.Int, 8),
			new ModeGamePref(EnumGamePrefs.EnemySpawnMode, GamePrefs.EnumType.Bool, true),
			new ModeGamePref(EnumGamePrefs.EnemyDifficulty, GamePrefs.EnumType.Int, 0),
			new ModeGamePref(EnumGamePrefs.AirDropFrequency, GamePrefs.EnumType.Int, 72),
			new ModeGamePref(EnumGamePrefs.BuildCreate, GamePrefs.EnumType.Bool, false),
			new ModeGamePref(EnumGamePrefs.PersistentPlayerProfiles, GamePrefs.EnumType.Bool, false),
			new ModeGamePref(EnumGamePrefs.AirDropMarker, GamePrefs.EnumType.Bool, true),
			new ModeGamePref(EnumGamePrefs.BedrollDeadZoneSize, GamePrefs.EnumType.Int, 15),
			new ModeGamePref(EnumGamePrefs.MaxChunkAge, GamePrefs.EnumType.Int, -1)
		};
	}

	public override void Init()
	{
		base.Init();
		GameStats.Set(EnumGameStats.ShowSpawnWindow, _value: false);
		GameStats.Set(EnumGameStats.TimeLimitActive, _value: false);
		GameStats.Set(EnumGameStats.DayLimitActive, _value: false);
		GameStats.Set(EnumGameStats.ShowWindow, "");
		GameStats.Set(EnumGameStats.IsSpawnEnemies, GamePrefs.GetBool(EnumGamePrefs.EnemySpawnMode));
		GameStats.Set(EnumGameStats.ScorePlayerKillMultiplier, 0);
		GameStats.Set(EnumGameStats.ScoreZombieKillMultiplier, 1);
		GameStats.Set(EnumGameStats.ScoreDiedMultiplier, -5);
		GameStats.Set(EnumGameStats.IsSpawnNearOtherPlayer, _value: false);
		GameStats.Set(EnumGameStats.ZombieHordeMeter, _value: true);
		GameStats.Set(EnumGameStats.DropOnDeath, GamePrefs.GetInt(EnumGamePrefs.DropOnDeath));
		GameStats.Set(EnumGameStats.DropOnQuit, 0);
		GameStats.Set(EnumGameStats.BloodMoonEnemyCount, GamePrefs.GetInt(EnumGamePrefs.BloodMoonEnemyCount));
		GameStats.Set(EnumGameStats.EnemySpawnMode, GamePrefs.GetBool(EnumGamePrefs.EnemySpawnMode));
		GameStats.Set(EnumGameStats.EnemyDifficulty, GamePrefs.GetInt(EnumGamePrefs.EnemyDifficulty));
		GameStats.Set(EnumGameStats.AirDropFrequency, GamePrefs.GetInt(EnumGamePrefs.AirDropFrequency));
		GameStats.Set(EnumGameStats.AirDropMarker, GamePrefs.GetBool(EnumGamePrefs.AirDropMarker));
		GameStats.Set(EnumGameStats.PartySharedKillRange, GamePrefs.GetInt(EnumGamePrefs.PartySharedKillRange));
		GamePrefs.Set(EnumGamePrefs.ServerMaxPlayerCount, 1);
		GamePrefs.Set(EnumGamePrefs.ServerIsPublic, _value: false);
		GamePrefs.Set(EnumGamePrefs.ServerPort, Constants.cDefaultPort);
		GameStats.Set(EnumGameStats.IsFlyingEnabled, GamePrefs.GetBool(EnumGamePrefs.BuildCreate));
		GameStats.Set(EnumGameStats.BiomeProgression, GamePrefs.GetBool(EnumGamePrefs.BiomeProgression));
	}

	public override int GetRoundCount()
	{
		return 1;
	}

	public override void StartRound(int _idx)
	{
		GameStats.Set(EnumGameStats.GameState, 1);
	}

	public override void EndRound(int _idx)
	{
	}
}
