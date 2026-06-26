using System;

public struct ScopedChunkWriteAccess : IDisposable
{
	[field: PublicizedFrom(EAccessModifier.Private)]
	public Chunk Chunk
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public ScopedChunkWriteAccess(Chunk chunk)
	{
		Chunk = chunk;
		Chunk?.EnterWriteLock();
	}

	public void Dispose()
	{
		Chunk?.ExitWriteLock();
	}
}
