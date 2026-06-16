using System;
using System.Collections.Generic;
using System.IO;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class TurretTracker
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cVersion = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSaveTime = 120f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cChangeSaveDelay = 10f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cMaxTurrets = 500;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int serverTurretCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<EntityTurret> turretsActive = new List<EntityTurret>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<int> turretsUnloaded = new List<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public float saveTime = 120f;

	[PublicizedFrom(EAccessModifier.Private)]
	public static TurretTracker instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public ThreadManager.ThreadInfo saveThread;

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cNameKey = "turrets";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cThreadKey = "turretDataSave";

	public static TurretTracker Instance => instance;

	public static void Init()
	{
		instance = new TurretTracker();
		instance.Load();
	}

	public void AddTrackedTurret(EntityTurret _turret)
	{
		if (!_turret)
		{
			Log.Error("{0} AddTrackedTurret null", GetType());
			return;
		}
		if (turretsUnloaded.Contains(_turret.entityId))
		{
			turretsUnloaded.Remove(_turret.entityId);
		}
		if (!turretsActive.Contains(_turret))
		{
			turretsActive.Add(_turret);
			TriggerSave();
		}
	}

	public void RemoveTrackedTurret(EntityTurret _turret, EnumRemoveEntityReason _reason)
	{
		turretsActive.Remove(_turret);
		if (_reason == EnumRemoveEntityReason.Unloaded)
		{
			turretsUnloaded.Add(_turret.entityId);
		}
		TriggerSave();
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageVehicleCount>().Setup());
	}

	public void TriggerSave()
	{
		saveTime = Mathf.Min(saveTime, 10f);
	}

	public void Update()
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return;
		}
		World world = GameManager.Instance.World;
		if (world != null && world.Players != null && world.Players.Count != 0 && GameManager.Instance.gameStateManager.IsGameStarted())
		{
			saveTime -= Time.deltaTime;
			if (saveTime <= 0f && (saveThread == null || saveThread.HasTerminated()))
			{
				saveTime = 120f;
				Save();
			}
		}
	}

	public static void Cleanup()
	{
		if (instance != null)
		{
			instance.SaveAndClear();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SaveAndClear()
	{
		WaitOnSave();
		Save();
		WaitOnSave();
		turretsActive.Clear();
		turretsUnloaded.Clear();
		instance = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WaitOnSave()
	{
		if (saveThread != null)
		{
			saveThread.WaitForEnd();
			saveThread = null;
		}
	}

	public void Load()
	{
		string text = string.Format("{0}/{1}", GameIO.GetSaveGameDir(), "turrets.dat");
		if (!SdFile.Exists(text))
		{
			return;
		}
		try
		{
			using Stream baseStream = SdFile.OpenRead(text);
			using PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
			pooledBinaryReader.SetBaseStream(baseStream);
			read(pooledBinaryReader);
		}
		catch (Exception)
		{
			text = string.Format("{0}/{1}", GameIO.GetSaveGameDir(), "turrets.dat.bak");
			if (SdFile.Exists(text))
			{
				using Stream baseStream2 = SdFile.OpenRead(text);
				using PooledBinaryReader pooledBinaryReader2 = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
				pooledBinaryReader2.SetBaseStream(baseStream2);
				read(pooledBinaryReader2);
			}
		}
		Log.Out("{0} {1}, loaded {2}", GetType(), text, turretsUnloaded.Count);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Save()
	{
		if (saveThread == null || !ThreadManager.ActiveThreads.ContainsKey("turretDataSave"))
		{
			Log.Out("{0} saving {1} ({2} + {3})", GetType(), turretsActive.Count + turretsUnloaded.Count, turretsActive.Count, turretsUnloaded.Count);
			PooledExpandableMemoryStream pooledExpandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(_bReset: true);
			using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false))
			{
				pooledBinaryWriter.SetBaseStream(pooledExpandableMemoryStream);
				write(pooledBinaryWriter);
			}
			saveThread = ThreadManager.StartThread("turretDataSave", null, SaveThread, null, pooledExpandableMemoryStream, null, _useRealThread: false, _isSilent: true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int SaveThread(ThreadManager.ThreadInfo _threadInfo)
	{
		PooledExpandableMemoryStream pooledExpandableMemoryStream = (PooledExpandableMemoryStream)_threadInfo.parameter;
		string text = string.Format("{0}/{1}", GameIO.GetSaveGameDir(), "turrets.dat");
		if (SdFile.Exists(text))
		{
			SdFile.Copy(text, string.Format("{0}/{1}", GameIO.GetSaveGameDir(), "turrets.dat.bak"), overwrite: true);
		}
		pooledExpandableMemoryStream.Position = 0L;
		StreamUtils.WriteStreamToFile(pooledExpandableMemoryStream, text);
		Log.Out("{0} saved {1} bytes", GetType(), pooledExpandableMemoryStream.Length);
		MemoryPools.poolMemoryStream.FreeSync(pooledExpandableMemoryStream);
		return -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void read(PooledBinaryReader _br)
	{
		if (_br.ReadChar() != 'v' || _br.ReadChar() != 'd' || _br.ReadChar() != 'a' || _br.ReadChar() != 0)
		{
			Log.Error("{0} file bad signature", GetType());
			return;
		}
		if (_br.ReadByte() != 1)
		{
			Log.Error("{0} file bad version", GetType());
			return;
		}
		turretsUnloaded.Clear();
		int num = _br.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			turretsUnloaded.Add(_br.ReadInt32());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void write(PooledBinaryWriter _bw)
	{
		_bw.Write('v');
		_bw.Write('d');
		_bw.Write('a');
		_bw.Write((byte)0);
		_bw.Write((byte)1);
		List<int> list = new List<int>();
		GetTurrets(list);
		_bw.Write(list.Count);
		for (int i = 0; i < list.Count; i++)
		{
			_bw.Write(list[i]);
		}
	}

	public List<int> GetTurretsList()
	{
		List<int> list = new List<int>();
		GetTurrets(list);
		return list;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GetTurrets(List<int> _list)
	{
		for (int i = 0; i < turretsActive.Count; i++)
		{
			_list.Add(turretsActive[i].entityId);
		}
		for (int j = 0; j < turretsUnloaded.Count; j++)
		{
			_list.Add(turretsUnloaded[j]);
		}
	}

	public static int GetServerTurretCount()
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return Instance.turretsActive.Count + Instance.turretsUnloaded.Count;
		}
		return serverTurretCount;
	}

	public static void SetServerTurretCount(int count)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			serverTurretCount = count;
		}
	}

	public static bool CanAddMoreTurrets()
	{
		if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
		{
			return GetServerTurretCount() < 500;
		}
		return true;
	}
}
