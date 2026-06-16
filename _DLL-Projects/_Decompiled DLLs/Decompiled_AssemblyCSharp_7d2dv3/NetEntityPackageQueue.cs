using System.Collections.Generic;

public class NetEntityPackageQueue
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly World world;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<int, Queue<NetPackage>> entityPackageQueues = new Dictionary<int, Queue<NetPackage>>(64);

	public NetEntityPackageQueue(World world)
	{
		this.world = world;
	}

	public bool HasPackagesForEntity(int entityId)
	{
		return entityPackageQueues.ContainsKey(entityId);
	}

	public void EnqueueNetPackageForEntity(int entityId, NetPackage netPackage)
	{
		if (!entityPackageQueues.TryGetValue(entityId, out var value))
		{
			value = new Queue<NetPackage>(10);
			entityPackageQueues.Add(entityId, value);
		}
		value.Enqueue(netPackage);
	}

	public void ProcessPackagesForEntity(int entityId)
	{
		if (entityPackageQueues.Remove(entityId, out var value))
		{
			NetPackage result;
			while (value.TryDequeue(out result))
			{
				result.ProcessPackage(world, GameManager.Instance);
				NetPackageManager.FreePackage(result);
			}
		}
	}

	public void Cleanup()
	{
		foreach (Queue<NetPackage> value in entityPackageQueues.Values)
		{
			NetPackage result;
			while (value.TryDequeue(out result))
			{
				NetPackageManager.FreePackage(result);
			}
		}
		entityPackageQueues.Clear();
	}
}
