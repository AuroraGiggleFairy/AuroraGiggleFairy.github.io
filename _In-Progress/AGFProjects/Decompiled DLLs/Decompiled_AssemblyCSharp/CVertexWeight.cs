using UnityEngine;

[PublicizedFrom(EAccessModifier.Internal)]
public class CVertexWeight
{
	public int index;

	public Vector3 localPosition;

	public float weight;

	public CVertexWeight(int i, Vector3 p, float w)
	{
		index = i;
		localPosition = p;
		weight = w;
	}
}
