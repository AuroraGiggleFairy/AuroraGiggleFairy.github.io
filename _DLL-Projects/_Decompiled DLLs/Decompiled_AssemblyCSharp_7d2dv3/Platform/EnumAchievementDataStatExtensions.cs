namespace Platform;

public static class EnumAchievementDataStatExtensions
{
	public static bool IsSupported(this EnumAchievementDataStat _stat)
	{
		return PlatformManager.NativePlatform.AchievementManager?.IsAchievementStatSupported(_stat) ?? true;
	}
}
