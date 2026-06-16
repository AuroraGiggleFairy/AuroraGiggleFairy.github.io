using System.Collections.Generic;
using UnityEngine;

public class Vector2EqualityComparer : IEqualityComparer<Vector2>
{
	public static readonly Vector2EqualityComparer Instance = new Vector2EqualityComparer();

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2EqualityComparer()
	{
	}

	public bool Equals(Vector2 _a, Vector2 _b)
	{
		return _a == _b;
	}

	public int GetHashCode(Vector2 _a)
	{
		return _a.GetHashCode();
	}
}
