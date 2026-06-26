public class LightingAround
{
	public enum Pos
	{
		Middle,
		X0Y0Z0,
		X1Y0Z0,
		X1Y0Z1,
		X0Y0Z1,
		X0Y1Z0,
		X1Y1Z0,
		X1Y1Z1,
		X0Y1Z1
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cCount = 9;

	[PublicizedFrom(EAccessModifier.Private)]
	public Lighting[] lights = new Lighting[9];

	public Lighting this[Pos _pos]
	{
		get
		{
			return lights[(int)_pos];
		}
		set
		{
			lights[(int)_pos] = value;
		}
	}

	public LightingAround(byte _sun, byte _block, byte _stabilityMiddle)
	{
		Lighting lighting = new Lighting(_sun, _block, _stabilityMiddle);
		for (int i = 0; i < 9; i++)
		{
			lights[i] = lighting;
		}
	}

	public void SetStab(byte _stab)
	{
		for (int i = 0; i < 9; i++)
		{
			lights[i].stability = _stab;
		}
	}
}
