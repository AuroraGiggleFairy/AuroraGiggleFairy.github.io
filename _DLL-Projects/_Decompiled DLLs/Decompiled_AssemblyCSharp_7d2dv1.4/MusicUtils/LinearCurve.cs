namespace MusicUtils;

public class LinearCurve : Curve
{
	public LinearCurve(float _startY, float _endY, float _startX, float _endX)
		: base(_startY, _endY, _startX, _endX)
	{
	}

	public override float GetMixerValue(float _param)
	{
		return Utils.FastClamp(GetLine(_param), linearStart, linearEnd);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public float GetLine(float _param)
	{
		return rate * (_param - startX) + linearStart;
	}
}
