public class VertexDistortion
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly float[] arrayB;

	[PublicizedFrom(EAccessModifier.Private)]
	static VertexDistortion()
	{
		arrayB = new float[9] { 0f, 0.2f, 0.15f, 0.1f, 0.1f, 0.1f, 0.1f, 0.1f, 0.1f };
		for (int i = 0; i < arrayB.Length; i++)
		{
			arrayB[i] *= 1.5f;
		}
	}
}
