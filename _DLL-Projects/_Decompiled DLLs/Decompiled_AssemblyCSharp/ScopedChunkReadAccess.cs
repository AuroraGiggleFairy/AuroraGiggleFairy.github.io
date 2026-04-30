using System;

public struct ScopedChunkReadAccess : IDisposable
{
	[field: PublicizedFrom(EAccessModifier.Private)]
	public Chunk Chunk
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public ScopedChunkReadAccess(Chunk chunk)
	{
		Chunk = chunk;
		Chunk?.EnterReadLock();
	}

	public void Dispose()
	{
		Chunk?.ExitReadLock();
	}
}
