using System;
using System.Collections.Generic;
using UnityEngine;

public class SharedChunkObserverCache
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class SharedChunkObserver : ISharedChunkObserver, IDisposable
	{
		public delegate void RemoveObserver(SharedChunkObserver observer);

		public Vector2i chunkPos;

		public int refCount;

		public ChunkManager.ChunkObserver chunkRef;

		[PublicizedFrom(EAccessModifier.Private)]
		public SharedChunkObserverCache cache;

		[PublicizedFrom(EAccessModifier.Private)]
		public RemoveObserver removeObserver;

		public Vector2i ChunkPos => chunkPos;

		public SharedChunkObserverCache Owner => cache;

		public SharedChunkObserver(SharedChunkObserverCache _cache, ChunkManager.ChunkObserver _chunkRef, RemoveObserver _removeObserverDelegate, Vector2i _chunkPos)
		{
			cache = _cache;
			chunkRef = _chunkRef;
			removeObserver = _removeObserverDelegate;
			chunkPos = _chunkPos;
			refCount = 1;
		}

		public void Reference()
		{
			if (cache.ThreadingSemantics.InterlockedAdd(ref refCount, 1) < 2)
			{
				throw new Exception("Synchronization error: shared chunk observer was already disposed with a ref count of zero!");
			}
		}

		public void Dispose()
		{
			if (cache.ThreadingSemantics.InterlockedAdd(ref refCount, -1) == 0)
			{
				removeObserver(this);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IThreadingSemantics threadingSemantics;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<Vector2i, SharedChunkObserver> observers = new Dictionary<Vector2i, SharedChunkObserver>();

	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkManager chunkManager;

	[PublicizedFrom(EAccessModifier.Private)]
	public int viewDim;

	public IThreadingSemantics ThreadingSemantics => threadingSemantics;

	public SharedChunkObserverCache(ChunkManager _chunkManager, int _viewDim, IThreadingSemantics _threadingSemantics)
	{
		chunkManager = _chunkManager;
		viewDim = _viewDim;
		threadingSemantics = _threadingSemantics;
	}

	public ISharedChunkObserver GetSharedObserverForChunk(Vector2i chunkPos)
	{
		return threadingSemantics.Synchronize([PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			if (observers.TryGetValue(chunkPos, out var value) && value.refCount < 1)
			{
				value = null;
			}
			if (value != null)
			{
				value.Reference();
			}
			else
			{
				value = new SharedChunkObserver(this, chunkManager.AddChunkObserver(new Vector3(chunkPos.x << 4, 0f, chunkPos.y << 4), _bBuildVisualMeshAround: false, viewDim, -1), removeChunkObserver, chunkPos);
				observers[chunkPos] = value;
			}
			return value;
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void removeChunkObserver(SharedChunkObserver observer)
	{
		threadingSemantics.Synchronize([PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			if (observers[observer.chunkPos] == observer)
			{
				observers.Remove(observer.chunkPos);
			}
		});
		chunkManager.RemoveChunkObserver(observer.chunkRef);
	}
}
