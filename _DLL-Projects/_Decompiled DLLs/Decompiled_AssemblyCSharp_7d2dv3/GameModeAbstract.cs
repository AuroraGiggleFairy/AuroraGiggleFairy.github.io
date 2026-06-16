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
		GameStats.Set(EnumGameStats.BlockDamagePlayer, GamePrefs.GetInt(EnumGamePrefs.BlockDamagePlayer));
		GameStats.Set(EnumGameStats.XPMultiplier, GamePrefs.GetInt(EnumGamePrefs.XPMultiplier));
		GameStats.Set(EnumGameStats.BloodMoonWarning, GamePrefs.GetInt(EnumGamePrefs.BloodMoonWarning));
		GameStats.Set(EnumGameStats.DayLightLength, GamePrefs.GetInt(EnumGamePrefs.DayLightLength));
		GameStats.Set(EnumGameStats.DayNightLength, GamePrefs.GetInt(EnumGamePrefs.DayNightLength));
		GameStats.Set(EnumGameStats.BlockDamageAI, GamePrefs.GetInt(EnumGamePrefs.BlockDamageAI));
		GameStats.Set(EnumGameStats.BlockDamageAIBM, GamePrefs.GetInt(EnumGamePrefs.BlockDamageAIBM));
		GameStats.Set(EnumGameStats.LootAbundance, GamePrefs.GetInt(EnumGamePrefs.LootAbundance));
		GameStats.Set(EnumGameStats.LootRespawnDays, GamePrefs.GetInt(EnumGamePrefs.LootRespawnDays));
		GameStats.Set(EnumGameStats.GlobalGSModifier, 100);
		GameStats.Set(EnumGameStats.BiomeGSModifier, 100);
		GameStats.Set(EnumGameStats.GlobalLSModifier, 100);
		GameStats.Set(EnumGameStats.BiomeLSModifier, 100);
		GameStats.Set(EnumGameStats.AirDropFrequency, GamePrefs.GetInt(EnumGamePrefs.AirDropFrequency));
		GameStats.Set(EnumGameStats.AirDropMarker, GamePrefs.GetBool(EnumGamePrefs.AirDropMarker));
		GameStats.Set(EnumGameStats.DeathPenalty, GamePrefs.GetInt(EnumGamePrefs.DeathPenalty));
		GameStats.Set(EnumGameStats.DropOnDeath, GamePrefs.GetInt(EnumGamePrefs.DropOnDeath));
		GameStats.Set(EnumGameStats.DropOnQuit, GamePrefs.GetInt(EnumGamePrefs.DropOnQuit));
		GameStats.Set(EnumGameStats.BloodMoonEnemyCount, GamePrefs.GetInt(EnumGamePrefs.BloodMoonEnemyCount));
		GameStats.Set(EnumGameStats.EnemySpawnMode, GamePrefs.GetBool(EnumGamePrefs.EnemySpawnMode));
		GameStats.Set(EnumGameStats.EnemyDifficulty, GamePrefs.GetInt(EnumGamePrefs.EnemyDifficulty));
		GameStats.Set(EnumGameStats.LandClaimCount, GamePrefs.GetInt(EnumGamePrefs.LandClaimCount));
		GameStats.Set(EnumGameStats.LandClaimSize, GamePrefs.GetInt(EnumGamePrefs.LandClaimSize));
		GameStats.Set(EnumGameStats.LandClaimDeadZone, GamePrefs.GetInt(EnumGamePrefs.LandClaimDeadZone));
		GameStats.Set(EnumGameStats.LandClaimExpiryTime, GamePrefs.GetInt(EnumGamePrefs.LandClaimExpiryTime));
		GameStats.Set(EnumGameStats.LandClaimDecayMode, GamePrefs.GetInt(EnumGamePrefs.LandClaimDecayMode));
		GameStats.Set(EnumGameStats.LandClaimOnlineDurabilityModifier, GamePrefs.GetInt(EnumGamePrefs.LandClaimOnlineDurabilityModifier));
		GameStats.Set(EnumGameStats.LandClaimOfflineDurabilityModifier, GamePrefs.GetInt(EnumGamePrefs.LandClaimOfflineDurabilityModifier));
		GameStats.Set(EnumGameStats.LandClaimOfflineDelay, GamePrefs.GetInt(EnumGamePrefs.LandClaimOfflineDelay));
		GameStats.Set(EnumGameStats.BedrollExpiryTime, GamePrefs.GetInt(EnumGamePrefs.BedrollExpiryTime));
		GameStats.Set(EnumGameStats.PartySharedKillRange, GamePrefs.GetInt(EnumGamePrefs.PartySharedKillRange));
		GameStats.Set(EnumGameStats.BiomeProgression, GamePrefs.GetBool(EnumGamePrefs.BiomeProgression));
		GameStats.Set(EnumGameStats.CameraRestrictionMode, GamePrefs.GetInt(EnumGamePrefs.CameraRestrictionMode));
		GameStats.Set(EnumGameStats.SandboxCode, GamePrefs.GetString(EnumGamePrefs.SandboxCode));
		GameStats.Set(EnumGameStats.OptionsPOICulling, GamePrefs.GetInt(EnumGamePrefs.OptionsPOICulling));
		GameStats.Set(EnumGameStats.AllowedViewDistance, GamePrefs.GetInt(EnumGamePrefs.OptionsGfxViewDistance));
		GameStats.Set(EnumGameStats.QuestProgressionDailyLimit, GamePrefs.GetInt(EnumGamePrefs.QuestProgressionDailyLimit));
		GameStats.Set(EnumGameStats.StormFreq, GamePrefs.GetInt(EnumGamePrefs.StormFreq));
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
