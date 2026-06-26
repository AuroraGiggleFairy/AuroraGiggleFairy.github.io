public abstract class WeatherParams
{
	public static float GeneratedHeatDecayRate = 1f;

	public static float MaxGeneratedHeat = 25f;

	public static float EnclosureDetectionThreshold = 0.85f;

	public static float RainfallSoakRate = 60f;

	public static float SnowfallSoakRate = 600f;

	public static float DryTempCutoff = 36f;

	public static float DryPercentPerSecondPerDegree = 0.00016666666f;

	public static float MaxDryPercentPerSecond = 0.1f;

	public static float CoreTempChangeRateWhenDry = 2f;

	public static float CoreTempChangeRateWhenWet = 5f;

	public static float PositiveTempChangeRateMultiplier = 1f;

	public static float OutsideTempChangeWhenSoaked = -20f;

	public static float BaseOutsideTempChangeWhenWet = -10f;

	public static float WindyOutsideTempChangeWhenWet = -7f;

	public static float WindyOutsideTempChangeWhenDry = -5f;

	public static float OutsideTempChangeWhenInSun = 10f;

	public static float OutsideTempChangeWhenInSunCloudScale = 0.5f;

	public static float OutsideTempShiftWhenRunning = 15f;

	public static float OutsideTempShiftWhenWalking = 0f;

	public static float OutsideTempShiftWhenIdle = -15f;

	public static float DegreesPerPointOfStaminaUsed = 0.5f;

	public static float ColdTempChangeThreshhold = 32f;

	public static float HotTempChangeThreshhold = 110f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public WeatherParams()
	{
	}
}
