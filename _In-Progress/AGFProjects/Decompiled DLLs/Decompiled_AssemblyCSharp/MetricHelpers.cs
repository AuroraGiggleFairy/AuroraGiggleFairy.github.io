using UnityEngine;

public static class MetricHelpers
{
	public static CallbackMetric TextureStreamingCurrent = new CallbackMetric
	{
		Header = "Texture Streaming Current",
		callback = [PublicizedFrom(EAccessModifier.Internal)] () => $"{(double)Texture.currentTextureMemory * 9.5367431640625E-07:F2}"
	};

	public static CallbackMetric TextureStreamingTarget = new CallbackMetric
	{
		Header = "Texture Streaming Target",
		callback = [PublicizedFrom(EAccessModifier.Internal)] () => $"{(double)Texture.targetTextureMemory * 9.5367431640625E-07:F2}"
	};

	public static CallbackMetric TextureStreamingDesired = new CallbackMetric
	{
		Header = "Texture Streaming Desired",
		callback = [PublicizedFrom(EAccessModifier.Internal)] () => $"{(double)Texture.desiredTextureMemory * 9.5367431640625E-07:F2}"
	};

	public static CallbackMetric TextureStreamingNonStreamed = new CallbackMetric
	{
		Header = "Texture Streaming Non-Streamed",
		callback = [PublicizedFrom(EAccessModifier.Internal)] () => $"{(double)Texture.nonStreamingTextureMemory * 9.5367431640625E-07:F2}"
	};

	public static CallbackMetric TextureStreamingBudget = new CallbackMetric
	{
		Header = "Texture Streaming Budget",
		callback = [PublicizedFrom(EAccessModifier.Internal)] () => $"{(QualitySettings.streamingMipmapsActive ? QualitySettings.streamingMipmapsMemoryBudget : (-1f)):F2}"
	};
}
