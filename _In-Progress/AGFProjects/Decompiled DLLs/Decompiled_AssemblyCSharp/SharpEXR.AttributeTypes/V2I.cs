namespace SharpEXR.AttributeTypes;

public struct V2I(int v0, int v1)
{
	public int V0 = v0;

	public int V1 = v1;

	public int X => V0;

	public int Y => V1;

	public override string ToString()
	{
		return $"{GetType().Name}: {V0}, {V1}";
	}
}
