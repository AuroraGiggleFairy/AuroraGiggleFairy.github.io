public interface IDynamicDecorator
{
	void DecorateChunk(World _world, Chunk _chunk);

	void Cleanup();
}
