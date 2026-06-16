using UnityEngine.Scripting;

[Preserve]
public class AIDirectorConstants
{
	public static bool DebugOutput = true;

	public const int kFileVersion = 10;

	public const int kMaxSupplyCrates = 12;

	public const float kStealthSightDistanceMultiplier = 0.8f;

	public const float kStealthNighttimeSightDistanceMultiplier = 0.5f;

	public const float kHordeMeterWarn1Threshold = 0.5f;

	public const float kHordeMeterWarn2Threshold = 0.8f;

	public const float kHordeMeterWarnResetThreshold = 0.2f;

	public const int kHordeDaySpawnRangeMin = 45;

	public const int kHordeDaySpawnRangeMax = 55;

	public const int kHordeNightSpawnRangeMin = 55;

	public const int kHordeNightSpawnRangeMax = 70;

	public const float kHordeMeterDecayDelay = 8f;

	public const float kHordeMeterDecayRate = 4f;

	public const int kWanderingHordeGlobalStartTime = 28000;

	public const int kSpawnWanderingHordeMin = 12000;

	public const int kSpawnWanderingHordeMax = 24000;

	public const int kWanderingHordeGroupSize = 6;

	public const float kWanderingHordeSpawnDistance = 92f;

	public const float kWanderingHordeSpawnMinDistance = 50f;

	public const float kWanderingHordePlayerClusterSize = 30f;

	public const int kSoundPriorityStart = 10;

	public const int kSoundPriorityRange = 100;

	public const int kScoutSpawnDistance = 80;

	public const float kScoutScreamGraceTime = 2f;

	public const float kScoutScreamAgainTime = 18f;

	public const float kScoutSpawnAnotherScoutChance = 0.12f;

	public const int kScoutSummonedPerScream = 5;

	public const int kScoutSummonedTotal = 25;
}
