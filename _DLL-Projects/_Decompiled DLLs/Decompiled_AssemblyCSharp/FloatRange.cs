public readonly struct FloatRange(float _min, float _max)
{
	public readonly float min = _min;

	public readonly float max = _max;

	public bool IsSet()
	{
		if (min == 0f)
		{
			return max != 0f;
		}
		return true;
	}

	public float Random(GameRandom _rnd)
	{
		return _rnd.RandomRange(min, max);
	}

	public override string ToString()
	{
		return "(" + min.ToCultureInvariantString() + "-" + max.ToCultureInvariantString() + ")";
	}
}
