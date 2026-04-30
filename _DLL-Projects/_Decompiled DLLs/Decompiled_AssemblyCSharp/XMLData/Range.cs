namespace XMLData;

public class Range<TValue>
{
	public bool hasMin;

	public bool hasMax;

	public TValue min;

	public TValue max;

	public Range()
	{
	}

	public Range(bool _hasMin, TValue _min, bool _hasMax, TValue _max)
	{
		hasMin = _hasMin;
		hasMax = _hasMax;
		min = _min;
		max = _max;
	}

	public override string ToString()
	{
		return string.Format("{0}-{1}", hasMin ? min.ToString() : "*", hasMax ? max.ToString() : "*");
	}
}
