using System.Collections.Generic;

public class ChunkBlockClearData : ChunkCustomData
{
	public List<Vector3i> BlockList = new List<Vector3i>();

	public World World;

	public ChunkBlockClearData()
	{
	}

	public ChunkBlockClearData(string _key, ulong _expiresInWorldTime, bool _isSavedToNetwork, World _world)
		: base(_key, _expiresInWorldTime, _isSavedToNetwork)
	{
		World = _world;
	}

	public override void OnRemove(Chunk chunk)
	{
		for (int num = BlockList.Count - 1; num >= 0; num--)
		{
			Vector3i vector3i = BlockList[num];
			chunk.SetBlock(World, vector3i.x, vector3i.y, vector3i.z, BlockValue.Air);
		}
	}
}
