using System.Collections.Generic;

namespace MusicUtils;

public static class SignalProcessing
{
	public const float cSilentVolume = -80f;

	public const float cMaxLPFCutoff = 22000f;

	public const float cPauseLowPassFilterCutoff = 500f;

	public const double cdBFullScaleBase = 1.12246204831;

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cExplorationLowPassCutoff = "ExpLPFCutOff";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cExplorationReverbDryLevel = "ExpReverbDryLevel";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cExplorationReverbRoom = "ExpReverbRoom";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cExplorationReverbRoomHF = "ExpReverbRoomHF";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cExplorationDecayHFRatio = "DecayHFRatio";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cExplorationReflectDelay = "ExpReflectDelay";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cExplorationHFReference = "ExpHFReference";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cExplorationReverbRoomLF = "ExpReverbRoomLF";

	public static readonly Dictionary<string, Curve> DspCurves = new Dictionary<string, Curve>
	{
		{
			"ExpLPFCutOff",
			new ExponentialCurve(2.0, 22000f, 750f, 0.25f, 0.5f)
		},
		{
			"ExpReverbDryLevel",
			new LogarithmicCurve(2.0, 600.0, 0f, -600f, 0.25f, 0.5f)
		},
		{
			"ExpReverbRoom",
			new LogarithmicCurve(2.0, 600.0, -10000f, -250f, 0.25f, 0.5f)
		},
		{
			"ExpReverbRoomHF",
			new LogarithmicCurve(2.0, 600.0, -600f, -250f, 0.25f, 0.5f)
		},
		{
			"DecayHFRatio",
			new LinearCurve(0.1f, 1f, 0.25f, 0.5f)
		},
		{
			"ExpReflectDelay",
			new LinearCurve(0.02f, 0.2f, 0.25f, 0.5f)
		},
		{
			"ExpHFReference",
			new ExponentialCurve(2.0, 1244.508f, 1174.659f, 0.25f, 0.5f)
		},
		{
			"ExpReverbRoomLF",
			new LogarithmicCurve(2.0, 600.0, -600f, 0f, 0.25f, 0.5f)
		}
	};

	public static float SuspenseRange => CombatReadyThreshold - SuspenseThreshold;

	public static float SuspenseThreshold
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return 0.25f;
		}
	}

	public static float CombatReadyThreshold
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return 0.5f;
		}
	}
}
