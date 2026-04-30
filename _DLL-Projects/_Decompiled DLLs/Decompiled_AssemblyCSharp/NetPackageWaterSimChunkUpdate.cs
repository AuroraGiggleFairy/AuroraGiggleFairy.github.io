using UnityEngine.Scripting;

[Preserve]
public class NetPackageWaterSimChunkUpdate : NetPackage, IMemoryPoolableObject
{
	[PublicizedFrom(EAccessModifier.Private)]
	public PooledExpandableMemoryStream ms;

	[PublicizedFrom(EAccessModifier.Private)]
	public PooledBinaryWriter sendWriter;

	[PublicizedFrom(EAccessModifier.Private)]
	public long lengthStreamPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public int numVoxelUpdates;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] sendBytes;

	[PublicizedFrom(EAccessModifier.Private)]
	public int sendLength;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public static int GetPoolSize()
	{
		return 200;
	}

	public void SetupForSend(Chunk chunk)
	{
		ms = MemoryPools.poolMemoryStream.AllocSync(_bReset: true);
		sendWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: true);
		ms.Position = 0L;
		sendWriter.SetBaseStream(ms);
		sendWriter.Write(chunk.X);
		sendWriter.Write(chunk.Z);
		lengthStreamPos = ms.Position;
		sendWriter.Write(0);
	}

	public void AddChange(ushort _voxelIndex, WaterValue _newValue)
	{
		sendWriter.Write(_voxelIndex);
		_newValue.Write(sendWriter);
		numVoxelUpdates++;
	}

	public void FinalizeSend()
	{
		ms.Position = lengthStreamPos;
		sendWriter.Write(numVoxelUpdates);
		sendLength = (int)ms.Length;
		sendBytes = MemoryPools.poolByte.Alloc(sendLength);
		ms.Position = 0L;
		ms.Read(sendBytes, 0, sendLength);
		MemoryPools.poolBinaryWriter.FreeSync(sendWriter);
		sendWriter = null;
		MemoryPools.poolMemoryStream.FreeSync(ms);
		ms = null;
	}

	public override void read(PooledBinaryReader _br)
	{
		ms = MemoryPools.poolMemoryStream.AllocSync(_bReset: true);
		ms.Position = 0L;
		int length = _br.ReadInt32();
		StreamUtils.StreamCopy(_br.BaseStream, ms, length);
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(sendLength);
		_bw.Write(sendBytes, 0, sendLength);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		using PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: true);
		ms.Position = 0L;
		pooledBinaryReader.SetBaseStream(ms);
		int x = pooledBinaryReader.ReadInt32();
		int y = pooledBinaryReader.ReadInt32();
		long chunkKey = WorldChunkCache.MakeChunkKey(x, y);
		WaterSimulationNative.Instance.changeApplier.GetChangeWriter(chunkKey);
		using WaterSimulationApplyChanges.ChangesForChunk.Writer writer = WaterSimulationNative.Instance.changeApplier.GetChangeWriter(chunkKey);
		int num = pooledBinaryReader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			int voxelIndex = pooledBinaryReader.ReadUInt16();
			WaterValue waterValue = WaterValue.FromStream(pooledBinaryReader);
			writer.RecordChange(voxelIndex, waterValue);
		}
	}

	public override int GetLength()
	{
		return sendLength + 4;
	}

	public void Reset()
	{
		if (sendWriter != null)
		{
			MemoryPools.poolBinaryWriter.FreeSync(sendWriter);
			sendWriter = null;
		}
		if (ms != null)
		{
			MemoryPools.poolMemoryStream.FreeSync(ms);
			ms = null;
		}
		if (sendBytes != null)
		{
			MemoryPools.poolByte.Free(sendBytes);
			sendBytes = null;
		}
		lengthStreamPos = 0L;
		numVoxelUpdates = 0;
		sendLength = 0;
	}

	public void Cleanup()
	{
		Reset();
	}
}
