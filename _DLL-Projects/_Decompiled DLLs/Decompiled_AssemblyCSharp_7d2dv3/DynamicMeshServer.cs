using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class DynamicMeshServer
{
	public static Dictionary<int, DynamicMeshClientConnection> ClientData = new Dictionary<int, DynamicMeshClientConnection>();

	public static List<NetPackageDynamicClientArrive> DelayedClientChecks = new List<NetPackageDynamicClientArrive>();

	public static bool AutoSend = false;

	public static DynamicMeshServerType ConnectionType = DynamicMeshServerType.Mesh;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float debugTime = 0f;

	public static bool ResendPackages = true;

	public static bool ShowSender = true;

	public static ConcurrentQueue<DynamicMeshSyncRequest> SyncRequests = new ConcurrentQueue<DynamicMeshSyncRequest>();

	public static List<DynamicMeshSyncRequest> ActiveSyncs = new List<DynamicMeshSyncRequest>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static ConcurrentQueue<DynamicMeshItem> SyncReleaseQueue = new ConcurrentQueue<DynamicMeshItem>();

	public static int MaxActiveSyncs = 10;

	public static void OnClientConnect(ClientInfo info)
	{
		GetData(info.entityId);
		if (DynamicMeshManager.DoLog)
		{
			DynamicMeshManager.LogMsg("User " + info.entityId + " joined dynamic meshes: " + ClientData.Count);
		}
	}

	public static void OnClientDisconnect(ClientInfo info)
	{
		int entityId = info.entityId;
		Log.Out("Client disconnected from dy mesh: Id: " + entityId + " Total: " + ClientData.Count);
		try
		{
			ClientData.Remove(entityId);
		}
		catch (Exception ex)
		{
			Log.Error("Client removal error: " + ex.Message);
		}
	}

	public static void CleanUp()
	{
		ClientData.Clear();
		DelayedClientChecks.Clear();
		DynamicMeshItem result;
		while (SyncReleaseQueue.TryDequeue(out result))
		{
		}
		DynamicMeshSyncRequest result2;
		while (SyncRequests.TryDequeue(out result2))
		{
		}
		ActiveSyncs.Clear();
	}

	public static void SendToAllClients(DynamicMeshItem item, bool isDelete)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.ClientCount() != 0)
		{
			DynamicMeshSyncRequest item2 = DynamicMeshSyncRequest.Create(item, isDelete);
			SyncRequests.Enqueue(item2);
		}
	}

	public static void SyncRelease(DynamicMeshItem item)
	{
		if (item != null)
		{
			SyncReleaseQueue.Enqueue(item);
		}
	}

	public static void Update()
	{
		while (true)
		{
			if (!SyncReleaseQueue.TryDequeue(out var item))
			{
				break;
			}
			DynamicMeshSyncRequest dynamicMeshSyncRequest = ActiveSyncs.FirstOrDefault([PublicizedFrom(EAccessModifier.Internal)] (DynamicMeshSyncRequest d) => d.Item.WorldPosition.x == item.WorldPosition.x && d.Item.WorldPosition.z == item.WorldPosition.z);
			if (dynamicMeshSyncRequest == null)
			{
				Log.Warning("Active sync could not be found to be cleared: " + item.ToDebugLocation());
			}
			else
			{
				dynamicMeshSyncRequest.SyncComplete = true;
			}
		}
		if (ActiveSyncs.Count < MaxActiveSyncs && SyncRequests.TryDequeue(out var result))
		{
			result.Initiated = DateTime.Now;
			ActiveSyncs.Add(result);
		}
		for (int num = ActiveSyncs.Count - 1; num >= 0; num--)
		{
			DynamicMeshSyncRequest dynamicMeshSyncRequest2 = ActiveSyncs[num];
			if (dynamicMeshSyncRequest2.SyncComplete || SingletonMonoBehaviour<ConnectionManager>.Instance.ClientCount() == 0)
			{
				DynamicMeshThread.ChunkDataQueue.ManuallyReleaseBytes(dynamicMeshSyncRequest2.Data);
				if (DynamicMeshManager.DoLogNet)
				{
					int num2 = (int)(DateTime.Now - dynamicMeshSyncRequest2.Initiated.Value).TotalMilliseconds;
					Log.Out("Package for " + ((dynamicMeshSyncRequest2.ClientId == -1) ? "all" : dynamicMeshSyncRequest2.ClientId.ToString()) + " took " + num2 + "ms for " + dynamicMeshSyncRequest2.Length + " bytes");
				}
				ActiveSyncs.RemoveAt(num);
			}
			else if (!dynamicMeshSyncRequest2.HasData)
			{
				if (dynamicMeshSyncRequest2.IsDelete || dynamicMeshSyncRequest2.TryGetData())
				{
					NetPackageDynamicMesh package = NetPackageManager.GetPackage<NetPackageDynamicMesh>();
					package.Setup(dynamicMeshSyncRequest2.Item, dynamicMeshSyncRequest2.Data);
					package.PresumedLength = dynamicMeshSyncRequest2.Length;
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package, _onlyClientsAttachedToAnEntity: false, dynamicMeshSyncRequest2.ClientId);
				}
			}
			else if (dynamicMeshSyncRequest2.SecondsAttempted > 20)
			{
				Log.Warning("Sync waited more than 20 seconds. Removing...");
				ActiveSyncs.Remove(dynamicMeshSyncRequest2);
				break;
			}
		}
		foreach (DynamicMeshClientConnection con in ClientData.Values)
		{
			if ((!con.SendMessage && !con.TriggerSend) || con.ItemsToSend.Count <= 0)
			{
				continue;
			}
			con.SendMessage = false;
			ConcurrentQueue<DynamicMeshSyncRequest> value = null;
			if (!con.ItemsToSend.ContainsKey(con.LastKey))
			{
				EntityPlayer entityPlayer = GameManager.Instance.World.GetPlayers().FirstOrDefault([PublicizedFrom(EAccessModifier.Internal)] (EntityPlayer d) => d.entityId == con.EntityId);
				Vector3 playerPos = ((entityPlayer == null) ? Vector3.zero : entityPlayer.GetPosition());
				(int, int) tuple = (from d in con.ItemsToSend.Keys.ToList()
					orderby Math.Abs(Mathf.Sqrt(Mathf.Pow(playerPos.x - (float)d.Item1, 2f) + Mathf.Pow(playerPos.z - (float)d.Item2, 2f)))
					select d).FirstOrDefault();
				con.LastKey = tuple;
				if (DynamicMeshManager.DoLog || DynamicMeshManager.DoLogNet)
				{
					float num3 = Math.Abs(Mathf.Sqrt(Mathf.Pow(playerPos.x - (float)tuple.Item1, 2f) + Mathf.Pow(playerPos.z - (float)tuple.Item2, 2f)));
					int count = con.ItemsToSend[tuple].Count;
					string[] obj = new string[6] { "Switching key to ", null, null, null, null, null };
					(int, int) tuple2 = tuple;
					obj[1] = tuple2.ToString();
					obj[2] = " Dist: ";
					obj[3] = num3.ToString();
					obj[4] = "   Items: ";
					obj[5] = count.ToString();
					Log.Out(string.Concat(obj));
				}
			}
			DynamicMeshSyncRequest result2;
			if (!con.ItemsToSend.TryGetValue(con.LastKey, out value))
			{
				if (!DynamicMeshManager.DoLog || DynamicMeshManager.DoLogNet)
				{
					Log.Out("Could not find last key to skipping");
				}
			}
			else if (value.Count == 0)
			{
				con.ItemsToSend.TryRemove(con.LastKey, out value);
			}
			else if (value.TryDequeue(out result2))
			{
				SyncRequests.Enqueue(result2);
				con.LastSend = DateTime.Now;
			}
		}
		if (debugTime < Time.time)
		{
			if (DynamicMeshManager.DoLog || DynamicMeshManager.DoLogNet)
			{
				DynamicMeshManager.LogMsg("Dyn Mesh Server update. " + ClientData.Count + "   Prefabs: " + DynamicMeshManager.Instance.PrefabCheck.ToString() + " buff: " + DynamicMeshManager.Instance.BufferRegionLoadRequests.Count + "  chunkData: " + DynamicMeshManager.Instance.ChunkMeshData.Count + "   Primary: " + DynamicMeshThread.PrimaryQueue.Count + "   Secondary: " + DynamicMeshThread.SecondaryQueue.Count);
			}
			debugTime = Time.time + 10f;
		}
	}

	public static void ChunkRequested(long key, bool isFinal, ClientInfo client)
	{
		DynamicMeshClientConnection data = GetData(client.entityId);
		data.RequestedChunks.Enqueue(key);
		if (isFinal)
		{
			data.FinalChunks.Add(key);
		}
	}

	public static DynamicMeshClientConnection GetData(int entityId)
	{
		if (!ClientData.TryGetValue(entityId, out var value))
		{
			value = new DynamicMeshClientConnection(entityId);
			ClientData.Add(entityId, value);
		}
		return value;
	}

	public static DynamicMeshClientConnection GetData(NetPackage package)
	{
		return GetData(package.Sender.entityId);
	}

	public static void ClientMessageRecieved(NetPackageDynamicClientArrive package)
	{
		GetData(package).UpdateItemsToSend(package);
	}

	public static void ProcessDelayedPackages()
	{
		if (DynamicMeshManager.DoLog)
		{
			DynamicMeshManager.LogMsg("==Processing delayed packages: " + DelayedClientChecks.Count);
		}
		foreach (NetPackageDynamicClientArrive delayedClientCheck in DelayedClientChecks)
		{
			GetData(delayedClientCheck).UpdateItemsToSend(delayedClientCheck);
		}
	}

	public static void ClientReadyForNextMesh(NetPackageDynamicMesh package)
	{
		GetData(package).SendMessage = true;
	}
}
