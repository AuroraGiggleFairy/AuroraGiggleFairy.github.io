namespace SharpEXR.AttributeTypes;

public struct M33F
{
	public readonly float[] Values = new float[9];

	public M33F(float v0, float v1, float v2, float v3, float v4, float v5, float v6, float v7, float v8)
	{
		Values[0] = v0;
		Values[1] = v1;
		Values[2] = v2;
		Values[3] = v3;
		Values[4] = v4;
		Values[5] = v5;
		Values[6] = v6;
		Values[7] = v7;
		Values[8] = v8;
	}
}
