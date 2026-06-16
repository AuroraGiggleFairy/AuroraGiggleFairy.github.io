using UnityEngine;

public class GeometryUtils
{
	public struct Triangle(Vector3 v0, Vector3 v1, Vector3 v2)
	{
		public Vector3 v0 = v0;

		public Vector3 v1 = v1;

		public Vector3 v2 = v2;

		public Vector3 normal = Vector3.Normalize(Vector3.Cross(v0 - v2, v0 - v1));
	}

	public static bool IntersectRayTriangle(Ray ray, Triangle tri, out Vector3 outNormal, out float hitDistance)
	{
		outNormal = Vector3.zero;
		hitDistance = 0f;
		Vector3 lhs = tri.v1 - tri.v0;
		Vector3 lhs2 = tri.v2 - tri.v0;
		Vector3 normal = tri.normal;
		float num = Vector3.Dot(normal, ray.direction);
		if (Mathf.Abs(num) < Mathf.Epsilon)
		{
			return false;
		}
		float num2 = (Vector3.Dot(normal, tri.v0) - Vector3.Dot(normal, ray.origin)) / num;
		if (num2 < 0f)
		{
			return false;
		}
		Vector3 vector = ray.origin + ray.direction * num2;
		if (Vector3.Dot(normal, Vector3.Cross(lhs, vector - tri.v0)) > 0f || Vector3.Dot(normal, Vector3.Cross(lhs2, tri.v2 - vector)) > 0f || Vector3.Dot(normal, Vector3.Cross(tri.v2 - tri.v1, vector - tri.v1)) > 0f)
		{
			return false;
		}
		hitDistance = num2;
		outNormal = normal.normalized;
		return true;
	}

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

	public static Rect RotateRectAboutY(Rect rect, float yRot)
	{
		Quaternion quaternion = Quaternion.AngleAxis(yRot, Vector3.up);
		Vector3 zero = Vector3.zero;
		zero.x = rect.min.x - rect.center.x;
		zero.z = rect.min.y - rect.center.y;
		Vector3 zero2 = Vector3.zero;
		zero2.x = rect.max.x - rect.center.x;
		zero2.z = rect.max.y - rect.center.y;
		zero = quaternion * zero;
		zero2 = quaternion * zero2;
		zero.x += rect.center.x;
		zero.z += rect.center.y;
		zero2.x += rect.center.x;
		zero2.z += rect.center.y;
		if (zero2.x < zero.x)
		{
			float x = zero.x;
			zero.x = zero2.x;
			zero2.x = x;
		}
		if (zero2.z < zero.z)
		{
			float z = zero.z;
			zero.z = zero2.z;
			zero2.z = z;
		}
		return new Rect(new Vector2(zero.x, zero.z), new Vector2(zero2.x - zero.x, zero2.x - zero.x));
	}
}
