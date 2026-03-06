using System.Text;
using UnityEngine.Scripting;

[Preserve]
public class GameModeZombieHorde : GameModeAbstract
{
	public static readonly string TypeName = typeof(GameModeZombieHorde).Name;

	public override string GetName()
	{
		return "gmZombieHorde";
	}

	public override string GetDescription()
	{
		return "gmZombieHordeDesc";
	}

	public override int GetID()
	{
		return 4;
	}

	public override ModeGamePref[] GetSupportedGamePrefsInfo()
	{
		return new ModeGamePref[18]
		{
			new ModeGamePref(EnumGamePrefs.GameDifficulty, GamePrefs.EnumType.Int, 1),
			new ModeGamePref(EnumGamePrefs.DayCount, GamePrefs.EnumType.Int, 9),
			new ModeGamePref(EnumGamePrefs.DayNightLength, GamePrefs.EnumType.Int, 8),
			new ModeGamePref(EnumGamePrefs.PlayerKillingMode, GamePrefs.EnumType.Int, EnumPlayerKillingMode.KillStrangersOnly),
			new ModeGamePref(EnumGamePrefs.ShowFriendPlayerOnMap, GamePrefs.EnumType.Bool, false),
			new ModeGamePref(EnumGamePrefs.LootAbundance, GamePrefs.EnumType.Int, 100),
			new ModeGamePref(EnumGamePrefs.LootRespawnDays, GamePrefs.EnumType.Int, 7),
			new ModeGamePref(EnumGamePrefs.DropOnDeath, GamePrefs.EnumType.Int, 1),
			new ModeGamePref(EnumGamePrefs.DropOnQuit, GamePrefs.EnumType.Int, 1),
			new ModeGamePref(EnumGamePrefs.BloodMoonEnemyCount, GamePrefs.EnumType.Int, 8),
			new ModeGamePref(EnumGamePrefs.EnemySpawnMode, GamePrefs.EnumType.Bool, true),
			new ModeGamePref(EnumGamePrefs.BuildCreate, GamePrefs.EnumType.Bool, false),
			new ModeGamePref(EnumGamePrefs.RebuildMap, GamePrefs.EnumType.Bool, true),
			new ModeGamePref(EnumGamePrefs.ServerIsPublic, GamePrefs.EnumType.Bool, false),
			new ModeGamePref(EnumGamePrefs.ServerMaxPlayerCount, GamePrefs.EnumType.Int, 4),
			new ModeGamePref(EnumGamePrefs.ServerPassword, GamePrefs.EnumType.String, ""),
			new ModeGamePref(EnumGamePrefs.ServerPort, GamePrefs.EnumType.Int, Constants.cDefaultPort),
			new ModeGamePref(EnumGamePrefs.MaxChunkAge, GamePrefs.EnumType.Int, -1)
		};
	}

	public override int GetRoundCount()
	{
		return 4;
	}

	public override void Init()
	{
		base.Init();
		GameStats.Set(EnumGameStats.TimeLimitActive, _value: false);
		GameStats.Set(EnumGameStats.ShowWindow, "");
		GameStats.Set(EnumGameStats.ShowSpawnWindow, _value: true);
		GameStats.Set(EnumGameStats.IsSpawnNearOtherPlayer, _value: false);
		GameStats.Set(EnumGameStats.IsSpawnEnemies, GamePrefs.GetInt(EnumGamePrefs.EnemySpawnMode) > 0);
		GameStats.Set(EnumGameStats.IsResetMapOnRestart, GamePrefs.GetBool(EnumGamePrefs.RebuildMap));
		GameStats.Set(EnumGameStats.ScorePlayerKillMultiplier, 1);
		GameStats.Set(EnumGameStats.ScoreZombieKillMultiplier, 3);
		GameStats.Set(EnumGameStats.ScoreDiedMultiplier, -5);
		GamePrefs.Set(EnumGamePrefs.DynamicSpawner, "SpawnHordeMode");
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
			GameStats.Set(EnumGameStats.DayLimitThisRound, GamePrefs.GetInt(EnumGamePrefs.DayCount));
			GameStats.Set(EnumGameStats.DayLimitActive, GamePrefs.GetInt(EnumGamePrefs.DayCount) > 0);
			GameStats.Set(EnumGameStats.TimeLimitActive, _value: false);
			GameStats.Set(EnumGameStats.FragLimitActive, _value: false);
			GameStats.Set(EnumGameStats.GameState, 1);
			break;
		case 1:
			GameStats.Set(EnumGameStats.TimeLimitThisRound, 10);
			GameStats.Set(EnumGameStats.TimeLimitActive, _value: true);
			GameStats.Set(EnumGameStats.DayLimitActive, _value: false);
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

	public override string GetAdditionalGameInfo(World _world)
	{
		if (GameStats.GetBool(EnumGameStats.TimeLimitActive) || GameStats.GetBool(EnumGameStats.DayLimitActive) || GameStats.GetBool(EnumGameStats.FragLimitActive))
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (GameStats.GetBool(EnumGameStats.FragLimitActive))
			{
				stringBuilder.AppendFormat("  Frag limit: {0}", GameStats.GetInt(EnumGameStats.FragLimitThisRound));
			}
			if (GameStats.GetBool(EnumGameStats.TimeLimitActive))
			{
				int num = GameStats.GetInt(EnumGameStats.TimeLimitThisRound);
				stringBuilder.AppendFormat("  Time limit: {0:D2}:{1:D2}", num / 60, num % 60);
			}
			if (GameStats.GetBool(EnumGameStats.DayLimitActive))
			{
				int num2 = GameStats.GetInt(EnumGameStats.DayLimitThisRound);
				(int Days, int Hours, int Minutes) tuple = GameUtils.WorldTimeToElements(_world.worldTime);
				int item = tuple.Days;
				int item2 = tuple.Hours;
				int item3 = tuple.Minutes;
				int num3 = num2 - item;
				int num4 = ((num3 >= 0) ? (24 - item2) : 0);
				int num5 = ((num3 >= 0) ? (60 - item3) : 0);
				stringBuilder.AppendFormat("  Days to play: {0}:{1:D2}:{2:D2}", (num3 >= 0) ? num3.ToString("D2") : "0", num4, num5);
			}
			return stringBuilder.ToString();
		}
		return string.Empty;
	}
}
