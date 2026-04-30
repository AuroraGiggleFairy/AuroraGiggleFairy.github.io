using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DynamicMeshClientConnection
{
	public int EntityId;

	public ConcurrentDictionary<(int, int), ConcurrentQueue<DynamicMeshSyncRequest>> ItemsToSend = new ConcurrentDictionary<(int, int), ConcurrentQueue<DynamicMeshSyncRequest>>();

	public ConcurrentQueue<long> RequestedChunks = new ConcurrentQueue<long>();

	public List<long> FinalChunks = new List<long>();

	public long CurrentRequestedChunk = long.MaxValue;

	public DateTime RequestTime = DateTime.Now.AddDays(-1.0);

	public DateTime LastSend = DateTime.Now.AddDays(-1.0);

	public bool SendMessage;

	public (int, int) LastKey = ValueTuple.Create(0, 0);

	public bool TriggerSend => (DateTime.Now - LastSend).TotalSeconds > 1.0;

	public bool HasMessage => ItemsToSend.Count > 0;

	public DynamicMeshClientConnection(int entityId)
	{
		EntityId = entityId;
	}

	public void AddToQueue(DynamicMeshSyncRequest package)
	{
		(int, int) key = (DynamicMeshUnity.RoundRegion(package.Item.WorldPosition.x), DynamicMeshUnity.RoundRegion(package.Item.WorldPosition.z));
		ConcurrentQueue<DynamicMeshSyncRequest> orAdd = ItemsToSend.GetOrAdd(key, new ConcurrentQueue<DynamicMeshSyncRequest>());
		orAdd.Enqueue(package);
		SendMessage = DynamicMeshServer.AutoSend || SendMessage || orAdd.Count == 1;
	}

	public bool RequestChunk()
	{
		if (CurrentRequestedChunk != long.MaxValue)
		{
			return false;
		}
		if (!RequestedChunks.TryDequeue(out CurrentRequestedChunk))
		{
			return false;
		}
		RequestTime = DateTime.Now;
		DynamicMeshThread.RequestChunk(CurrentRequestedChunk);
		return true;
	}

	public void UpdateItemsToSend(NetPackageDynamicClientArrive package)
	{
		UpdateItemsToSend(this, package);
	}

	public static void UpdateItemsToSend(DynamicMeshClientConnection data, NetPackageDynamicClientArrive package)
	{
		if (DynamicMeshManager.Instance == null || DynamicMeshManager.Instance.ItemsDictionary == null)
		{
			DynamicMeshManager.LogMsg(package.Sender.playerName + " connected before the world was ready. Can not sync dymesh data. They must reconnect to start the sync");
			data.SendMessage = true;
			return;
		}
		data.SendMessage = false;
		data.ItemsToSend.Clear();
		DynamicMeshManager.LogMsg("Update items to send for " + package.Sender.playerName + " id: " + package.Sender.entityId + "  recieved: " + package.Items.Count);
		EntityPlayer entityPlayer = GameManager.Instance.World.GetPlayers().FirstOrDefault([PublicizedFrom(EAccessModifier.Internal)] (EntityPlayer d) => d.entityId == package.Sender.entityId);
		Vector3 playerPos = ((entityPlayer == null) ? Vector3.zero : entityPlayer.GetPosition());
		playerPos.y = 0f;
		List<DynamicMeshRegion> list = DynamicMeshRegion.Regions.Values.OrderBy([PublicizedFrom(EAccessModifier.Internal)] (DynamicMeshRegion d) => Math.Abs(Vector3.Distance(playerPos, d.WorldPosition.ToVector3()))).ToList();
		int num = 0;
		List<DynamicMeshItem> list2 = new List<DynamicMeshItem>(DynamicMeshManager.Instance.ItemsDictionary.Count);
		NetPackageRegionMetaData package2 = NetPackageManager.GetPackage<NetPackageRegionMetaData>();
		foreach (DynamicMeshRegion item in list)
		{
			num += ProcessItem(data, item, item.LoadedItems, package, package2);
			num += ProcessItem(data, item, item.UnloadedItems, package, package2);
		}
		package2.ChunksWithData.AddRange(DynamicMeshThread.PrimaryQueue.Values.Select([PublicizedFrom(EAccessModifier.Internal)] (DynamicMeshItem d) => new Vector2i(d.WorldPosition.x, d.WorldPosition.z)));
		package2.ChunksWithData.AddRange(DynamicMeshThread.SecondaryQueue.Values.Select([PublicizedFrom(EAccessModifier.Internal)] (DynamicMeshItem d) => new Vector2i(d.WorldPosition.x, d.WorldPosition.z)));
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package2, _onlyClientsAttachedToAnEntity: false, data.EntityId);
		DynamicMeshManager.LogMsg("Items to send: " + list2.Count + "   Added: " + num + "   chunks: " + package2.ChunksWithData.Count);
		data.SendMessage = data.ItemsToSend.Count > 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int ProcessItem(DynamicMeshClientConnection conn, DynamicMeshRegion r, List<DynamicMeshItem> items, NetPackageDynamicClientArrive package, NetPackageRegionMetaData allChunkData)
	{
		int num = 0;
		foreach (DynamicMeshItem i in items)
		{
			if (i != null)
			{
				if (i.FileExists())
				{
					allChunkData.ChunksWithData.Add(new Vector2i(i.WorldPosition.x, i.WorldPosition.z));
				}
				if (!package.Items.Any([PublicizedFrom(EAccessModifier.Internal)] (RegionItemData d) => d.X == i.WorldPosition.x && d.Z == i.WorldPosition.z && d.UpdateTime == i.UpdateTime))
				{
					DynamicMeshSyncRequest package2 = DynamicMeshSyncRequest.Create(i, isDelete: false, conn.EntityId);
					conn.AddToQueue(package2);
					num++;
				}
			}
		}
		return num;
	}
}
