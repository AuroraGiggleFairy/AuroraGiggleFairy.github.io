using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace RaycastPathing;

[Preserve]
public class RaycastNode
{
	[PublicizedFrom(EAccessModifier.Private)]
	public RaycastNodeInfo info;

	[PublicizedFrom(EAccessModifier.Private)]
	public RaycastNodeHierarcy hierarchy;

	public cPathNodeType nodeType;

	public Vector3 Position => info.Position;

	public Vector3 Center => info.Center;

	public Vector3i BlockPos => info.BlockPos;

	public float Scale => info.Scale;

	public int Depth => info.Depth;

	public Vector3 Min => info.Min;

	public Vector3 Max => info.Max;

	public RaycastNode Parent => hierarchy.parent;

	public List<RaycastNode> Neighbors => hierarchy.neighbors;

	public List<RaycastNode> Children => hierarchy.children;

	public List<RaycastNode> ChildAirBlocks => hierarchy.childAirBlocks;

	public List<RaycastNode> ChildSolidBlocks => hierarchy.childSolidBlocks;

	public RaycastNode Waypoint => hierarchy.waypoint;

	public bool FlowToWaypoint => hierarchy.flowToWaypoint;

	public RaycastNode(Vector3 pos, float scale = 1f, int depth = 0)
	{
		info = new RaycastNodeInfo(pos, scale, depth);
		hierarchy = new RaycastNodeHierarcy();
	}

	public RaycastNode(Vector3 min, Vector3 max, float scale = 1f, int depth = 0)
	{
		info = new RaycastNodeInfo(min, max, scale, depth);
		hierarchy = new RaycastNodeHierarcy();
	}

	public void SetParent(RaycastNode node)
	{
		hierarchy.parent = node;
	}

	public void AddNeighbor(RaycastNode node)
	{
		hierarchy.neighbors.Add(node);
	}

	public RaycastNode GetNeighbor(Vector3 pos)
	{
		return hierarchy.GetNeighbor(pos);
	}

	public void AddChild(RaycastNode node)
	{
		hierarchy.AddChild(node);
	}

	public void SetWaypoint(RaycastNode node)
	{
		hierarchy.SetWayPoint(node);
	}

	public void SetType(cPathNodeType _nodeType)
	{
		nodeType = _nodeType;
	}

	public virtual void DebugDraw()
	{
	}
}
