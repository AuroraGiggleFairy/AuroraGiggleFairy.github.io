public readonly struct IntRange(int _min, int _max)
{
	public readonly int min = _min;

	public readonly int max = _max;

	public bool IsSet()
	{
		if (min == 0)
		{
			return max != 0;
		}
		return true;
	}

	public float Random(GameRandom _rnd)
	{
		return _rnd.RandomRange(min, max);
	}

	public override string ToString()
	{
		string[] obj = new string[5] { "(", null, null, null, null };
		int num = min;
		obj[1] = num.ToString();
		obj[2] = "-";
		num = max;
		obj[3] = num.ToString();
		obj[4] = ")";
		return string.Concat(obj);
	}
}
