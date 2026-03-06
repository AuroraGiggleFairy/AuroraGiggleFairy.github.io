using System;

namespace MusicUtils;

public class ExponentialCurve : LinearCurve
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly double b;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly float min;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly float max;

	public ExponentialCurve(double _base, float _start, float _end, float _startX, float _endX)
		: base((float)Math.Log(_start, _base), (float)Math.Log(_end, _base), _startX, _endX)
	{
		b = _base;
		if (_start < _end)
		{
			min = _start;
			max = _end;
		}
		else
		{
			min = _end;
			max = _start;
		}
	}

	public override float GetMixerValue(float _param)
	{
		return Utils.FastClamp((float)Math.Pow(b, GetLine(_param)), min, max);
	}
}
