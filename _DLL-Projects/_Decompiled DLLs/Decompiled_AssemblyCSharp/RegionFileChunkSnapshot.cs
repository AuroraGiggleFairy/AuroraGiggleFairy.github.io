using System;

public class RegionFileChunkSnapshot : IRegionFileChunkSnapshot, IMemoryPoolableObject
{
	public const string EXT = "7rg";

	[PublicizedFrom(EAccessModifier.Private)]
	public PooledMemoryStream stream;

	public long Size => stream?.Length ?? 0;

	public void Update(Chunk chunk, bool saveIfUnchanged)
	{
		if (!saveIfUnchanged && !chunk.NeedsSaving)
		{
			return;
		}
		if (stream != null)
		{
			stream.SetLength(0L);
		}
		else
		{
			stream = MemoryPools.poolMS.AllocSync(_bReset: true);
		}
		try
		{
			using PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false);
			pooledBinaryWriter.SetBaseStream(stream);
			pooledBinaryWriter.Write((byte)116);
			pooledBinaryWriter.Write((byte)116);
			pooledBinaryWriter.Write((byte)99);
			pooledBinaryWriter.Write((byte)0);
			pooledBinaryWriter.Write(Chunk.CurrentSaveVersion);
			chunk.save(pooledBinaryWriter);
			stream.Position = 0L;
		}
		catch (Exception ex)
		{
			Log.Error("Error writing blocks to stream (chunkX=" + chunk.X + " chunkZ=" + chunk.Z + "): " + ex.Message + "\nStackTrace: " + ex.StackTrace);
			MemoryPools.poolMS.FreeSync(stream);
		}
	}

	public void Write(RegionFileChunkWriter writer, string dir, int chunkX, int chunkZ)
	{
		if (stream != null)
		{
			writer.WriteStreamCompressed(dir, chunkX, chunkZ, "7rg", stream);
		}
	}

	public void Cleanup()
	{
		Reset();
	}

	public void Reset()
	{
		if (stream != null)
		{
			MemoryPools.poolMS.FreeSync(stream);
			stream = null;
		}
	}
}
