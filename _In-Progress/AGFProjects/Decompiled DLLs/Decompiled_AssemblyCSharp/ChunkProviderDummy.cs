using System.Collections;

public class ChunkProviderDummy : ChunkProviderAbstract
{
	[PublicizedFrom(EAccessModifier.Private)]
	public SpawnPointList dummySpawnPointList = new SpawnPointList();

	public override IEnumerator Init(World _worldData)
	{
		MultiBlockManager.Instance.Initialize(null);
		yield return null;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		MultiBlockManager.Instance.Cleanup();
	}

	public override EnumChunkProviderId GetProviderId()
	{
		return EnumChunkProviderId.NetworkClient;
	}

	public virtual void setChunkPrerequisits(string sSeed)
	{
	}

	public override void UnloadChunk(Chunk _c)
	{
		MemoryPools.PoolChunks.FreeSync(_c);
	}

	public override SpawnPointList GetSpawnPointList()
	{
		return dummySpawnPointList;
	}

	public override void SetSpawnPointList(SpawnPointList _spawnPointList)
	{
		dummySpawnPointList = _spawnPointList;
	}
}
