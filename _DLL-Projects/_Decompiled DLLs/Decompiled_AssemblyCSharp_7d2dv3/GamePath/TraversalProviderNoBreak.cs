using Pathfinding;

namespace GamePath;

[PublicizedFrom(EAccessModifier.Internal)]
public class TraversalProviderNoBreak : ITraversalProvider
{
	public bool CanTraverseConnection(Path path, Connection conn)
	{
		if (!conn.node.Walkable)
		{
			return false;
		}
		if (conn.tag == 3)
		{
			if (conn.payload is TEFeatureDoor tEFeatureDoor)
			{
				if (tEFeatureDoor.IsOpen())
				{
					return true;
				}
				if (((path.enabledTags >> 3) & 1) == 0)
				{
					return false;
				}
				if (!tEFeatureDoor.CanOpen(out var _))
				{
					return false;
				}
			}
			return true;
		}
		return ((path.enabledTags >> (int)conn.node.Tag) & 1) != 0;
	}

	public bool CanTraverse(Path path, GraphNode node, int gridDirection)
	{
		if (!node.Walkable)
		{
			return false;
		}
		if (!path.CanBreakBlocks && gridDirection >= 0 && node is AstarVoxelGrid.VoxelNode voxelNode && (voxelNode.BlockerFlags & (17 << gridDirection)) > 0)
		{
			return false;
		}
		if (((path.enabledTags >> (int)node.Tag) & 1) != 0)
		{
			return node.Penalty < 1000;
		}
		return false;
	}

	public uint GetTraversalCost(Path path, GraphNode node)
	{
		return node.Penalty;
	}
}
