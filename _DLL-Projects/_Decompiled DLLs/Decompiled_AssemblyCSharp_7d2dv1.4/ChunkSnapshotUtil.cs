using System;

public class ChunkSnapshotUtil : IRegionFileChunkSnapshotUtil
{
	[PublicizedFrom(EAccessModifier.Private)]
	public RegionFileAccessAbstract regionFileAccess;

	[PublicizedFrom(EAccessModifier.Private)]
	public RegionFileChunkReader chunkReader;

	[PublicizedFrom(EAccessModifier.Private)]
	public RegionFileChunkWriter chunkWriter;

	[PublicizedFrom(EAccessModifier.Private)]
	public MemoryPooledObject<RegionFileChunkSnapshot> poolSnapshots = new MemoryPooledObject<RegionFileChunkSnapshot>(255);

	public ChunkSnapshotUtil(RegionFileAccessAbstract regionFileAccess)
	{
		this.regionFileAccess = regionFileAccess;
		chunkReader = new RegionFileChunkReader(regionFileAccess);
		chunkWriter = new RegionFileChunkWriter(regionFileAccess);
	}

	public IRegionFileChunkSnapshot TakeSnapshot(Chunk chunk, bool saveIfUnchanged)
	{
		RegionFileChunkSnapshot regionFileChunkSnapshot = poolSnapshots.AllocSync(_bReset: true);
		regionFileChunkSnapshot.Update(chunk, saveIfUnchanged);
		return regionFileChunkSnapshot;
	}

	public void WriteSnapshot(IRegionFileChunkSnapshot snapshot, string dir, int chunkX, int chunkZ)
	{
		snapshot.Write(chunkWriter, dir, chunkX, chunkZ);
	}

	public Chunk LoadChunk(string dir, long key)
	{
		int chunkX = WorldChunkCache.extractX(key);
		int chunkZ = WorldChunkCache.extractZ(key);
		try
		{
			uint version;
			PooledBinaryReader pooledBinaryReader = chunkReader.readIntoLoadStream(dir, chunkX, chunkZ, "7rg", out version);
			if (pooledBinaryReader == null)
			{
				return null;
			}
			Chunk chunk = MemoryPools.PoolChunks.AllocSync(_bReset: true);
			chunk.load(pooledBinaryReader, version);
			chunk.NeedsRegeneration = true;
			return chunk;
		}
		catch (Exception e)
		{
			Log.Error("EXCEPTION: In load chunk (chunkX=" + chunkX + " chunkZ=" + chunkZ + ")");
			Log.Exception(e);
			try
			{
				chunkReader.WriteBackup(dir, chunkX, chunkZ);
			}
			catch (Exception e2)
			{
				Log.Error("Error backing up data:");
				Log.Exception(e2);
			}
		}
		try
		{
			regionFileAccess.Remove(dir, chunkX, chunkZ);
		}
		catch (Exception ex)
		{
			Log.Error("In remove chunk (chunkX=" + chunkX + " chunkZ=" + chunkZ + "):" + ex.Message);
		}
		return null;
	}

	public void Free(IRegionFileChunkSnapshot iSnapshot)
	{
		if (iSnapshot != null)
		{
			if (iSnapshot is RegionFileChunkSnapshot t)
			{
				poolSnapshots.FreeSync(t);
			}
			else
			{
				Log.Error("Attempting to free snapshot of wrong type. Expected: RegionFileChunkSnapshot, Actual: " + iSnapshot.GetType().Name);
			}
		}
	}

	public void Cleanup()
	{
		poolSnapshots.Cleanup();
	}
}
