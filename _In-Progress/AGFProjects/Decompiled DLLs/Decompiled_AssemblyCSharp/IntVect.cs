public struct IntVect(int x, int y, int z)
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int m_X = x;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int m_Y = y;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int m_Z = z;

	public int X => m_X;

	public int Y => m_Y;

	public int Z => m_Z;

	public override bool Equals(object obj)
	{
		IntVect intVect = (IntVect)obj;
		if (intVect.X == m_X && intVect.Y == m_Y)
		{
			return intVect.Z == m_Z;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return m_X * 8976890 + m_Y * 981131 + m_Z;
	}

	public static bool operator ==(IntVect one, IntVect other)
	{
		if (one.X == other.X && one.Y == other.Y)
		{
			return one.Z == other.Z;
		}
		return false;
	}

	public static bool operator !=(IntVect one, IntVect other)
	{
		return !(one == other);
	}
}
