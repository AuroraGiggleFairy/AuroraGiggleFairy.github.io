using UnityEngine;
using UnityEngine.Scripting;

namespace RaycastPathing;

[Preserve]
public class RaycastPathUtils
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cMaxRayDist = 100;

	public static bool IsPositionBlocked(Vector3 start, Vector3 end, out RaycastHit hit, int layerMask = 0, bool debugDraw = false)
	{
		Vector3 direction = end - start;
		return IsPositionBlocked(new Ray(start - Origin.position, direction), out hit, layerMask, debugDraw, direction.magnitude + 1f);
	}

	public static bool IsPositionBlocked(Vector3 start, Vector3 end, int layerMask = 0, bool debugDraw = false)
	{
		RaycastHit hit;
		return IsPositionBlocked(start, end, out hit, layerMask, debugDraw);
	}

	public static bool IsPointBlocked(Vector3 start, Vector3 end, int layerMask = 0, bool debugDraw = false, float duration = 0f)
	{
		RaycastHit hit;
		return CheckPositionBlocked(start, end, out hit, layerMask, debugDraw, duration);
	}

	public static bool CheckPositionBlocked(Vector3 start, Vector3 end, out RaycastHit hit, int layerMask = 0, bool debugDraw = false, float duration = 0f)
	{
		Vector3 direction = end - start;
		return CheckPositionBlocked(new Ray(start - Origin.position, direction), out hit, layerMask, debugDraw, direction.magnitude, duration);
	}

	public static bool CheckPositionBlocked(Ray ray, out RaycastHit hit, int layerMask = 0, bool debugDraw = false, float maxDist = 100f, float duration = 0f)
	{
		bool flag = Physics.Raycast(ray, out hit, maxDist, layerMask);
		if (debugDraw)
		{
			if (flag)
			{
				Utils.DrawLine(ray.origin, ray.origin + ray.direction * hit.distance, Color.magenta, Color.red, 1, duration);
			}
			else
			{
				Utils.DrawLine(ray.origin, ray.origin + ray.direction * maxDist, Color.cyan, Color.blue, 1, duration);
			}
		}
		return flag;
	}

	public static bool IsPositionBlocked(Ray ray, out RaycastHit hit, int layerMask = 0, bool debugDraw = false, float maxDist = 100f)
	{
		bool flag = Physics.Raycast(ray, out hit, maxDist, layerMask);
		if (debugDraw)
		{
			if (flag)
			{
				Utils.DrawLine(ray.origin, ray.origin + ray.direction * hit.distance, Color.magenta, Color.red, 1, 5f);
			}
			else
			{
				Utils.DrawLine(ray.origin, ray.origin + ray.direction * maxDist, Color.cyan, Color.blue, 1, 5f);
			}
		}
		return flag;
	}

	public static bool IsPositionBlocked(Ray ray, int layerMask = 0, bool debugDraw = false, float maxDist = 100f)
	{
		RaycastHit hit;
		return IsPositionBlocked(ray, out hit, layerMask, debugDraw);
	}

	public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 1f)
	{
		Utils.DrawLine(start - Origin.position, end - Origin.position, color, color, 10, duration);
	}

	public static void DrawBounds(Vector3i pos, Color color, float duration, float scale = 1f)
	{
		Utils.DrawBoxLines(new Vector3(pos.x, pos.y, pos.z) - Origin.position, new Vector3((float)pos.x + scale, (float)pos.y + scale, (float)pos.z + scale) - Origin.position, color, duration);
	}

	public static void DrawBounds(Vector3 pos, Color color, float duration, float scale = 1f)
	{
		Utils.DrawBoxLines(new Vector3i(pos.x, pos.y, pos.z) - Origin.position, new Vector3i(pos.x + scale, pos.y + scale, pos.z + scale) - Origin.position, color, duration);
	}

	public static void DrawNode(RaycastNode node, Color color, float duration)
	{
		DrawNode(node.Min, node.Max, color, duration);
	}

	public static void DrawNode(Vector3 min, Vector3 max, Color color, float duration)
	{
		DrawVolume(min - Origin.position, max - Origin.position, color, duration);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void DrawVolume(Vector3 min, Vector3 max, Color color, float duration)
	{
		Vector3 vector = new Vector3(max.x, min.y, min.z);
		Vector3 vector2 = new Vector3(min.x, max.y, min.z);
		Vector3 end = new Vector3(min.x, min.y, max.z);
		Vector3 vector3 = new Vector3(min.x, max.y, max.z);
		Vector3 vector4 = new Vector3(max.x, min.y, max.z);
		Vector3 end2 = new Vector3(max.x, max.y, min.z);
		Debug.DrawLine(min, vector, color, duration);
		Debug.DrawLine(min, vector2, color, duration);
		Debug.DrawLine(min, end, color, duration);
		Debug.DrawLine(max, vector3, color, duration);
		Debug.DrawLine(max, vector4, color, duration);
		Debug.DrawLine(max, end2, color, duration);
		Debug.DrawLine(vector3, vector2, color, duration);
		Debug.DrawLine(vector, vector4, color, duration);
		Debug.DrawLine(vector4, end, color, duration);
		Debug.DrawLine(vector2, end2, color, duration);
		Debug.DrawLine(vector3, end, color, duration);
		Debug.DrawLine(vector, end2, color, duration);
	}
}
