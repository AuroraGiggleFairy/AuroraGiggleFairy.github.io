using System;
using System.Diagnostics;
using UnityEngine;

public class WaterClippingUtils
{
	public const string PropWaterClipPlane = "WaterClipPlane";

	public const string PropWaterFlow = "WaterFlow";

	public static readonly Bounds CubeBounds = new Bounds(new Vector3(0.5f, 0.5f, 0.5f), Vector3.one);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Vector3[] cubeVerts = new Vector3[8]
	{
		new Vector3(0f, 0f, 0f),
		new Vector3(0f, 1f, 0f),
		new Vector3(1f, 1f, 0f),
		new Vector3(1f, 0f, 0f),
		new Vector3(0f, 0f, 1f),
		new Vector3(0f, 1f, 1f),
		new Vector3(1f, 1f, 1f),
		new Vector3(1f, 0f, 1f)
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly int[] cubeEdges = new int[24]
	{
		0, 1, 1, 2, 2, 3, 3, 0, 4, 5,
		5, 6, 6, 7, 7, 4, 0, 4, 1, 5,
		2, 6, 3, 7
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly float[] cubeVertDistances = new float[8];

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly float[] hullVertAngles = new float[6];

	public static bool GetCubePlaneIntersectionEdgeLoop(Plane plane, ref Vector3[] intersectionPoints, out int count)
	{
		count = 0;
		for (int i = 0; i < cubeVerts.Length; i++)
		{
			cubeVertDistances[i] = plane.GetDistanceToPoint(cubeVerts[i]);
		}
		Vector3 zero = Vector3.zero;
		for (int j = 0; j < cubeEdges.Length; j += 2)
		{
			float num = cubeVertDistances[cubeEdges[j]];
			float num2 = cubeVertDistances[cubeEdges[j + 1]];
			if (Mathf.Sign(num) != Mathf.Sign(num2))
			{
				Vector3 b = cubeVerts[cubeEdges[j]];
				Vector3 a = cubeVerts[cubeEdges[j + 1]];
				float t = (0f - num2) / (num - num2);
				Vector3 vector = Vector3.Lerp(a, b, t);
				intersectionPoints[count] = vector;
				zero += vector;
				count++;
			}
		}
		if (count < 3)
		{
			return false;
		}
		zero /= (float)count;
		Vector3 vector2 = intersectionPoints[0] - zero;
		hullVertAngles[0] = 0f;
		for (int k = 1; k < count; k++)
		{
			float num3 = Vector3.SignedAngle(vector2, intersectionPoints[k] - zero, plane.normal);
			hullVertAngles[k] = ((num3 < 0f) ? (num3 + 360f) : num3);
		}
		for (int l = count; l < 6; l++)
		{
			hullVertAngles[l] = 1000f + (float)l;
		}
		Array.Sort(hullVertAngles, intersectionPoints);
		return true;
	}

	[Conditional("DEBUG_WATER_CLIPPING")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void DebugDrawIntersectionSurface(Plane plane, Vector3[] intersectionPoints, int count, Vector3 hullCenter)
	{
		for (int i = 0; i < count; i++)
		{
			Vector3 vector = intersectionPoints[i];
			UnityEngine.Debug.DrawLine(vector + 0.1f * Vector3.down, vector + 0.1f * Vector3.up, Color.white);
			UnityEngine.Debug.DrawLine(vector + 0.1f * Vector3.left, vector + 0.1f * Vector3.right, Color.white);
			UnityEngine.Debug.DrawLine(vector + 0.1f * Vector3.forward, vector + 0.1f * Vector3.back, Color.white);
			UnityEngine.Debug.DrawLine(hullCenter, vector, Color.white);
			UnityEngine.Debug.DrawLine(vector, intersectionPoints[(i + 1) % count], Color.white);
		}
		for (int j = 0; j < cubeVerts.Length; j++)
		{
			Vector3 vector2 = cubeVerts[j] + cubeVertDistances[j] * -plane.normal;
			if (!CubeBounds.Contains(vector2))
			{
				Vector3 end = GeometryUtils.NearestPointOnEdgeLoop(vector2, intersectionPoints, count);
				UnityEngine.Debug.DrawLine(vector2, end, Color.yellow);
			}
		}
	}

	[Conditional("DEBUG_WATER_CLIPPING")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void DebugDrawCubeVertPlaneOffsets(Plane plane)
	{
		for (int i = 0; i < cubeVerts.Length; i++)
		{
			float num = cubeVertDistances[i];
			UnityEngine.Debug.DrawRay(cubeVerts[i], num * -plane.normal, (Mathf.Sign(num) > 0f) ? Color.green : Color.red);
		}
	}
}
