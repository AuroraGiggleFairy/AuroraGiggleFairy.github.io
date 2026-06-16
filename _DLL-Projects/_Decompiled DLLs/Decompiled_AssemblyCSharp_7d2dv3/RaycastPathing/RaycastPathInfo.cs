using UnityEngine;
using UnityEngine.Scripting;

namespace RaycastPathing;

[Preserve]
public class RaycastPathInfo
{
	public readonly Vector3 Start;

	public readonly Vector3 Target;

	[PublicizedFrom(EAccessModifier.Private)]
	public RaycastNode StartNode;

	[PublicizedFrom(EAccessModifier.Private)]
	public RaycastNode TargetNode;

	public Vector3i StartBlockPos => StartNode.BlockPos;

	public Vector3i TargetBlockPos => TargetNode.BlockPos;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool PathStartsIndoors
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool PathEndsIndoors
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public RaycastPathInfo(Vector3 start, Vector3 target)
	{
		Start = start;
		Target = target;
		StartNode = new RaycastNode(Start);
		TargetNode = new RaycastNode(Target);
		PathStartsIndoors = RaycastPathWorldUtils.IsUnderground(start);
		PathEndsIndoors = RaycastPathWorldUtils.IsUnderground(target);
	}

	public static implicit operator bool(RaycastPathInfo exists)
	{
		return exists != null;
	}
}
