using UnityEngine.Scripting;

[Preserve]
public class NetPackageDecoResetWorldChunk : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public PooledExpandableMemoryStream ms = MemoryPools.poolMemoryStream.AllocSync(_bReset: true);

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageDecoResetWorldChunk Setup(long _chunkKey)
	{
		using PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false);
		pooledBinaryWriter.SetBaseStream(ms);
		pooledBinaryWriter.Write(_chunkKey);
		return this;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	~NetPackageDecoResetWorldChunk()
	{
		MemoryPools.poolMemoryStream.FreeSync(ms);
	}

	public override void read(PooledBinaryReader _br)
	{
		int length = _br.ReadInt32();
		StreamUtils.StreamCopy(_br.BaseStream, ms, length);
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write((int)ms.Length);
		ms.WriteTo(_bw.BaseStream);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		using PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
		lock (ms)
		{
			pooledBinaryReader.SetBaseStream(ms);
			ms.Position = 0L;
			long worldChunkKey = pooledBinaryReader.ReadInt64();
			DecoManager.Instance.ResetDecosForWorldChunk(worldChunkKey);
		}
	}

	public override int GetLength()
	{
		return (int)ms.Length;
	}
}
