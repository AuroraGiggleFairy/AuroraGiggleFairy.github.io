using System;
using UnityEngine;

public static class OversizedBlockUtils
{
	public static Bounds GetLocalStabilityBounds(Bounds localBounds, Quaternion rotation)
	{
		Vector3 vector = Quaternion.Inverse(rotation) * Vector3.down;
		float num = Vector3.Dot(vector, Vector3.one);
		if (!Mathf.Approximately(Mathf.Abs(num), 1f))
		{
			Debug.LogError($"Error in GetLocalStabilityBounds: relativeDown is not an axis-aligned unit vector. Value: {vector}. This could suggest an oversized block has been used with an unsupported rotation.");
		}
		Vector3 min;
		Vector3 max;
		if (num < 0f)
		{
			min = localBounds.min + vector;
			max = localBounds.max + Vector3.Scale(vector, localBounds.size);
		}
		else
		{
			min = localBounds.min + Vector3.Scale(vector, localBounds.size);
			max = localBounds.max + vector;
		}
		Bounds result = default(Bounds);
		result.SetMinMax(min, max);
		result.extents += new Vector3(-0.05f, -0.05f, -0.05f);
		return result;
	}

	public static void GetWorldAlignedBoundsExtents(Vector3i position, Quaternion rotation, Bounds localBounds, out Vector3i min, out Vector3i max)
	{
		Span<Vector3> corners = stackalloc Vector3[8];
		GetWorldCorners(ref corners, position, rotation, localBounds);
		min = new Vector3i(int.MaxValue, int.MaxValue, int.MaxValue);
		max = new Vector3i(int.MinValue, int.MinValue, int.MinValue);
		Span<Vector3> span = corners;
		for (int i = 0; i < span.Length; i++)
		{
			Vector3 vector = span[i];
			min.x = Mathf.Min(min.x, Mathf.FloorToInt(vector.x));
			min.y = Mathf.Min(min.y, Mathf.FloorToInt(vector.y));
			min.z = Mathf.Min(min.z, Mathf.FloorToInt(vector.z));
			max.x = Mathf.Max(max.x, Mathf.CeilToInt(vector.x));
			max.y = Mathf.Max(max.y, Mathf.CeilToInt(vector.y));
			max.z = Mathf.Max(max.z, Mathf.CeilToInt(vector.z));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void GetWorldCorners(ref Span<Vector3> corners, Vector3i position, Quaternion rotation, Bounds localBounds)
	{
		Vector3 extents = localBounds.extents;
		for (int i = 0; i < 8; i++)
		{
			corners[i] = localBounds.center + new Vector3(((i & 1) == 0) ? extents.x : (0f - extents.x), ((i & 2) == 0) ? extents.y : (0f - extents.y), ((i & 4) == 0) ? extents.z : (0f - extents.z));
			corners[i] = position + rotation * corners[i];
		}
	}

	public static Matrix4x4 GetBlockWorldToLocalMatrix(Vector3i position, Quaternion rotation)
	{
		return Matrix4x4.TRS(position + new Vector3(0.5f, 0.5f, 0.5f), rotation, Vector3.one).inverse;
	}

	public static bool IsBlockCenterWithinBounds(Vector3i blockPosition, Bounds localBounds, Matrix4x4 worldToLocalMatrix)
	{
		Vector3 point = worldToLocalMatrix.MultiplyPoint3x4(blockPosition + new Vector3(0.5f, 0.5f, 0.5f));
		return localBounds.Contains(point);
	}
}
