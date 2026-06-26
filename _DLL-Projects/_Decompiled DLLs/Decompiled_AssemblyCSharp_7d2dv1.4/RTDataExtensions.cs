using GameSparks.RT;
using UnityEngine;

public static class RTDataExtensions
{
	public static RTData SetVector2(this RTData data, uint index, Vector2 vector2)
	{
		data.SetRTVector(index, new RTVector(vector2.x, vector2.y));
		return data;
	}

	public static Vector2? GetVector2(this RTData data, uint index)
	{
		if (data.GetRTVector(index).HasValue)
		{
			RTVector value = data.GetRTVector(index).Value;
			return new Vector2(value.x.Value, value.y.Value);
		}
		return null;
	}

	public static RTData SetVector3(this RTData data, uint index, Vector3 vector3)
	{
		data.SetRTVector(index, new RTVector(vector3.x, vector3.y, vector3.z));
		return data;
	}

	public static Vector3? GetVector3(this RTData data, uint index)
	{
		if (data.GetRTVector(index).HasValue)
		{
			RTVector value = data.GetRTVector(index).Value;
			if (!value.z.HasValue)
			{
				return null;
			}
			return new Vector3(value.x.Value, value.y.Value, value.z.Value);
		}
		return null;
	}

	public static RTData SetVector4(this RTData data, uint index, Vector4 vector4)
	{
		data.SetRTVector(index, new RTVector(vector4.x, vector4.y, vector4.z, vector4.w));
		return data;
	}

	public static Vector4? GetVector4(this RTData data, uint index)
	{
		if (data.GetRTVector(index).HasValue)
		{
			RTVector value = data.GetRTVector(index).Value;
			if (!value.w.HasValue)
			{
				return null;
			}
			return new Vector4(value.x.Value, value.y.Value, value.z.Value, value.w.Value);
		}
		return null;
	}
}
