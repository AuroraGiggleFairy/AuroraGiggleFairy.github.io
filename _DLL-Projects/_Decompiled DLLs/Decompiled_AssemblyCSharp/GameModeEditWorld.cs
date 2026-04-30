using UnityEngine.Scripting;

[Preserve]
public class GameModeEditWorld : GameModeAbstract
{
	public static readonly string TypeName = typeof(GameModeEditWorld).Name;

	public override string GetName()
	{
		return "gmEditWorld";
	}

	public override string GetDescription()
	{
		return "gmEditWorldDesc";
	}

	public override int GetID()
	{
		return 8;
	}

	public override ModeGamePref[] GetSupportedGamePrefsInfo()
	{
		return new ModeGamePref[4]
		{
			new ModeGamePref(EnumGamePrefs.GameDifficulty, GamePrefs.EnumType.Int, 1),
			new ModeGamePref(EnumGamePrefs.ServerIsPublic, GamePrefs.EnumType.Bool, false),
			new ModeGamePref(EnumGamePrefs.ServerVisibility, GamePrefs.EnumType.Int, 1),
			new ModeGamePref(EnumGamePrefs.ServerMaxPlayerCount, GamePrefs.EnumType.Int, 8)
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
		GameStats.Set(EnumGameStats.TimeOfDayIncPerSec, 0);
		GameStats.Set(EnumGameStats.IsSpawnEnemies, _value: false);
		GameStats.Set(EnumGameStats.IsTeleportEnabled, _value: true);
		GameStats.Set(EnumGameStats.IsFlyingEnabled, _value: true);
		GameStats.Set(EnumGameStats.IsCreativeMenuEnabled, _value: true);
		GameStats.Set(EnumGameStats.IsPlayerDamageEnabled, _value: false);
		GameStats.Set(EnumGameStats.AirDropFrequency, 0);
		GameStats.Set(EnumGameStats.AutoParty, _value: true);
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
