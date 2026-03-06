public interface IRegionFileChunkSnapshot
{
	long Size { get; }

	void Update(Chunk chunk, bool saveIfUnchanged);

	void Write(RegionFileChunkWriter writer, string dir, int chunkX, int chunkZ);

	void Reset();
}
