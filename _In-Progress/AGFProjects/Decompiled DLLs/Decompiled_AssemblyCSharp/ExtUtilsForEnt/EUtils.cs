using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace ExtUtilsForEnt;

[Preserve]
public class EUtils
{
	public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 1f)
	{
		Utils.DrawLine(start - Origin.position, end - Origin.position, color, color, 10, duration);
	}

	public static void DrawPath(List<Vector3> path, Color start, Color end)
	{
		for (int i = 0; i < path.Count - 1; i++)
		{
			Utils.DrawLine(path[i] - Origin.position, path[i + 1] - Origin.position, start, end, 10, 10f);
		}
	}

	public static void DrawBounds(Vector3 pos, Color color, float duration, float scale = 1f)
	{
		Utils.DrawBoxLines(new Vector3(pos.x, pos.y, pos.z) - Origin.position, new Vector3(pos.x + scale, pos.y + scale, pos.z + scale) - Origin.position, color, duration);
	}

	public static void DrawBounds(Vector3i pos, Color color, float duration, float scale = 1f)
	{
		Utils.DrawBoxLines(new Vector3(pos.x, pos.y, pos.z) - Origin.position, new Vector3((float)pos.x + scale, (float)pos.y + scale, (float)pos.z + scale) - Origin.position, color, duration);
	}

	public static bool isPositionBlocked(Vector3 start, Vector3 end, int layerMask = 0, bool debugDraw = false)
	{
		Vector3 direction = end - start;
		float magnitude = direction.magnitude;
		Ray ray = new Ray(start - Origin.position, direction);
		RaycastHit hitInfo;
		bool flag = Physics.Raycast(ray, out hitInfo, magnitude, layerMask);
		if (debugDraw)
		{
			if (flag)
			{
				Utils.DrawLine(ray.origin, ray.origin + ray.direction * hitInfo.distance, Color.magenta, Color.red, 1, 5f);
			}
			else
			{
				Utils.DrawLine(ray.origin, ray.origin + ray.direction * magnitude, Color.cyan, Color.blue, 1, 5f);
			}
		}
		return flag;
	}

	public static bool isPositionBlocked(Vector3 start, Vector3 end, out RaycastHit hit, int layerMask = 0, bool debugDraw = false)
	{
		Vector3 direction = end - start;
		float magnitude = direction.magnitude;
		Ray ray = new Ray(start - Origin.position, direction);
		bool flag = Physics.Raycast(ray, out hit, magnitude, layerMask);
		if (debugDraw)
		{
			if (flag)
			{
				Utils.DrawLine(ray.origin, ray.origin + ray.direction * hit.distance, Color.magenta, Color.red, 1, 5f);
			}
			else
			{
				Utils.DrawLine(ray.origin, ray.origin + ray.direction * magnitude, Color.cyan, Color.blue, 1, 5f);
			}
		}
		return flag;
	}
}
