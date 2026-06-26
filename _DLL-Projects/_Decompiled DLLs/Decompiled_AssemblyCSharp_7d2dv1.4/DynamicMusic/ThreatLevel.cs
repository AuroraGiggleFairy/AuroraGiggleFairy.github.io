using MusicUtils.Enums;
using UnityEngine;

namespace DynamicMusic;

public struct ThreatLevel : IThreatLevel
{
	public const float SPOOKED_THRESHOLD = 0.3f;

	public const float PANICKED_THRESHOLD = 0.7f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float SPOOKED_DECAY_TIME = 30f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float suspenseTimer;

	[PublicizedFrom(EAccessModifier.Private)]
	public float numeric;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public ThreatLevelType Category
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public float Numeric
	{
		get
		{
			return numeric;
		}
		set
		{
			if (value < 0.3f && Time.time - suspenseTimer <= 30f)
			{
				numeric = 0.3f;
				Category = ThreatLevelType.Spooked;
				return;
			}
			Category = ((!((numeric = value) < 0.3f)) ? ((value < 0.7f) ? ThreatLevelType.Spooked : ThreatLevelType.Panicked) : ThreatLevelType.Safe);
			if (Category == ThreatLevelType.Spooked)
			{
				suspenseTimer = Time.time;
			}
			else if (Category == ThreatLevelType.Panicked)
			{
				suspenseTimer = 0f;
			}
		}
	}
}
