using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class GameModeSurvival : GameModeAbstract
{
	public static readonly string TypeName = typeof(GameModeSurvival).Name;

	public static int OverrideMaxPlayerCount = -1;

	public override string GetName()
	{
		return "gmSurvival";
	}

	public override string GetDescription()
	{
		return "gmSurvivalDesc";
	}

	public override int GetID()
	{
		return 1;
	}

	public override ModeGamePref[] GetSupportedGamePrefsInfo()
	{
		return new ModeGamePref[57]
		{
			new ModeGamePref(EnumGamePrefs.GameDifficulty, GamePrefs.EnumType.Int, 1),
			new ModeGamePref(EnumGamePrefs.DayNightLength, GamePrefs.EnumType.Int, 60),
			new ModeGamePref(EnumGamePrefs.DayLightLength, GamePrefs.EnumType.Int, 18),
			new ModeGamePref(EnumGamePrefs.BloodMoonFrequency, GamePrefs.EnumType.Int, 7),
			new ModeGamePref(EnumGamePrefs.BloodMoonRange, GamePrefs.EnumType.Int, 0),
			new ModeGamePref(EnumGamePrefs.BloodMoonWarning, GamePrefs.EnumType.Int, 8),
			new ModeGamePref(EnumGamePrefs.ZombieFeralSense, GamePrefs.EnumType.Int, 0),
			new ModeGamePref(EnumGamePrefs.ZombieMove, GamePrefs.EnumType.Int, 0),
			new ModeGamePref(EnumGamePrefs.ZombieMoveNight, GamePrefs.EnumType.Int, 3),
			new ModeGamePref(EnumGamePrefs.ZombieFeralMove, GamePrefs.EnumType.Int, 3),
			new ModeGamePref(EnumGamePrefs.ZombieBMMove, GamePrefs.EnumType.Int, 3),
			new ModeGamePref(EnumGamePrefs.MaxSpawnedZombies, GamePrefs.EnumType.Int, 64),
			new ModeGamePref(EnumGamePrefs.MaxSpawnedAnimals, GamePrefs.EnumType.Int, 50),
			new ModeGamePref(EnumGamePrefs.PersistentPlayerProfiles, GamePrefs.EnumType.Bool, false),
			new ModeGamePref(EnumGamePrefs.XPMultiplier, GamePrefs.EnumType.Int, 100),
			new ModeGamePref(EnumGamePrefs.BlockDamagePlayer, GamePrefs.EnumType.Int, 100),
			new ModeGamePref(EnumGamePrefs.BlockDamageAI, GamePrefs.EnumType.Int, 100),
			new ModeGamePref(EnumGamePrefs.BlockDamageAIBM, GamePrefs.EnumType.Int, 100),
			new ModeGamePref(EnumGamePrefs.LootAbundance, GamePrefs.EnumType.Int, 100),
			new ModeGamePref(EnumGamePrefs.LootRespawnDays, GamePrefs.EnumType.Int, 7),
			new ModeGamePref(EnumGamePrefs.DeathPenalty, GamePrefs.EnumType.Int, 1),
			new ModeGamePref(EnumGamePrefs.DropOnDeath, GamePrefs.EnumType.Int, 1),
			new ModeGamePref(EnumGamePrefs.DropOnQuit, GamePrefs.EnumType.Int, 0),
			new ModeGamePref(EnumGamePrefs.BloodMoonEnemyCount, GamePrefs.EnumType.Int, 8),
			new ModeGamePref(EnumGamePrefs.EnemySpawnMode, GamePrefs.EnumType.Bool, true),
			new ModeGamePref(EnumGamePrefs.EnemyDifficulty, GamePrefs.EnumType.Int, 0),
			new ModeGamePref(EnumGamePrefs.ServerEnabled, GamePrefs.EnumType.Bool, false),
			new ModeGamePref(EnumGamePrefs.ServerIsPublic, GamePrefs.EnumType.Bool, true),
			new ModeGamePref(EnumGamePrefs.ServerVisibility, GamePrefs.EnumType.Int, 2),
			new ModeGamePref(EnumGamePrefs.ServerMaxPlayerCount, GamePrefs.EnumType.Int, 8, new Dictionary<DeviceFlag, object>
			{
				{
					DeviceFlag.PS5,
					(OverrideMaxPlayerCount == -1) ? 4 : OverrideMaxPlayerCount
				},
				{
					DeviceFlag.XBoxSeriesX,
					(OverrideMaxPlayerCount == -1) ? 4 : OverrideMaxPlayerCount
				},
				{
					DeviceFlag.XBoxSeriesS,
					(OverrideMaxPlayerCount == -1) ? 2 : OverrideMaxPlayerCount
				}
			}),
			new ModeGamePref(EnumGamePrefs.ServerPassword, GamePrefs.EnumType.String, ""),
			new ModeGamePref(EnumGamePrefs.ServerPort, GamePrefs.EnumType.Int, Constants.cDefaultPort),
			new ModeGamePref(EnumGamePrefs.ServerAllowCrossplay, GamePrefs.EnumType.Bool, true),
			new ModeGamePref(EnumGamePrefs.ServerEACPeerToPeer, GamePrefs.EnumType.Bool, true),
			new ModeGamePref(EnumGamePrefs.PlayerKillingMode, GamePrefs.EnumType.Int, EnumPlayerKillingMode.KillStrangersOnly),
			new ModeGamePref(EnumGamePrefs.LandClaimCount, GamePrefs.EnumType.Int, 5),
			new ModeGamePref(EnumGamePrefs.LandClaimSize, GamePrefs.EnumType.Int, 41),
			new ModeGamePref(EnumGamePrefs.LandClaimDeadZone, GamePrefs.EnumType.Int, 30),
			new ModeGamePref(EnumGamePrefs.LandClaimExpiryTime, GamePrefs.EnumType.Int, 7),
			new ModeGamePref(EnumGamePrefs.LandClaimDecayMode, GamePrefs.EnumType.Int, 0),
			new ModeGamePref(EnumGamePrefs.LandClaimOnlineDurabilityModifier, GamePrefs.EnumType.Int, 4),
			new ModeGamePref(EnumGamePrefs.LandClaimOfflineDurabilityModifier, GamePrefs.EnumType.Int, 4),
			new ModeGamePref(EnumGamePrefs.LandClaimOfflineDelay, GamePrefs.EnumType.Int, 0),
			new ModeGamePref(EnumGamePrefs.BedrollDeadZoneSize, GamePrefs.EnumType.Int, 15),
			new ModeGamePref(EnumGamePrefs.BedrollExpiryTime, GamePrefs.EnumType.Int, 45),
			new ModeGamePref(EnumGamePrefs.AirDropFrequency, GamePrefs.EnumType.Int, 72),
			new ModeGamePref(EnumGamePrefs.BuildCreate, GamePrefs.EnumType.Bool, false),
			new ModeGamePref(EnumGamePrefs.AirDropMarker, GamePrefs.EnumType.Bool, true),
			new ModeGamePref(EnumGamePrefs.PartySharedKillRange, GamePrefs.EnumType.Int, 100),
			new ModeGamePref(EnumGamePrefs.ShowFriendPlayerOnMap, GamePrefs.EnumType.Bool, true),
			new ModeGamePref(EnumGamePrefs.MaxChunkAge, GamePrefs.EnumType.Int, -1),
			new ModeGamePref(EnumGamePrefs.QuestProgressionDailyLimit, GamePrefs.EnumType.Int, 4),
			new ModeGamePref(EnumGamePrefs.BiomeProgression, GamePrefs.EnumType.Bool, true),
			new ModeGamePref(EnumGamePrefs.StormFreq, GamePrefs.EnumType.Int, 100),
			new ModeGamePref(EnumGamePrefs.CameraRestrictionMode, GamePrefs.EnumType.Int, 0),
			new ModeGamePref(EnumGamePrefs.AISmellMode, GamePrefs.EnumType.Int, 3),
			new ModeGamePref(EnumGamePrefs.JarRefund, GamePrefs.EnumType.Int, 0)
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
		GameStats.Set(EnumGameStats.AirDropFrequency, GamePrefs.GetInt(EnumGamePrefs.AirDropFrequency));
		GameStats.Set(EnumGameStats.AirDropMarker, GamePrefs.GetBool(EnumGamePrefs.AirDropMarker));
		GameStats.Set(EnumGameStats.PartySharedKillRange, GamePrefs.GetInt(EnumGamePrefs.PartySharedKillRange));
		GameStats.Set(EnumGameStats.IsFlyingEnabled, GamePrefs.GetBool(EnumGamePrefs.BuildCreate));
		GameStats.Set(EnumGameStats.AutoParty, _value: false);
		GameStats.Set(EnumGameStats.BiomeProgression, GamePrefs.GetBool(EnumGamePrefs.BiomeProgression));
		GameStats.Set(EnumGameStats.CameraRestrictionMode, GamePrefs.GetInt(EnumGamePrefs.CameraRestrictionMode));
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
