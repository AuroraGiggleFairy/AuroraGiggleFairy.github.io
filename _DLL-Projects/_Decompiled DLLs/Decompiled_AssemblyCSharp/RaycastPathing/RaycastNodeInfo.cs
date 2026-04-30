using UnityEngine;
using UnityEngine.Scripting;

namespace RaycastPathing;

[Preserve]
public class RaycastNodeInfo
{
	public readonly Vector3 Position;

	public readonly Vector3i BlockPos;

	public readonly float Scale;

	public readonly int Depth;

	public readonly Vector3 Min;

	public readonly Vector3 Max;

	public readonly Vector3 Center;

	public RaycastNodeInfo(Vector3 pos, float scale = 1f, int depth = 0)
	{
		Position = pos;
		BlockPos = World.worldToBlockPos(pos);
		Scale = scale;
		Depth = depth;
		Min = pos - Vector3.one * scale * 0.5f;
		Max = pos + Vector3.one * scale * 0.5f;
		Center = (Min + Max) * 0.5f;
	}

	public RaycastNodeInfo(Vector3 min, Vector3 max, float scale = 1f, int depth = 0)
	{
		Position = (min + max) * 0.5f;
		BlockPos = World.worldToBlockPos(Position);
		Scale = scale;
		Depth = depth;
		Min = min;
		Max = max;
		Center = Position;
	}
}
