namespace SharpEXR.AttributeTypes;

public struct V3F(float v0, float v1, float v2)
{
	public float V0 = v0;

	public float V1 = v1;

	public float V2 = v2;

	public float X => V0;

	public float Y => V1;

	public float Z => V2;

	public override string ToString()
	{
		return $"{GetType().Name}: {V0}, {V1}, {V2}";
	}
}
