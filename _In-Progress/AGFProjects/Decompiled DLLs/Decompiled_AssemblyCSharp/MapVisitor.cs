using System;
using System.Collections;
using UnityEngine;

public class MapVisitor
{
	public delegate void VisitChunkDelegate(Chunk _chunk, int _chunksVisited, int _chunksTotal, float _elapsedSeconds);

	public delegate void VisitMapDoneDelegate(int _chunks, float _elapsedSeconds);

	[PublicizedFrom(EAccessModifier.Private)]
	public Coroutine coroutine;

	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkManager.ChunkObserver observer;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Vector3i chunkPos1;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Vector3i chunkPos2;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasBeenStarted;

	public Vector3i ChunkPosStart => chunkPos1;

	public Vector3i ChunkPosEnd => chunkPos2;

	public Vector3i WorldPosStart => new Vector3i(chunkPos1.x << 4, 0, chunkPos1.z << 4);

	public Vector3i WorldPosEnd => new Vector3i((chunkPos2.x + 1 << 4) - 1, 255, (chunkPos2.z + 1 << 4) - 1);

	public event VisitChunkDelegate OnVisitChunk;

	public event VisitMapDoneDelegate OnVisitMapDone;

	public MapVisitor(Vector3i _worldPos1, Vector3i _worldPos2)
	{
		int x = _worldPos1.x;
		int z = _worldPos1.z;
		int x2 = _worldPos2.x;
		int z2 = _worldPos2.z;
		chunkPos1 = new Vector3i(World.toChunkXZ((x <= x2) ? x : x2), 0, World.toChunkXZ((z <= z2) ? z : z2));
		chunkPos2 = new Vector3i(World.toChunkXZ((x <= x2) ? x2 : x), 0, World.toChunkXZ((z <= z2) ? z2 : z));
	}

	public void Start()
	{
		if (!hasBeenStarted)
		{
			coroutine = ThreadManager.StartCoroutine(visitCo());
			hasBeenStarted = true;
		}
	}

	public void Stop()
	{
		if (hasBeenStarted && coroutine != null)
		{
			ThreadManager.StopCoroutine(coroutine);
			GameManager.Instance.RemoveChunkObserver(observer);
			observer = null;
			coroutine = null;
		}
	}

	public bool IsRunning()
	{
		return coroutine != null;
	}

	public bool HasBeenStarted()
	{
		return hasBeenStarted;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator visitCo()
	{
		int viewDim = 8;
		int num = chunkPos2.x - chunkPos1.x + 1;
		int num2 = chunkPos2.z - chunkPos1.z + 1;
		int chunksTotal = num * num2;
		int chunksDone = 0;
		float startTime = Time.time;
		int curChunkX = Math.Min(chunkPos1.x + viewDim, chunkPos2.x);
		int curChunkZ = Math.Min(chunkPos1.z + viewDim, chunkPos2.z);
		observer = GameManager.Instance.AddChunkObserver(chunkPosToBlockPos(curChunkX, curChunkZ), _bBuildVisualMeshAround: false, viewDim, -1);
		yield return null;
		while (curChunkX - viewDim <= chunkPos2.x && curChunkZ - viewDim <= chunkPos2.z)
		{
			observer.SetPosition(chunkPosToBlockPos(curChunkX, curChunkZ));
			for (int xOffset = -viewDim; xOffset <= viewDim; xOffset++)
			{
				for (int zOffset = -viewDim; zOffset <= viewDim; zOffset++)
				{
					if (curChunkX + xOffset >= chunkPos1.x && curChunkZ + zOffset >= chunkPos1.z && curChunkX + xOffset <= chunkPos2.x && curChunkZ + zOffset <= chunkPos2.z)
					{
						Chunk chunk;
						while ((chunk = GameManager.Instance.World.GetChunkSync(curChunkX + xOffset, curChunkZ + zOffset) as Chunk) == null || chunk.NeedsDecoration)
						{
							yield return null;
						}
						chunksDone++;
						if (this.OnVisitChunk != null)
						{
							float elapsedSeconds = Time.time - startTime;
							this.OnVisitChunk(chunk, chunksDone, chunksTotal, elapsedSeconds);
						}
					}
				}
			}
			curChunkX += viewDim * 2 + 1;
			if (curChunkX - viewDim > chunkPos2.x)
			{
				curChunkX = Math.Min(chunkPos1.x + viewDim, chunkPos2.x);
				curChunkZ += viewDim * 2 + 1;
			}
		}
		yield return null;
		GameManager.Instance.RemoveChunkObserver(observer);
		observer = null;
		float elapsedSeconds2 = Time.time - startTime;
		if (this.OnVisitMapDone != null)
		{
			this.OnVisitMapDone(chunksDone, elapsedSeconds2);
		}
		coroutine = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 chunkPosToBlockPos(int _x, int _z)
	{
		return new Vector3(chunkXZtoBlockXZ(_x), 0f, chunkXZtoBlockXZ(_z));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int chunkXZtoBlockXZ(int _xz)
	{
		if (_xz >= 0)
		{
			return _xz * 16;
		}
		return _xz * 16 + 1;
	}
}
