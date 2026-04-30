namespace Platform;

public static class AchievementUtils
{
	public static bool IsCreativeModeActive()
	{
		if (!GamePrefs.GetString(EnumGamePrefs.GameMode).Equals(GameModeCreative.TypeName) && !GameStats.GetBool(EnumGameStats.IsCreativeMenuEnabled) && !GamePrefs.GetBool(EnumGamePrefs.CreativeMenuEnabled))
		{
			return GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled);
		}
		return true;
	}
}
