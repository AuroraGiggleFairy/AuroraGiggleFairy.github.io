using System;
using UnityEngine;

public class DynamicObserver
{
	public static int ViewSize = 3;

	public Vector3 Position;

	public ChunkManager.ChunkObserver Observer;

	public float StopTime = float.MaxValue;

	public void Start(Vector3 pos)
	{
		Position = pos;
		if (GameManager.Instance.World != null)
		{
			if (Observer == null)
			{
				Observer = GameManager.Instance.AddChunkObserver(pos, _bBuildVisualMeshAround: false, ViewSize, GameManager.Instance.World.GetPrimaryPlayerId());
			}
			Observer.SetPosition(Position);
			StopTime = float.MaxValue;
		}
	}

	public bool ContainsPoint(Vector3i pos)
	{
		int num = ViewSize * 16;
		if ((float)pos.x >= Position.x - (float)num && (float)pos.x <= Position.x + (float)num && (float)pos.z >= Position.z - (float)num)
		{
			return (float)pos.z <= Position.z + (float)num;
		}
		return false;
	}

	public bool HasFallingBlocks()
	{
		foreach (long item in Observer.chunksLoaded)
		{
			if (GameManager.Instance == null)
			{
				return false;
			}
			if (GameManager.Instance.World == null)
			{
				return false;
			}
			Chunk chunk = (Chunk)GameManager.Instance.World.GetChunkSync(item);
			if (chunk == null)
			{
				DynamicMeshManager.LogMsg("Observer couldn't load chunk so assuming falling");
				return true;
			}
			if (chunk.HasFallingBlocks())
			{
				DynamicMeshManager.Instance.AddUpdateData(chunk.Key, isUrgent: false, addToThread: true, checkPlayerArea: true, 3);
				return true;
			}
		}
		return false;
	}

	public void Stop()
	{
		if (Observer == null)
		{
			return;
		}
		try
		{
			GameManager.Instance.RemoveChunkObserver(Observer);
		}
		catch (Exception ex)
		{
			if (DynamicMeshManager.DoLog)
			{
				DynamicMeshManager.LogMsg("Observer already destroyed: " + ex.Message);
			}
		}
		Observer = null;
	}
}
