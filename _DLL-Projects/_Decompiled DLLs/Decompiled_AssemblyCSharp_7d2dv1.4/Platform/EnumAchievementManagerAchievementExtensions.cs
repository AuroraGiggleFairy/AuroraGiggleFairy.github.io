namespace Platform;

public static class EnumAchievementManagerAchievementExtensions
{
	public static bool IsSupported(this EnumAchievementManagerAchievement _achievement)
	{
		return AchievementData.GetStat(_achievement).IsSupported();
	}
}
