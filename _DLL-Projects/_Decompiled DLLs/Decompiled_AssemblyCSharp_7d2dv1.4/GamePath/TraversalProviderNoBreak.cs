using Pathfinding;

namespace GamePath;

[PublicizedFrom(EAccessModifier.Internal)]
public class TraversalProviderNoBreak : ITraversalProvider
{
	public bool CanTraverse(Path path, GraphNode node)
	{
		if (node.Walkable && ((path.enabledTags >> (int)node.Tag) & 1) != 0)
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
