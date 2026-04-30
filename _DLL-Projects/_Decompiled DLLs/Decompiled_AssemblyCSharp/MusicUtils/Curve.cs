namespace MusicUtils;

public abstract class Curve
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly float rate;

	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly float linearStart;

	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly float linearEnd;

	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly float startX;

	public Curve(float _start, float _end, float _startX, float _endX)
	{
		rate = (_end - _start) / (_endX - _startX);
		linearStart = _start;
		linearEnd = _end;
		startX = _startX;
	}

	public abstract float GetMixerValue(float _param);
}
