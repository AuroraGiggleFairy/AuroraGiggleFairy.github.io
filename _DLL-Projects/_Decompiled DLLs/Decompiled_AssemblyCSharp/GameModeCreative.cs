using UnityEngine.Scripting;

[Preserve]
public class GameModeCreative : GameModeAbstract
{
	public static readonly string TypeName = typeof(GameModeCreative).Name;

	public override string GetName()
	{
		return "gmCreative";
	}

	public override string GetDescription()
	{
		return "gmCreativeDesc";
	}

	public override int GetID()
	{
		return 2;
	}

	public override ModeGamePref[] GetSupportedGamePrefsInfo()
	{
		return new ModeGamePref[3]
		{
			new ModeGamePref(EnumGamePrefs.ServerIsPublic, GamePrefs.EnumType.Bool, false),
			new ModeGamePref(EnumGamePrefs.DayNightLength, GamePrefs.EnumType.Int, 60),
			new ModeGamePref(EnumGamePrefs.DayLightLength, GamePrefs.EnumType.Int, 18)
		};
	}

	public override void Init()
	{
		base.Init();
		GameStats.Set(EnumGameStats.ShowSpawnWindow, _value: false);
		GameStats.Set(EnumGameStats.TimeLimitActive, _value: false);
		GameStats.Set(EnumGameStats.DayLimitActive, _value: false);
		GameStats.Set(EnumGameStats.IsSpawnNearOtherPlayer, _value: true);
		GameStats.Set(EnumGameStats.ShowWindow, "");
		GameStats.Set(EnumGameStats.TimeOfDayIncPerSec, 4);
		GameStats.Set(EnumGameStats.IsSpawnEnemies, _value: false);
		GameStats.Set(EnumGameStats.ShowAllPlayersOnMap, _value: true);
		GameStats.Set(EnumGameStats.ZombieHordeMeter, _value: false);
		GameStats.Set(EnumGameStats.DeathPenalty, 0);
		GameStats.Set(EnumGameStats.DropOnDeath, 0);
		GameStats.Set(EnumGameStats.DropOnQuit, 0);
		GameStats.Set(EnumGameStats.IsTeleportEnabled, _value: true);
		GameStats.Set(EnumGameStats.IsFlyingEnabled, _value: true);
		GameStats.Set(EnumGameStats.IsCreativeMenuEnabled, _value: true);
		GameStats.Set(EnumGameStats.IsPlayerDamageEnabled, _value: false);
		GameStats.Set(EnumGameStats.AutoParty, _value: false);
		GamePrefs.Set(EnumGamePrefs.DynamicSpawner, "");
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

	public override string GetAdditionalGameInfo(World _world)
	{
		return GamePrefs.GetInt(EnumGamePrefs.SelectionOperationMode) switch
		{
			0 => "Player move", 
			1 => "Selection move " + ((GamePrefs.GetInt(EnumGamePrefs.SelectionContextMode) == 0) ? "absolute" : "relative"), 
			2 => "Selection size", 
			_ => "", 
		};
	}
}
