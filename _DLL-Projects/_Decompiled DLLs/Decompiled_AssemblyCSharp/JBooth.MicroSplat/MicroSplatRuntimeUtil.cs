using UnityEngine;

namespace JBooth.MicroSplat;

public class MicroSplatRuntimeUtil
{
	public static Vector2 UnityUVScaleToUVScale(Vector2 uv, Terrain t)
	{
		float x = t.terrainData.size.x;
		float z = t.terrainData.size.z;
		uv.x = 1f / (uv.x / x);
		uv.y = 1f / (uv.y / z);
		return uv;
	}

	public static Vector2 UVScaleToUnityUVScale(Vector2 uv, Terrain t)
	{
		float x = t.terrainData.size.x;
		float z = t.terrainData.size.z;
		if (uv.x < 0f)
		{
			uv.x = 0.001f;
		}
		if (uv.y < 0f)
		{
			uv.y = 0.001f;
		}
		uv.x = x / uv.x;
		uv.y = z / uv.y;
		return uv;
	}
}
