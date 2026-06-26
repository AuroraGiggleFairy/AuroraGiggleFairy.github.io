using System.Collections;
using UnityEngine;

public static class ProfilerGameUtils
{
	public static bool TryGetFlyingPlayer(out EntityPlayerLocal player)
	{
		World world = GameManager.Instance.World;
		if (world == null)
		{
			player = null;
			return false;
		}
		player = world.GetPrimaryPlayer();
		if (player == null)
		{
			player = null;
			return false;
		}
		if ((bool)player.AttachedToEntity)
		{
			player.Detach();
		}
		player.IsGodMode.Value = true;
		player.IsNoCollisionMode.Value = true;
		player.IsFlyMode.Value = true;
		player.Buffs.AddBuff("god");
		return true;
	}

	public static IEnumerator WaitForSingleChunkToLoad(int chunkX, int chunkZ, ChunkConditions.Delegate chunkCondition)
	{
		ChunkCluster cc = GameManager.Instance.World.ChunkClusters[0];
		Chunk chunk = null;
		while (chunk == null)
		{
			chunk = cc.GetChunkSync(chunkX, chunkZ);
			yield return null;
		}
		WaitForSingleChunkToLoad(chunk, chunkCondition);
	}

	public static IEnumerator WaitForSingleChunkToLoad(Chunk chunk, ChunkConditions.Delegate chunkCondition)
	{
		if (chunk != null)
		{
			float startTime = Time.realtimeSinceStartup;
			while (!chunkCondition(chunk) && !(Time.realtimeSinceStartup - startTime > 30f))
			{
				yield return null;
			}
		}
	}

	public static IEnumerator WaitForChunksAroundObserverToLoad(ChunkManager.ChunkObserver observer, ChunkConditions.Delegate chunkCondition)
	{
		ChunkCluster cc = GameManager.Instance.World.ChunkClusters[0];
		bool isLoaded = false;
		float startTime = Time.realtimeSinceStartup;
		while (!isLoaded)
		{
			isLoaded = true;
			for (int i = 0; i <= observer.viewDim - 2; i++)
			{
				foreach (long item in observer.chunksAround.buckets.array[i])
				{
					Chunk chunkSync = cc.GetChunkSync(item);
					if (chunkSync == null)
					{
						isLoaded = false;
						break;
					}
					if (!chunkCondition(chunkSync))
					{
						isLoaded = false;
						break;
					}
				}
			}
			if (Time.realtimeSinceStartup - startTime > 30f)
			{
				isLoaded = true;
				Debug.LogErrorFormat("Could not load Chunks at player chunk pos ({0},{1}). Forcing continue", observer.curChunkPos.x, observer.curChunkPos.z);
			}
			yield return false;
		}
	}
}
