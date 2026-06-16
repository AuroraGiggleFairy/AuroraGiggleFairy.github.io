using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PowerManager
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const float UPDATE_TIME_SEC = 0.16f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float SAVE_TIME_SEC = 120f;

	public static byte FileVersion = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public static PowerManager instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<PowerItem> Circuits;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<PowerSource> PowerSources;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<PowerTrigger> PowerTriggers;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<Vector3i, PowerItem> PowerItemDictionary = new Dictionary<Vector3i, PowerItem>();

	[PublicizedFrom(EAccessModifier.Private)]
	public float updateTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float saveTime = 120f;

	[PublicizedFrom(EAccessModifier.Private)]
	public ThreadManager.ThreadInfo dataSaveThreadInfo;

	public List<TileEntityPoweredBlock> ClientUpdateList = new List<TileEntityPoweredBlock>();

	[field: PublicizedFrom(EAccessModifier.Private)]
	public byte CurrentFileVersion { get; set; }

	public static PowerManager Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new PowerManager();
			}
			return instance;
		}
	}

	public static bool HasInstance => instance != null;

	[PublicizedFrom(EAccessModifier.Private)]
	public PowerManager()
	{
		instance = this;
		Circuits = new List<PowerItem>();
		PowerSources = new List<PowerSource>();
		PowerTriggers = new List<PowerTrigger>();
	}

	public void Update()
	{
		if (GameManager.Instance.World == null || GameManager.Instance.World.Players == null || GameManager.Instance.World.Players.Count == 0)
		{
			return;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && GameManager.Instance.gameStateManager.IsGameStarted())
		{
			updateTime -= Time.deltaTime;
			if (updateTime <= 0f)
			{
				for (int i = 0; i < PowerSources.Count; i++)
				{
					PowerSources[i].Update();
				}
				for (int j = 0; j < PowerTriggers.Count; j++)
				{
					PowerTriggers[j].CachedUpdateCall();
				}
				updateTime = 0.16f;
			}
			saveTime -= Time.deltaTime;
			if (saveTime <= 0f && (dataSaveThreadInfo == null || dataSaveThreadInfo.HasTerminated()))
			{
				saveTime = 120f;
				SavePowerManager();
			}
		}
		for (int k = 0; k < ClientUpdateList.Count; k++)
		{
			ClientUpdateList[k].ClientUpdate();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int savePowerDataThreaded(ThreadManager.ThreadInfo _threadInfo)
	{
		PooledExpandableMemoryStream pooledExpandableMemoryStream = (PooledExpandableMemoryStream)_threadInfo.parameter;
		string text = string.Format("{0}/{1}", GameIO.GetSaveGameDir(), "power.dat");
		if (SdFile.Exists(text))
		{
			SdFile.Copy(text, string.Format("{0}/{1}", GameIO.GetSaveGameDir(), "power.dat.bak"), overwrite: true);
		}
		pooledExpandableMemoryStream.Position = 0L;
		StreamUtils.WriteStreamToFile(pooledExpandableMemoryStream, text);
		MemoryPools.poolMemoryStream.FreeSync(pooledExpandableMemoryStream);
		return -1;
	}

	public void LoadPowerManager()
	{
		string path = string.Format("{0}/{1}", GameIO.GetSaveGameDir(), "power.dat");
		if (!SdFile.Exists(path))
		{
			return;
		}
		try
		{
			using Stream baseStream = SdFile.OpenRead(path);
			using PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
			pooledBinaryReader.SetBaseStream(baseStream);
			Read(pooledBinaryReader);
		}
		catch (Exception)
		{
			path = string.Format("{0}/{1}", GameIO.GetSaveGameDir(), "power.dat.bak");
			if (!SdFile.Exists(path))
			{
				return;
			}
			using Stream baseStream2 = SdFile.OpenRead(path);
			using PooledBinaryReader pooledBinaryReader2 = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
			pooledBinaryReader2.SetBaseStream(baseStream2);
			Read(pooledBinaryReader2);
		}
	}

	public void SavePowerManager()
	{
		if (dataSaveThreadInfo == null || !ThreadManager.ActiveThreads.ContainsKey("powerDataSave"))
		{
			PooledExpandableMemoryStream pooledExpandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(_bReset: true);
			using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false))
			{
				pooledBinaryWriter.SetBaseStream(pooledExpandableMemoryStream);
				Write(pooledBinaryWriter);
			}
			dataSaveThreadInfo = ThreadManager.StartThread("powerDataSave", null, savePowerDataThreaded, null, pooledExpandableMemoryStream, null, _useRealThread: false, _isSilent: true);
		}
	}

	public void Write(BinaryWriter bw)
	{
		bw.Write(FileVersion);
		bw.Write(Circuits.Count);
		for (int i = 0; i < Circuits.Count; i++)
		{
			bw.Write((byte)Circuits[i].PowerItemType);
			Circuits[i].write(bw);
		}
	}

	public void Read(BinaryReader br)
	{
		CurrentFileVersion = br.ReadByte();
		Circuits.Clear();
		int num = br.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			PowerItem powerItem = PowerItem.CreateItem((PowerItem.PowerItemTypes)br.ReadByte());
			powerItem.read(br, CurrentFileVersion);
			AddPowerNode(powerItem);
		}
	}

	public void Cleanup()
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SavePowerManager();
		}
		instance = null;
		Circuits.Clear();
		if (dataSaveThreadInfo != null)
		{
			dataSaveThreadInfo.WaitForEnd();
			dataSaveThreadInfo = null;
		}
	}

	public void AddPowerNode(PowerItem node, PowerItem parent = null)
	{
		Circuits.Add(node);
		SetParent(node, parent);
		if (node is PowerSource)
		{
			PowerSources.Add((PowerSource)node);
		}
		if (node is PowerTrigger)
		{
			PowerTriggers.Add((PowerTrigger)node);
		}
		PowerItemDictionary.Add(node.Position, node);
	}

	public void RemovePowerNode(PowerItem node)
	{
		foreach (PowerItem item in new List<PowerItem>(node.Children))
		{
			SetParent(item, null);
		}
		SetParent(node, null);
		Circuits.Remove(node);
		if (node is PowerSource)
		{
			PowerSources.Remove((PowerSource)node);
		}
		if (node is PowerTrigger)
		{
			PowerTriggers.Remove((PowerTrigger)node);
		}
		if (PowerItemDictionary.ContainsKey(node.Position))
		{
			PowerItemDictionary.Remove(node.Position);
		}
	}

	public void FindPowerItems(Func<PowerItem, bool> _predicate, List<PowerItem> _results)
	{
		foreach (PowerItem value in PowerItemDictionary.Values)
		{
			if (_predicate(value))
			{
				_results.Add(value);
			}
		}
	}

	public void SetParent(PowerItem child, PowerItem parent)
	{
		if (child == null || child.Parent == parent || CircularParentCheck(parent, child))
		{
			return;
		}
		if (child.Parent != null)
		{
			RemoveParent(child);
		}
		if (parent != null)
		{
			if (child != null && Circuits.Contains(child))
			{
				Circuits.Remove(child);
			}
			parent.Children.Add(child);
			child.Parent = parent;
			child.SendHasLocalChangesToRoot();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CircularParentCheck(PowerItem Parent, PowerItem Child)
	{
		if (Parent == Child)
		{
			return true;
		}
		if (Parent != null && Parent.Parent != null)
		{
			return CircularParentCheck(Parent.Parent, Child);
		}
		return false;
	}

	public void RemoveParent(PowerItem node)
	{
		if (node.Parent != null)
		{
			PowerItem parent = node.Parent;
			node.Parent.Children.Remove(node);
			if (node.Parent.TileEntity != null)
			{
				node.Parent.TileEntity.CreateWireDataFromPowerItem();
				node.Parent.TileEntity.DrawWires();
			}
			node.Parent = null;
			Circuits.Add(node);
			parent.SendHasLocalChangesToRoot();
			node.HandleDisconnect();
		}
	}

	public void RemoveChild(PowerItem child)
	{
		child.Parent.Children.Remove(child);
		child.Parent = null;
		Circuits.Add(child);
	}

	public void SetParent(Vector3i childPos, Vector3i parentPos)
	{
		PowerItem powerItemByWorldPos = GetPowerItemByWorldPos(parentPos);
		PowerItem powerItemByWorldPos2 = GetPowerItemByWorldPos(childPos);
		SetParent(powerItemByWorldPos2, powerItemByWorldPos);
	}

	public PowerItem GetPowerItemByWorldPos(Vector3i position)
	{
		if (PowerItemDictionary.ContainsKey(position))
		{
			return PowerItemDictionary[position];
		}
		return null;
	}

	public void LogPowerManager()
	{
		for (int i = 0; i < PowerSources.Count; i++)
		{
			LogChildren(PowerSources[i]);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LogChildren(PowerItem item)
	{
		try
		{
			Log.Out($"{new string('\t', (item.Depth <= 100) ? (item.Depth + 1) : 0)}{item.ToString()}({item.Depth}) - Pos:{item.Position} | Powered:{item.IsPowered}");
			for (int i = 0; i < item.Children.Count; i++)
			{
				LogChildren(item.Children[i]);
			}
		}
		catch (Exception e)
		{
			Log.Exception(e);
		}
	}
}
