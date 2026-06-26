namespace SharpEXR.AttributeTypes;

public struct V3I(int v0, int v1, int v2)
{
	public int V0 = v0;

	public int V1 = v1;

	public int V2 = v2;

	public int X => V0;

	public int Y => V1;

	public int Z => V2;

	public override string ToString()
	{
		return $"{GetType().Name}: {V0}, {V1}, {V2}";
	}
}
