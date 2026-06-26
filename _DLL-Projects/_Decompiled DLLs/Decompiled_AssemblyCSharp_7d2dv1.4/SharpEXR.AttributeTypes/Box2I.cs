namespace SharpEXR.AttributeTypes;

public struct Box2I(int xMin, int yMin, int xMax, int yMax)
{
	public readonly int XMin = xMin;

	public readonly int YMin = yMin;

	public readonly int XMax = xMax;

	public readonly int YMax = yMax;

	public int Width => XMax - XMin + 1;

	public int Height => YMax - YMin + 1;

	public override string ToString()
	{
		return $"{GetType().Name}: ({XMin}, {YMin})-({XMax}, {YMax})";
	}
}
