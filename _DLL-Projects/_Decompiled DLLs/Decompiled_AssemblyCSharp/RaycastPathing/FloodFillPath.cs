using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace RaycastPathing;

[Preserve]
public class FloodFillPath(Vector3 start, Vector3 target) : RaycastPath(start, target)
{
	public List<FloodFillNode> open = new List<FloodFillNode>();

	public List<FloodFillNode> closed = new List<FloodFillNode>();

	public bool IsPosOpen(Vector3 pos)
	{
		return open.Find([PublicizedFrom(EAccessModifier.Internal)] (FloodFillNode n) => n.Position == pos) != null;
	}

	public bool IsPosClosed(Vector3 pos)
	{
		return closed.Find([PublicizedFrom(EAccessModifier.Internal)] (FloodFillNode n) => n.Position == pos) != null;
	}

	public FloodFillNode getLowestScore()
	{
		FloodFillNode result = null;
		float num = float.MaxValue;
		float num2 = float.MaxValue;
		for (int i = 0; i < open.Count; i++)
		{
			FloodFillNode floodFillNode = open[i];
			if (floodFillNode.F <= num && floodFillNode.Heuristic < num2)
			{
				result = floodFillNode;
				num = floodFillNode.F;
				num2 = floodFillNode.Heuristic;
			}
		}
		return result;
	}

	public override void DebugDraw()
	{
		for (int i = 0; i < closed.Count; i++)
		{
			FloodFillNode floodFillNode = closed[i];
			if (floodFillNode.nodeType == cPathNodeType.Air)
			{
				RaycastPathUtils.DrawBounds(floodFillNode.BlockPos, Color.yellow, 0f);
			}
		}
		base.DebugDraw();
	}
}
