using UnityEngine.Scripting;

[Preserve]
public class NetPackageChunkRemoveAll : NetPackage
{
	public override int Channel => 0;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public override void read(PooledBinaryReader _reader)
	{
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null)
		{
			GameManager.Instance.World.m_ChunkManager.RemoveAllChunks();
		}
	}

	public override int GetLength()
	{
		return 4;
	}
}
