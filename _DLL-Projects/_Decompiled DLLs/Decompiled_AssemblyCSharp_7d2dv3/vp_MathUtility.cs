using UnityEngine;

public static class vp_MathUtility
{
	public static float NaNSafeFloat(float value, float prevValue = 0f)
	{
		value = (double.IsNaN(value) ? prevValue : value);
		return value;
	}

	public static Vector2 NaNSafeVector2(Vector2 vector, Vector2 prevVector = default(Vector2))
	{
		vector.x = (double.IsNaN(vector.x) ? prevVector.x : vector.x);
		vector.y = (double.IsNaN(vector.y) ? prevVector.y : vector.y);
		return vector;
	}

	public static Vector3 NaNSafeVector3(Vector3 vector, Vector3 prevVector = default(Vector3))
	{
		vector.x = (double.IsNaN(vector.x) ? prevVector.x : vector.x);
		vector.y = (double.IsNaN(vector.y) ? prevVector.y : vector.y);
		vector.z = (double.IsNaN(vector.z) ? prevVector.z : vector.z);
		return vector;
	}

	public static Quaternion NaNSafeQuaternion(Quaternion quaternion, Quaternion prevQuaternion = default(Quaternion))
	{
		quaternion.x = (double.IsNaN(quaternion.x) ? prevQuaternion.x : quaternion.x);
		quaternion.y = (double.IsNaN(quaternion.y) ? prevQuaternion.y : quaternion.y);
		quaternion.z = (double.IsNaN(quaternion.z) ? prevQuaternion.z : quaternion.z);
		quaternion.w = (double.IsNaN(quaternion.w) ? prevQuaternion.w : quaternion.w);
		return quaternion;
	}

	public static Vector3 SnapToZero(Vector3 value, float epsilon = 0.0001f)
	{
		value.x = ((Mathf.Abs(value.x) < epsilon) ? 0f : value.x);
		value.y = ((Mathf.Abs(value.y) < epsilon) ? 0f : value.y);
		value.z = ((Mathf.Abs(value.z) < epsilon) ? 0f : value.z);
		return value;
	}

	public static float SnapToZero(float value, float epsilon = 0.0001f)
	{
		value = ((Mathf.Abs(value) < epsilon) ? 0f : value);
		return value;
	}

	public static float ReduceDecimals(float value, float factor = 1000f)
	{
		return Mathf.Round(value * factor) / factor;
	}

	public static bool IsOdd(int val)
	{
		return val % 2 != 0;
	}

	public static float Sinus(float rate, float amp, float offset = 0f)
	{
		return Mathf.Cos((Time.time + offset) * rate) * amp;
	}
}
