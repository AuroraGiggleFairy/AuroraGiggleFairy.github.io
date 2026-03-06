using MusicUtils.Enums;

namespace DynamicMusic;

public abstract class AbstractMusicTimeTracker : AbstractFilter, IFilter<SectionType>
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public float dailyAllottedPlayTime;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float dailyPlayTimeUsed;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float musicStartTime;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool IsMusicPlaying;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float pauseDuration;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float pauseStartTime;

	[PublicizedFrom(EAccessModifier.Protected)]
	public AbstractMusicTimeTracker()
	{
	}
}
