using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace RaycastPathing;

[Preserve]
public class RaycastPath
{
	public List<RaycastNode> Nodes = new List<RaycastNode>();

	public List<Vector3> ProjectedPoints = new List<Vector3>();

	[field: PublicizedFrom(EAccessModifier.Private)]
	public RaycastPathInfo Info
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public Vector3 Start => Info.Start;

	public Vector3 Target => Info.Target;

	public Vector3i StartBlockPos => Info.StartBlockPos;

	public Vector3i TargetBlockPos => Info.TargetBlockPos;

	public bool PathStartsIndoors => Info.PathStartsIndoors;

	public bool PathEndsIndoors => Info.PathEndsIndoors;

	public RaycastPath(Vector3 start, Vector3 target)
	{
		Info = new RaycastPathInfo(start, target);
		RaycastPathManager.Instance.Add(this);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	~RaycastPath()
	{
		Destruct();
	}

	public void Destruct()
	{
		RaycastPathManager.Instance.Remove(this);
	}

	public void AddNode(RaycastNode node)
	{
		if (!Nodes.Contains(node))
		{
			Nodes.Add(node);
		}
	}

	public void AddProjectedPoint(Vector3 point)
	{
		if (!ProjectedPoints.Contains(point))
		{
			ProjectedPoints.Add(point);
		}
	}

	public virtual void DebugDraw()
	{
		for (int i = 0; i < ProjectedPoints.Count - 1; i++)
		{
			Utils.DrawLine(World.blockToTransformPos(new Vector3i(ProjectedPoints[i] - Origin.position)), World.blockToTransformPos(new Vector3i(ProjectedPoints[i + 1] - Origin.position)), Color.white, Color.cyan, 2);
		}
		for (int j = 0; j < Nodes.Count; j++)
		{
			RaycastNode raycastNode = Nodes[j];
			for (int k = 0; k < raycastNode.Neighbors.Count; k++)
			{
				RaycastNode raycastNode2 = raycastNode.Neighbors[k];
				for (int l = 0; l < raycastNode2.ChildSolidBlocks.Count; l++)
				{
					RaycastPathUtils.DrawNode(raycastNode2.ChildSolidBlocks[l], Color.red, 0f);
				}
				for (int m = 0; m < raycastNode2.ChildAirBlocks.Count; m++)
				{
					RaycastPathUtils.DrawNode(raycastNode2.ChildAirBlocks[m], Color.cyan, 0f);
				}
			}
			if (raycastNode.Children.Count < 1)
			{
				switch (raycastNode.nodeType)
				{
				case cPathNodeType.Air:
					RaycastPathUtils.DrawNode(raycastNode, Color.cyan, 0f);
					break;
				case cPathNodeType.Door:
					RaycastPathUtils.DrawNode(raycastNode, Color.green, 0f);
					break;
				}
			}
			else
			{
				for (int n = 0; n < raycastNode.ChildSolidBlocks.Count; n++)
				{
					RaycastPathUtils.DrawNode(raycastNode.ChildSolidBlocks[n], Color.red, 0f);
				}
				for (int num = 0; num < raycastNode.ChildAirBlocks.Count; num++)
				{
					RaycastPathUtils.DrawNode(raycastNode.ChildAirBlocks[num], Color.cyan, 0f);
				}
			}
		}
		for (int num2 = 0; num2 < Nodes.Count - 1; num2++)
		{
			RaycastNode raycastNode3 = Nodes[num2];
			Utils.DrawLine(endPos: Nodes[num2 + 1].Position - Origin.position, startPos: raycastNode3.Position - Origin.position, startColor: Color.white, endColor: Color.green, segments: 2);
		}
	}
}
