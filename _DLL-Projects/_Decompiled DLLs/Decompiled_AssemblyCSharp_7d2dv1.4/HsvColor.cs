public struct HsvColor
{
	public double H;

	public double S;

	public double V;

	public float normalizedH
	{
		get
		{
			return (float)H / 360f;
		}
		set
		{
			H = (double)value * 360.0;
		}
	}

	public float normalizedS
	{
		get
		{
			return (float)S;
		}
		set
		{
			S = value;
		}
	}

	public float normalizedV
	{
		get
		{
			return (float)V;
		}
		set
		{
			V = value;
		}
	}

	public HsvColor(double h, double s, double v)
	{
		H = h;
		S = s;
		V = v;
	}

	public override string ToString()
	{
		return "{" + H.ToCultureInvariantString("f2") + "," + S.ToCultureInvariantString("f2") + "," + V.ToCultureInvariantString("f2") + "}";
	}
}
