using System.IO;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageChunk : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Chunk chunk;

	[PublicizedFrom(EAccessModifier.Private)]
	public PooledMemoryStream serializedData;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bOverwriteExisting;

	[PublicizedFrom(EAccessModifier.Private)]
	public int dataLen;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] data;

	[PublicizedFrom(EAccessModifier.Private)]
	public int chunkX;

	[PublicizedFrom(EAccessModifier.Private)]
	public int chunkY;

	[PublicizedFrom(EAccessModifier.Private)]
	public int chunkZ;

	public override int Channel => 1;

	public override bool Compress => true;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	[PublicizedFrom(EAccessModifier.Protected)]
	~NetPackageChunk()
	{
		if (chunk != null)
		{
			chunk.InProgressNetworking = false;
			chunk = null;
		}
		if (serializedData != null)
		{
			MemoryPools.poolMS.FreeSync(serializedData);
			serializedData = null;
		}
	}

	public NetPackageChunk Setup(Chunk _chunk, bool _bOverwriteExisting = false)
	{
		chunk = _chunk;
		bOverwriteExisting = _bOverwriteExisting;
		serializedData = MemoryPools.poolMS.AllocSync(_bReset: true);
		using PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false);
		pooledBinaryWriter.SetBaseStream(serializedData);
		chunk.write(pooledBinaryWriter);
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		_reader.ReadByte();
		bOverwriteExisting = _reader.ReadBoolean();
		if (bOverwriteExisting)
		{
			chunkX = _reader.ReadInt16();
			chunkY = _reader.ReadInt16();
			chunkZ = _reader.ReadInt16();
		}
		dataLen = _reader.ReadInt32();
		data = _reader.ReadBytes(dataLen);
		if (!bOverwriteExisting)
		{
			if (chunk == null)
			{
				chunk = MemoryPools.PoolChunks.AllocSync(_bReset: true);
			}
			using PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
			pooledBinaryReader.SetBaseStream(new MemoryStream(data));
			chunk.read(pooledBinaryReader, uint.MaxValue);
		}
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write((byte)0);
		_writer.Write(bOverwriteExisting);
		if (bOverwriteExisting)
		{
			_writer.Write((short)chunk.X);
			_writer.Write((short)chunk.Y);
			_writer.Write((short)chunk.Z);
		}
		_writer.Write((int)serializedData.Length);
		serializedData.Position = 0L;
		StreamUtils.StreamCopy(serializedData, _writer.BaseStream);
		MemoryPools.poolMS.FreeSync(serializedData);
		serializedData = null;
		chunk = null;
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			Log.Warning("Received chunk while world is not set up");
			if (chunk != null)
			{
				MemoryPools.PoolChunks.FreeSync(chunk);
				chunk = null;
			}
			return;
		}
		long key = ((!bOverwriteExisting) ? chunk.Key : WorldChunkCache.MakeChunkKey(chunkX, chunkZ));
		Chunk chunkSync;
		if ((chunkSync = _world.ChunkCache.GetChunkSync(key)) != null && !bOverwriteExisting)
		{
			Log.Error(GetType().Name + ": chunk already loaded " + chunk);
			return;
		}
		if (bOverwriteExisting)
		{
			Bounds bounds = Chunk.CalculateAABB(chunkX, chunkY, chunkZ);
			MultiBlockManager.Instance.DeregisterTrackedBlockDatas(bounds);
		}
		if (!bOverwriteExisting)
		{
			_world.ChunkCache.AddChunkSync(chunk);
			chunk.NeedsRegeneration = true;
			chunk = null;
		}
		else if (chunkSync != null)
		{
			using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: false))
			{
				pooledBinaryReader.SetBaseStream(new MemoryStream(data));
				chunkSync.OnUnload(_world);
				_world.ChunkCache.RemoveChunkSync(chunkSync.Key);
				chunkSync.Reset();
				chunkSync.read(pooledBinaryReader, uint.MaxValue);
				_world.ChunkCache.AddChunkSync(chunkSync);
				data = null;
			}
		}
	}

	public override int GetLength()
	{
		return 14 + (int)serializedData.Length;
	}
}
