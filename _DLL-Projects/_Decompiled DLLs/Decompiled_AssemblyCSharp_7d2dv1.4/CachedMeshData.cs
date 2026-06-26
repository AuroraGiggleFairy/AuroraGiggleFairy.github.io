using System;
using UnityEngine;

[Serializable]
public class CachedMeshData
{
	public string name;

	public int vertexCount;

	public int triCount;

	public bool ApproximatelyEquals(CachedMeshData other)
	{
		if (name.Equals(other.name) && Mathf.Abs(vertexCount - other.vertexCount) < 10)
		{
			return Mathf.Abs(triCount - other.triCount) < 10;
		}
		return false;
	}
}
