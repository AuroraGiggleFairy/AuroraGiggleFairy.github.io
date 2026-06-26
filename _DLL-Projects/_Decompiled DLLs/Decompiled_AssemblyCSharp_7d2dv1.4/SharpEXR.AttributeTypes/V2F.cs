namespace SharpEXR.AttributeTypes;

public struct V2F(float v0, float v1)
{
	public float V0 = v0;

	public float V1 = v1;

	public float X => V0;

	public float Y => V1;

	public override string ToString()
	{
		return $"{GetType().Name}: {V0}, {V1}";
	}
}
