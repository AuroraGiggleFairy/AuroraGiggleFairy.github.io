using UnityEngine;

public class GeometryUtils
{
	public static Vector3 NearestPointOnLine(Vector3 fromPoint, Vector3 lineStart, Vector3 lineEnd)
	{
		Vector3 lhs = fromPoint - lineStart;
		Vector3 vector = lineEnd - lineStart;
		float num = Mathf.Clamp01(Vector3.Dot(lhs, vector) / Vector3.Dot(vector, vector));
		return lineStart + num * vector;
	}

	public static Vector3 NearestPointOnEdgeLoop(Vector3 fromPoint, Vector3[] loopPoints, int loopPointCount)
	{
		(float, Vector3) tuple = (float.MaxValue, fromPoint);
		for (int i = 0; i < loopPointCount; i++)
		{
			Vector3 lineStart = loopPoints[i];
			Vector3 lineEnd = loopPoints[(i + 1) % loopPointCount];
			Vector3 vector = NearestPointOnLine(fromPoint, lineStart, lineEnd);
			float num = Vector3.Distance(fromPoint, vector);
			if (num < tuple.Item1)
			{
				tuple = (num, vector);
			}
		}
		return tuple.Item2;
	}

	public static void RotatePlaneAroundPoint(ref Plane plane, Vector3 pivot, Quaternion rotation)
	{
		if (!(rotation == Quaternion.identity))
		{
			Vector3 normal = plane.normal;
			Vector3 vector = plane.normal * (0f - plane.distance);
			vector = rotation * (vector - pivot) + pivot;
			normal = rotation * normal;
			plane.SetNormalAndPosition(normal, vector);
		}
	}
}
