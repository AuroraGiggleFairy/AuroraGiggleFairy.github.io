using UnityEngine.Scripting;

[Preserve]
public class NetPackageChunkRemove : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public long chunkKey;

	public override int Channel => 1;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageChunkRemove Setup(long _chunkKey)
	{
		chunkKey = _chunkKey;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		chunkKey = _reader.ReadInt64();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(chunkKey);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null)
		{
			_callbacks.RemoveChunk(chunkKey);
		}
	}

	public override int GetLength()
	{
		return 4;
	}
}
