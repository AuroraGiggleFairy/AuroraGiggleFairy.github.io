using System;

public class DynamicMeshSyncRequest
{
	public DynamicMeshItem Item;

	public bool IsDelete;

	public bool SyncComplete;

	public byte[] Data;

	public bool HasData;

	public int Length;

	public int ClientId = -1;

	public DateTime? Initiated;

	public DateTime Created;

	public int SecondsAlive => (int)(DateTime.Now - Created).TotalSeconds;

	public int SecondsAttempted
	{
		get
		{
			if (Initiated.HasValue)
			{
				return (int)(DateTime.Now - Initiated.Value).TotalSeconds;
			}
			return 0;
		}
	}

	public static DynamicMeshSyncRequest Create(DynamicMeshItem item, bool isDelete)
	{
		return new DynamicMeshSyncRequest
		{
			Item = item,
			IsDelete = isDelete
		};
	}

	public static DynamicMeshSyncRequest Create(DynamicMeshItem item, bool isDelete, int clientId)
	{
		return new DynamicMeshSyncRequest
		{
			Item = item,
			IsDelete = isDelete,
			ClientId = clientId
		};
	}

	public bool TryGetData()
	{
		if (DynamicMeshThread.ChunkDataQueue.CollectBytes(Item.Key, out Data, out Length))
		{
			HasData = true;
			return true;
		}
		return false;
	}
}
