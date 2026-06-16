using System;
using UnityEngine;

public class MovableSharedChunkObserver : IDisposable
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ISharedChunkObserver observer;

	[PublicizedFrom(EAccessModifier.Private)]
	public SharedChunkObserverCache cache;

	public MovableSharedChunkObserver(SharedChunkObserverCache _observerCache)
	{
		cache = _observerCache;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	~MovableSharedChunkObserver()
	{
		Dispose();
	}

	public void SetPosition(Vector3 newPosition)
	{
		Vector2i vector2i = new Vector2i(World.toChunkXZ(Utils.Fastfloor(newPosition.x)), World.toChunkXZ(Utils.Fastfloor(newPosition.z)));
		if (observer == null || observer.ChunkPos != vector2i)
		{
			if (observer != null)
			{
				observer.Dispose();
			}
			observer = cache.GetSharedObserverForChunk(vector2i);
		}
	}

	public void Dispose()
	{
		if (observer != null)
		{
			observer.Dispose();
			observer = null;
		}
	}
}
