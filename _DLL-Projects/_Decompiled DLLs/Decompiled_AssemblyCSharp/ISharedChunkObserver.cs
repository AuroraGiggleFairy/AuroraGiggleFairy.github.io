using System;

public interface ISharedChunkObserver : IDisposable
{
	Vector2i ChunkPos { get; }

	SharedChunkObserverCache Owner { get; }

	void Reference();
}
