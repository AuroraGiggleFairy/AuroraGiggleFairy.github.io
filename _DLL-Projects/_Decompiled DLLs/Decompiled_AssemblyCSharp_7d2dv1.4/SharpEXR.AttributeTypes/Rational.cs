namespace SharpEXR.AttributeTypes;

public struct Rational(int numerator, uint denominator)
{
	public readonly int Numerator = numerator;

	public readonly uint Denominator = denominator;

	public double Value => (double)Numerator / (double)Denominator;

	public override string ToString()
	{
		return $"{Numerator}/{Denominator}";
	}
}
