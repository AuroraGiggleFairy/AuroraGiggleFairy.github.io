using System.Collections.Generic;
using UnityEngine;

namespace Assets.DuckType.Jiggle;

public static class Extensions
{
	public static Quaternion Append(this Quaternion source, Quaternion quaternion)
	{
		return quaternion * source;
	}

	public static Quaternion FromToRotation(this Quaternion source, Quaternion target)
	{
		return Quaternion.Inverse(source) * target;
	}

	public static Quaternion Scale(this Quaternion source, float scale)
	{
		return Quaternion.SlerpUnclamped(Quaternion.identity, source, scale);
	}

	public static Quaternion Inverse(this Quaternion source)
	{
		return Quaternion.Inverse(source);
	}

	public static List<Vector3> GetOrthogonalVectors(this Vector3 source, int numVectors)
	{
		_ = source.normalized;
		Vector3 vector = ((Mathf.Abs(source.normalized.y) != 1f) ? Vector3.Cross(source, Vector3.up) : Vector3.Cross(source, Vector3.right));
		float num = 360f / (float)numVectors;
		List<Vector3> list = new List<Vector3>();
		for (int i = 0; i < numVectors; i++)
		{
			list.Add(Quaternion.AngleAxis(num * (float)i, source) * vector);
		}
		return list;
	}

	public static bool HasLength(this Vector3 source)
	{
		if (source.x == 0f && source.y == 0f)
		{
			return source.z != 0f;
		}
		return true;
	}

	public static float Clamp01(this float source)
	{
		return Mathf.Max(Mathf.Min(source, 1f), 0f);
	}
}
