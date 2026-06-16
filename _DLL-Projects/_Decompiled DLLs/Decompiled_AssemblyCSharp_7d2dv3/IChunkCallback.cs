public interface IChunkCallback
{
	void OnChunkAdded(Chunk _c);

	void OnChunkBeforeRemove(Chunk _c);

	void OnChunkBeforeSave(Chunk _c);
}
