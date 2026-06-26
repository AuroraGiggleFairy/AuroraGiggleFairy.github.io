using System.Collections.Generic;

namespace WorldGenerationEngineFinal;

public class PathNodePool
{
	[PublicizedFrom(EAccessModifier.Private)]
	public List<PathNode> pool;

	[PublicizedFrom(EAccessModifier.Private)]
	public int used;

	public PathNodePool(int _initialSize)
	{
		pool = new List<PathNode>(_initialSize);
	}

	public PathNode Alloc()
	{
		PathNode pathNode;
		if (used >= pool.Count)
		{
			pathNode = new PathNode();
			pool.Add(pathNode);
		}
		else
		{
			pathNode = pool[used];
		}
		used++;
		return pathNode;
	}

	public void ReturnAll()
	{
		for (int i = 0; i < used; i++)
		{
			pool[i].Reset();
		}
		used = 0;
	}

	public void Cleanup()
	{
		ReturnAll();
		pool.Clear();
		pool.Capacity = 16;
	}

	public void LogStats()
	{
		Log.Out($"PathNodePool: Capacity={pool.Capacity}, Allocated={pool.Count}, InUse={used}");
	}
}
