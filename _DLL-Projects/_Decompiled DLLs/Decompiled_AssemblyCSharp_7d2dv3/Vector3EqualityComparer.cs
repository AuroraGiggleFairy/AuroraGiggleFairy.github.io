using System.Collections.Generic;
using UnityEngine;

public class Vector3EqualityComparer : IEqualityComparer<Vector3>
{
	public static readonly Vector3EqualityComparer Instance = new Vector3EqualityComparer();

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3EqualityComparer()
	{
	}

	public bool Equals(Vector3 _a, Vector3 _b)
	{
		return _a == _b;
	}

	public int GetHashCode(Vector3 _a)
	{
		return _a.GetHashCode();
	}
}
