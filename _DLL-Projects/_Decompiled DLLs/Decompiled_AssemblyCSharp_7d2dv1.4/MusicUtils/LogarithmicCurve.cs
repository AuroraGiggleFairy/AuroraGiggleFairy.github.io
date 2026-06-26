using System;

namespace MusicUtils;

public class LogarithmicCurve : LinearCurve
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly double b;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly float min;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly float max;

	public LogarithmicCurve(double _base, double _scale, float _start, float _end, float _startX, float _endX)
		: base((float)Math.Pow(_base, (double)_start / _scale), (float)Math.Pow(_base, (double)_end / _scale), _startX, _endX)
	{
		b = Math.Pow(_base, 1.0 / _scale);
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
		return Utils.FastClamp((float)Math.Log(Math.Max(GetLine(_param), 0f), b), min, max);
	}
}
