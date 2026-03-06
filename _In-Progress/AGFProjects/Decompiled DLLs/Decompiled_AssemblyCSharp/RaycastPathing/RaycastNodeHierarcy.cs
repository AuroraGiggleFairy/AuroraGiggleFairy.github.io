using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace RaycastPathing;

[Preserve]
public class RaycastNodeHierarcy
{
	public RaycastNode parent;

	public List<RaycastNode> neighbors = new List<RaycastNode>();

	public List<RaycastNode> children = new List<RaycastNode>();

	public List<RaycastNode> childAirBlocks = new List<RaycastNode>();

	public List<RaycastNode> childSolidBlocks = new List<RaycastNode>();

	public bool flowToWaypoint;

	public RaycastNode waypoint;

	public void SetWayPoint(RaycastNode node)
	{
		flowToWaypoint = true;
		waypoint = node;
	}

	public void SetParent(RaycastNode node)
	{
		parent = node;
	}

	public void AddNeighbor(RaycastNode node)
	{
		if (!neighbors.Contains(node))
		{
			neighbors.Add(node);
		}
	}

	public RaycastNode GetNeighbor(Vector3 pos)
	{
		for (int i = 0; i < neighbors.Count; i++)
		{
			if (neighbors[i].Center == pos)
			{
				return neighbors[i];
			}
		}
		return null;
	}

	public void AddChild(RaycastNode node)
	{
		if (children.Contains(node))
		{
			return;
		}
		children.Add(node);
		if (node.nodeType == cPathNodeType.Air)
		{
			if (!childAirBlocks.Contains(node))
			{
				childAirBlocks.Add(node);
			}
		}
		else if (!childSolidBlocks.Contains(node))
		{
			childSolidBlocks.Add(node);
		}
	}
}
