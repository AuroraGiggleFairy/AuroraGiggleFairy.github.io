using Unity.Burst;
using Unity.Collections;

namespace WorldGenerationEngineFinal;

[BurstCompile(CompileSynchronously = true)]
public struct PathNodePool
{
	[PublicizedFrom(EAccessModifier.Private)]
	public NativeList<PathNode> pool;

	[PublicizedFrom(EAccessModifier.Private)]
	public int free;

	public PathNodePool(int _size)
	{
		pool = new NativeList<PathNode>(_size, Allocator.Persistent);
		pool.AddReplicate(default(PathNode), _size);
		free = 0;
	}

	public void Init(int _size)
	{
		pool = new NativeList<PathNode>(_size, Allocator.Persistent);
		pool.AddReplicate(default(PathNode), _size);
		free = 0;
	}

	public int Alloc()
	{
		int length = pool.Length;
		if (free >= length)
		{
			pool.ResizeUninitialized(length + 10000);
		}
		return free++;
	}

	public void ReturnAll()
	{
		free = 0;
	}

	public void Node(int _index, out PathNode _node)
	{
		_node = pool[_index];
	}

	public ref PathNode Node(int _index)
	{
		return ref pool.ElementAt(_index);
	}

	public void Cleanup()
	{
		pool.Dispose();
	}

	public void LogStats()
	{
		Log.Out($"PathNodePool: Capacity={pool.Capacity}, Allocated={pool.Length}, Free={free}");
	}
}
