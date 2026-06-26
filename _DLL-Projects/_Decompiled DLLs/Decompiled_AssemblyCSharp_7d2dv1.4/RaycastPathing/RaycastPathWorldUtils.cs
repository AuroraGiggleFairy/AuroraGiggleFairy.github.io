using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace RaycastPathing;

[Preserve]
public class RaycastPathWorldUtils
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3i[] mainBlockAxis = new Vector3i[6]
	{
		new Vector3i(0, 0, 1),
		new Vector3i(1, 0, 0),
		new Vector3i(0, 0, -1),
		new Vector3i(-1, 0, 0),
		new Vector3i(0, 1, 0),
		new Vector3i(0, -1, 0)
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3i[] diagonalBlockAxis = new Vector3i[20]
	{
		new Vector3i(0, 1, 1),
		new Vector3i(0, -1, 1),
		new Vector3i(1, 1, 0),
		new Vector3i(1, -1, 0),
		new Vector3i(0, 1, -1),
		new Vector3i(0, -1, -1),
		new Vector3i(-1, 1, 0),
		new Vector3i(-1, -1, 0),
		new Vector3i(1, 0, 1),
		new Vector3i(1, 1, 1),
		new Vector3i(1, -1, 1),
		new Vector3i(1, 0, -1),
		new Vector3i(1, 1, -1),
		new Vector3i(1, -1, -1),
		new Vector3i(-1, 0, 1),
		new Vector3i(-1, 1, 1),
		new Vector3i(-1, -1, 1),
		new Vector3i(-1, 0, -1),
		new Vector3i(-1, 1, -1),
		new Vector3i(-1, -1, -1)
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3[] subBlockOffsets = new Vector3[8]
	{
		new Vector3(1f, 1f, 1f),
		new Vector3(1f, 1f, 0f),
		new Vector3(0f, 1f, 1f),
		new Vector3(0f, 1f, 0f),
		new Vector3(1f, 0f, 1f),
		new Vector3(1f, 0f, 0f),
		new Vector3(0f, 0f, 1f),
		new Vector3(0f, 0f, 0f)
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3[] quarterBlockOffsets = new Vector3[8]
	{
		new Vector3(0.25f, 0.25f, 0.25f),
		new Vector3(-0.25f, 0.25f, 0.25f),
		new Vector3(0.25f, 0.25f, -0.25f),
		new Vector3(-0.25f, 0.25f, -0.25f),
		new Vector3(0.25f, -0.25f, 0.25f),
		new Vector3(-0.25f, -0.25f, 0.25f),
		new Vector3(0.25f, -0.25f, -0.25f),
		new Vector3(-0.25f, -0.25f, -0.25f)
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3 underGroundTestOffset = new Vector3(0f, 255f, 0f);

	public const int cPathLayer = 1073807360;

	public static Vector3i GetBlockPosition(Vector3 worldPos)
	{
		Vector3i vector3i = new Vector3i(worldPos);
		Vector3 v = worldPos - vector3i.ToVector3Center();
		return vector3i + Vector3i.FromVector3Rounded(v);
	}

	public static Vector3 GetCenterPosition(Vector3 worldPos, float scale = 1f)
	{
		return worldPos + Vector3.one * 0.5f * scale;
	}

	public static bool IsConfinedSpace(World world, Vector3 point, float dist, bool debugDraw = false)
	{
		int num = 0;
		List<RaycastNode> list = ScanBlocksAround(world, point, dist, debugDraw);
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].nodeType == cPathNodeType.Air)
			{
				num++;
			}
		}
		if ((float)num < dist * dist)
		{
			return true;
		}
		return false;
	}

	public static bool IsUnderground(Vector3 target)
	{
		return Physics.Raycast(new Ray(target + underGroundTestOffset - Origin.position, Vector3.down), underGroundTestOffset.y, 268500992);
	}

	public static bool isPosUnderWater(World world, Vector3 pos)
	{
		Vector3i blockPosition = GetBlockPosition(pos);
		return world.GetBlock(blockPosition).type == 240;
	}

	public static cPathNodeType getBlockType(World world, RaycastNode current, Vector3 adjacentCenter)
	{
		cPathNodeType cPathNodeType2 = cPathNodeType.Unassigned;
		Vector3i pos = World.worldToBlockPos(adjacentCenter);
		BlockValue block = world.GetBlock(pos);
		if (block.Block.shape.IsSolidSpace || block.Block.shape.IsSolidCube)
		{
			return cPathNodeType.Solid;
		}
		if (block.isair)
		{
			return cPathNodeType.Air;
		}
		if (block.Block.HasTag(BlockTags.Door))
		{
			return cPathNodeType.Door;
		}
		if (HasSubBlocks(world, current, adjacentCenter))
		{
			return cPathNodeType.Half;
		}
		return cPathNodeType.Solid;
	}

	public static cPathNodeType getBlockType(World world, Vector3i pos)
	{
		cPathNodeType cPathNodeType2 = cPathNodeType.Unassigned;
		BlockValue block = world.GetBlock(pos);
		if (block.isair)
		{
			return cPathNodeType.Air;
		}
		if (block.Block.HasTag(BlockTags.Door))
		{
			return cPathNodeType.Door;
		}
		return cPathNodeType.Solid;
	}

	public static List<RaycastNode> ScanBlocksAround(World world, Vector3 point, float dist, bool debugDraw = false)
	{
		List<RaycastNode> list = new List<RaycastNode>();
		List<RaycastNode> list2 = new List<RaycastNode>();
		RaycastNode raycastNode = new RaycastNode(GetBlockPosition(point));
		list.Add(raycastNode);
		if (debugDraw)
		{
			RaycastPathUtils.DrawBounds(raycastNode.Center, Color.yellow, 0.95f);
		}
		while (list.Count > 0)
		{
			RaycastNode raycastNode2 = list[0];
			list.RemoveAt(0);
			list2.Add(raycastNode2);
			for (int i = 0; i < mainBlockAxis.Length; i++)
			{
				Vector3 pos = raycastNode2.Center + mainBlockAxis[i];
				float magnitude = (raycastNode.Center - pos).magnitude;
				if (list.Find([PublicizedFrom(EAccessModifier.Internal)] (RaycastNode n) => n.Center == pos) != null || list2.Find([PublicizedFrom(EAccessModifier.Internal)] (RaycastNode n) => n.Center == pos) != null || !(magnitude < dist * 0.5f))
				{
					continue;
				}
				RaycastNode raycastNode3 = new RaycastNode(pos);
				raycastNode3.SetParent(raycastNode2);
				cPathNodeType blockType = getBlockType(world, raycastNode3.BlockPos);
				raycastNode3.SetType(blockType);
				list.Add(raycastNode3);
				if (debugDraw)
				{
					switch (blockType)
					{
					case cPathNodeType.Air:
						RaycastPathUtils.DrawBounds(raycastNode3.Center, Color.cyan, 1f, 0.95f);
						break;
					case cPathNodeType.Solid:
						RaycastPathUtils.DrawBounds(raycastNode3.Center, Color.red, 1f, 0.95f);
						break;
					}
				}
			}
		}
		return list2;
	}

	public static List<RaycastNode> ScanVolume(World world, Vector3 pos, bool useTarget = false, bool useDiagnols = false, bool debugDraw = false, float duration = 0f)
	{
		List<RaycastNode> list = new List<RaycastNode>();
		RaycastNode raycastNode = new RaycastNode(pos);
		if (useTarget)
		{
			if (world.GetBlock(raycastNode.BlockPos).isair)
			{
				raycastNode.SetType(cPathNodeType.Air);
			}
			list.Add(raycastNode);
		}
		for (int i = 0; i < mainBlockAxis.Length; i++)
		{
			RaycastNode item = InitNeighborNode(world, raycastNode, raycastNode.BlockPos + mainBlockAxis[i], debugDraw, duration);
			list.Add(item);
		}
		if (useDiagnols)
		{
			for (int j = 0; j < diagonalBlockAxis.Length; j++)
			{
				Vector3i adjacentPos = raycastNode.BlockPos + diagonalBlockAxis[j];
				RaycastNode item2 = InitNeighborNode(world, raycastNode, adjacentPos, debugDraw, duration);
				list.Add(item2);
			}
		}
		if (debugDraw)
		{
			for (int k = 0; k < list.Count; k++)
			{
				RaycastPathUtils.DrawNode(list[k], Color.yellow, duration);
			}
		}
		return list;
	}

	public static RaycastNode FindNodeType(List<RaycastNode> nodes, cPathNodeType targetType = cPathNodeType.Air)
	{
		RaycastNode result = null;
		for (int i = 0; i < nodes.Count; i++)
		{
			if (nodes[i].nodeType == targetType)
			{
				return nodes[i];
			}
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static RaycastNode InitNeighborNode(World world, RaycastNode current, Vector3i adjacentPos, bool debugDraw = false, float duration = 0f)
	{
		Vector3 vector = World.blockToTransformPos(adjacentPos) + Vector3.up * 0.5f;
		cPathNodeType blockType = getBlockType(world, current, vector);
		RaycastNode raycastNode = new RaycastNode(vector);
		raycastNode.SetType(blockType);
		raycastNode.SetParent(current);
		current.AddNeighbor(raycastNode);
		if (blockType == cPathNodeType.Half)
		{
			List<RaycastNode> list = ScanChildBlocksAround(world, current, vector, debugDraw, duration);
			for (int i = 0; i < list.Count; i++)
			{
				RaycastNode raycastNode2 = list[i];
				raycastNode2.SetParent(raycastNode);
				raycastNode.AddChild(raycastNode2);
			}
		}
		return raycastNode;
	}

	public static float getH(Vector3 from, Vector3 to)
	{
		return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y) + Mathf.Abs(from.z - to.z);
	}

	public static List<FloodFillNode> ScanNeighborNodes(World world, FloodFillPath path, FloodFillNode current, bool useDiagnols = false)
	{
		List<FloodFillNode> list = new List<FloodFillNode>();
		for (int i = 0; i < mainBlockAxis.Length; i++)
		{
			FloodFillNode item = BuildNeighborNode(world, path, current, current.BlockPos + mainBlockAxis[i]);
			list.Add(item);
		}
		if (useDiagnols)
		{
			for (int j = 0; j < diagonalBlockAxis.Length; j++)
			{
				Vector3 vector = current.Center + diagonalBlockAxis[j];
				Vector3 normalized = (current.Center - vector).normalized;
				Vector3i adjacentPos = current.BlockPos + diagonalBlockAxis[j];
				if (!RaycastPathUtils.IsPointBlocked(current.Center, current.Center + normalized * 1.5f, 65536) && !RaycastPathUtils.IsPointBlocked(current.Center, current.Center - normalized * 1.5f, 65536))
				{
					FloodFillNode item2 = BuildNeighborNode(world, path, current, adjacentPos);
					list.Add(item2);
				}
			}
		}
		return list;
	}

	public static FloodFillNode BuildNeighborNode(World world, FloodFillPath path, FloodFillNode current, Vector3i adjacentPos, bool debugDraw = false)
	{
		Vector3 vector = World.blockToTransformPos(adjacentPos) + Vector3.up * 0.5f;
		cPathNodeType blockType = getBlockType(world, current, vector);
		FloodFillNode floodFillNode = new FloodFillNode(vector);
		floodFillNode.SetType(blockType);
		floodFillNode.SetParent(current);
		current.AddNeighbor(floodFillNode);
		if (blockType == cPathNodeType.Half)
		{
			List<RaycastNode> list = ScanChildBlocksAround(world, current, vector, debugDraw);
			for (int i = 0; i < list.Count; i++)
			{
				RaycastNode raycastNode = list[i];
				raycastNode.SetParent(current);
				floodFillNode.AddChild(raycastNode);
			}
		}
		floodFillNode.G = current.G + 1f;
		floodFillNode.Heuristic = getH(floodFillNode.BlockPos, path.TargetBlockPos);
		return floodFillNode;
	}

	public static bool HasSubBlocks(World world, RaycastNode current, Vector3 spacialCenter, bool debugDraw = false, float duration = 0f)
	{
		int num = 0;
		List<RaycastNode> list = ScanChildBlocksAround(world, current, spacialCenter, debugDraw, duration);
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].nodeType == cPathNodeType.Air)
			{
				num++;
			}
		}
		return num > 0;
	}

	public static List<RaycastNode> ScanChildBlocksAround(World world, RaycastNode current, Vector3 spacialCenter, bool debugDraw = false, float duration = 0f)
	{
		List<RaycastNode> list = new List<RaycastNode>();
		for (int i = 0; i < quarterBlockOffsets.Length; i++)
		{
			RaycastNode item = new RaycastNode(spacialCenter + quarterBlockOffsets[i], 0.5f, current.Depth + 1);
			list.Add(item);
		}
		for (int j = 0; j < list.Count; j++)
		{
			RaycastNode raycastNode = list[j];
			Vector3 position = raycastNode.Position;
			if (!RaycastPathUtils.IsPointBlocked(spacialCenter, position, 65536, debugDraw, duration) && !RaycastPathUtils.IsPointBlocked(current.Position, spacialCenter, 65536, debugDraw, duration))
			{
				raycastNode.SetType(cPathNodeType.Air);
			}
			else
			{
				raycastNode.SetType(cPathNodeType.Solid);
			}
			if (debugDraw)
			{
				if (raycastNode.nodeType == cPathNodeType.Air)
				{
					RaycastPathUtils.DrawNode(raycastNode, Color.cyan, duration);
				}
				else if (raycastNode.nodeType == cPathNodeType.Solid)
				{
					RaycastPathUtils.DrawNode(raycastNode, Color.magenta, duration);
				}
			}
		}
		list.Sort([PublicizedFrom(EAccessModifier.Internal)] (RaycastNode x, RaycastNode y) => (!(Vector3.Distance(current.Position, x.Position) < Vector3.Distance(current.Position, y.Position))) ? 1 : (-1));
		return list;
	}

	public static List<RaycastNode> ScanPath(World world, Vector3 fromPos, List<Vector3> path, bool useDiagnols, bool debugDraw = false, float duration = 0f)
	{
		List<RaycastNode> list = new List<RaycastNode>();
		Vector3 fromPos2 = fromPos;
		for (int i = 0; i < path.Count; i++)
		{
			list.Add(ScanPoint(world, path[i], fromPos2, useDiagnols, debugDraw, duration));
			fromPos2 = path[i];
		}
		return list;
	}

	public static RaycastNode ScanPoint(World world, Vector3 worldPos, Vector3 fromPos, bool useDiagnols, bool debugDraw = false, float duration = 0f)
	{
		RaycastNode raycastNode = CreateNode(world, World.worldToBlockPos(worldPos).ToVector3Center());
		for (int i = 0; i < mainBlockAxis.Length; i++)
		{
			CreateNode(world, raycastNode.BlockPos.ToVector3Center() + mainBlockAxis[i], raycastNode);
		}
		if (useDiagnols)
		{
			for (int j = 0; j < diagonalBlockAxis.Length; j++)
			{
				CreateNode(world, raycastNode.BlockPos.ToVector3Center() + diagonalBlockAxis[j], raycastNode);
			}
		}
		if (debugDraw)
		{
			if (raycastNode.nodeType == cPathNodeType.Solid)
			{
				RaycastPathUtils.DrawNode(raycastNode, Color.magenta, duration);
			}
			if (raycastNode.nodeType == cPathNodeType.Air)
			{
				RaycastPathUtils.DrawNode(raycastNode, Color.cyan, duration);
			}
			if (raycastNode.nodeType == cPathNodeType.Door)
			{
				RaycastPathUtils.DrawNode(raycastNode, Color.blue, duration);
			}
		}
		if (raycastNode.nodeType == cPathNodeType.Half)
		{
			RaycastNode waypoint = ProcNeighborNodes(raycastNode.Neighbors, raycastNode);
			raycastNode.SetWaypoint(waypoint);
		}
		return raycastNode;
	}

	public static cPathNodeType GetBlockType(World world, Vector3 toPos)
	{
		Vector3i pos = World.worldToBlockPos(toPos);
		BlockValue block = world.GetBlock(pos);
		Vector3 worldPos = pos.ToVector3Center();
		if (block.isair)
		{
			return cPathNodeType.Air;
		}
		if (block.Block.shape.IsSolidSpace || block.Block.shape.IsSolidCube)
		{
			return cPathNodeType.Solid;
		}
		if (block.Block.HasTag(BlockTags.Door))
		{
			return cPathNodeType.Door;
		}
		if (HasChildNodes(world, worldPos))
		{
			return cPathNodeType.Half;
		}
		return cPathNodeType.Air;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static RaycastNode CreateNode(World world, Vector3 worldPos, RaycastNode parent = null, bool debugDraw = false, float duration = 0f)
	{
		RaycastNode raycastNode = new RaycastNode(worldPos);
		cPathNodeType blockType = GetBlockType(world, worldPos);
		raycastNode.SetType(blockType);
		if (parent != null)
		{
			raycastNode.SetParent(parent);
			parent.AddNeighbor(raycastNode);
		}
		if (blockType == cPathNodeType.Half)
		{
			List<RaycastNode> list = CreateChildNodes(world, worldPos, debugDraw, duration);
			for (int i = 0; i < list.Count; i++)
			{
				RaycastNode raycastNode2 = list[i];
				raycastNode2.SetParent(raycastNode);
				raycastNode.AddChild(raycastNode2);
				if (debugDraw)
				{
					if (raycastNode2.nodeType == cPathNodeType.Air)
					{
						RaycastPathUtils.DrawNode(raycastNode2, Color.cyan, duration);
					}
					else if (raycastNode2.nodeType == cPathNodeType.Solid)
					{
						RaycastPathUtils.DrawNode(raycastNode2, Color.magenta, duration);
					}
				}
			}
		}
		return raycastNode;
	}

	public static List<RaycastNode> CreateChildNodes(World world, Vector3 worldPos, bool debugDraw = false, float duration = 0f)
	{
		List<RaycastNode> list = new List<RaycastNode>();
		Vector3 vector = World.worldToBlockPos(worldPos).ToVector3Center();
		for (int i = 0; i < quarterBlockOffsets.Length; i++)
		{
			RaycastNode item = new RaycastNode(vector + quarterBlockOffsets[i], 0.5f);
			list.Add(item);
		}
		for (int j = 0; j < list.Count; j++)
		{
			RaycastNode raycastNode = list[j];
			if (!RaycastPathUtils.IsPointBlocked(vector, raycastNode.Position, 1073807360, debugDraw, duration))
			{
				raycastNode.SetType(cPathNodeType.Air);
			}
			else
			{
				raycastNode.SetType(cPathNodeType.Solid);
			}
		}
		return list;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static RaycastNode ProcNeighborNodes(List<RaycastNode> currentNeighbors, RaycastNode current)
	{
		List<RaycastNode> list = new List<RaycastNode>();
		for (int i = 0; i < currentNeighbors.Count; i++)
		{
			RaycastNode raycastNode = currentNeighbors[i];
			if (raycastNode.ChildAirBlocks.Count > 0)
			{
				for (int j = 0; j < raycastNode.ChildAirBlocks.Count; j++)
				{
					RaycastNode item = raycastNode.Children[j];
					list.Add(item);
				}
			}
		}
		List<RaycastNode> list2 = new List<RaycastNode>();
		for (int k = 0; k < current.ChildAirBlocks.Count; k++)
		{
			RaycastNode raycastNode2 = current.ChildAirBlocks[k];
			list2.Add(raycastNode2);
			for (int l = 0; l < list.Count; l++)
			{
				RaycastNode raycastNode3 = list[l];
				if (!RaycastPathUtils.IsPointBlocked(raycastNode2.Position, raycastNode3.Position, 65536, debugDraw: true))
				{
					list2.Add(raycastNode3);
				}
			}
		}
		Vector3 vector = Vector3.zero;
		Vector3 vector2 = Vector3.zero;
		for (int m = 0; m < list2.Count - 1; m++)
		{
			RaycastNode raycastNode4 = list2[m];
			RaycastNode raycastNode5 = list2[m + 1];
			if (m < 1)
			{
				vector = Vector3.Min(raycastNode4.Min, raycastNode5.Min);
				vector2 = Vector3.Max(raycastNode4.Max, raycastNode5.Max);
			}
			else
			{
				vector = Vector3.Min(vector, raycastNode5.Min);
				vector2 = Vector3.Max(vector2, raycastNode5.Max);
			}
		}
		RaycastNode raycastNode6 = new RaycastNode(vector, vector2);
		RaycastPathUtils.DrawNode(raycastNode6, Color.green, 5f);
		current.SetWaypoint(raycastNode6);
		return raycastNode6;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int GetChildAirblockCount(World world, Vector3 worldPos, bool debugDraw = false, float duration = 0f)
	{
		int num = 0;
		List<RaycastNode> list = CreateChildNodes(world, worldPos, debugDraw, duration);
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].nodeType == cPathNodeType.Air)
			{
				num++;
			}
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool HasChildNodes(World world, Vector3 worldPos, bool debugDraw = false, float duration = 0f)
	{
		int childAirblockCount = GetChildAirblockCount(world, worldPos, debugDraw, duration);
		if (childAirblockCount > 0)
		{
			return childAirblockCount < quarterBlockOffsets.Length;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool AreChildrenAir(World world, Vector3 worldPos, bool debugDraw = false, float duration = 0f)
	{
		return GetChildAirblockCount(world, worldPos, debugDraw, duration) == quarterBlockOffsets.Length;
	}
}
