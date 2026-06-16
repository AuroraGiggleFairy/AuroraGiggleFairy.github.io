namespace SharpEXR.AttributeTypes;

public struct Box2F(float xMin, float yMin, float xMax, float yMax)
{
	public readonly float XMin = xMin;

	public readonly float YMin = yMin;

	public readonly float XMax = xMax;

	public readonly float YMax = yMax;

	public float Width => XMax - XMin + 1f;

	public float Height => YMax - YMin + 1f;

	public override string ToString()
	{
		return $"{GetType().Name}: ({XMin}, {YMin})-({XMax}, {YMax})";
	}
}
