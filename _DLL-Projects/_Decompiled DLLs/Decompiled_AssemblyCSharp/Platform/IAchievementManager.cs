namespace Platform;

public interface IAchievementManager
{
	void Init(IPlatform _owner);

	void ShowAchievementsUi();

	bool IsAchievementStatSupported(EnumAchievementDataStat _stat);

	void SetAchievementStat(EnumAchievementDataStat _stat, int _value);

	void SetAchievementStat(EnumAchievementDataStat _stat, float _value);

	void ResetStats(bool _andAchievements);

	void UnlockAllAchievements();

	void Destroy();
}
