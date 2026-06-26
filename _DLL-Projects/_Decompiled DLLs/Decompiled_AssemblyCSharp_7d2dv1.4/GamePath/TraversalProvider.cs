using Pathfinding;

namespace GamePath;

[PublicizedFrom(EAccessModifier.Internal)]
public class TraversalProvider : ITraversalProvider
{
	public bool CanTraverse(Path path, GraphNode node)
	{
		if (node.Walkable)
		{
			return ((path.enabledTags >> (int)node.Tag) & 1) != 0;
		}
		return false;
	}

	public uint GetTraversalCost(Path path, GraphNode node)
	{
		return (uint)((float)node.Penalty * path.CostScale);
	}
}
