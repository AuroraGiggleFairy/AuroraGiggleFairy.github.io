using System;
using System.Collections.Generic;
using System.IO;
using Platform;

public class BlockLimitTracker
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cVersion = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cMaxPowerBlocks = 10000;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cMaxPlayerStorageBlocks = 10000;

	public static BlockLimitTracker instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockTracker poweredBlockTracker;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockTracker playerStorageTracker;

	[PublicizedFrom(EAccessModifier.Private)]
	public ThreadManager.ThreadInfo saveThread;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> clientAmounts;

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cNameKey = "blockLimits";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cThreadKey = "blockLimitSaveData";

	public static void Init()
	{
		instance = new BlockLimitTracker();
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			instance.Load();
		}
	}

	public BlockLimitTracker()
	{
		poweredBlockTracker = new BlockTracker(10000);
		playerStorageTracker = new BlockTracker(10000);
		clientAmounts = new List<int>();
	}

	public bool CanAddBlock(BlockValue _blockValue, Vector3i _blockPosition, out eSetBlockResponse _response)
	{
		_response = eSetBlockResponse.Success;
		if ((DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX).IsCurrent() || GameManager.Instance.IsEditMode())
		{
			return true;
		}
		if (_blockValue.Block.isMultiBlock && _blockValue.ischild)
		{
			return true;
		}
		if (_blockValue.Block is BlockPowered || _blockValue.Block is BlockPowerSource)
		{
			if (!poweredBlockTracker.CanAdd(_blockPosition))
			{
				_response = eSetBlockResponse.PowerBlockLimitExceeded;
				return false;
			}
		}
		else if ((_blockValue.Block is BlockLoot || _blockValue.Block is BlockSecureLoot || (_blockValue.Block is BlockCompositeTileEntity blockCompositeTileEntity && blockCompositeTileEntity.CompositeData.HasFeature<ITileEntityLootable>())) && !playerStorageTracker.CanAdd(_blockPosition))
		{
			_response = eSetBlockResponse.StorageBlockLimitExceeded;
			return false;
		}
		return true;
	}

	public void TryAddTrackedBlock(BlockValue _blockValue, Vector3i _blockPosition, int _entityId)
	{
		if (_entityId == -1 || _blockValue.isair || (_blockValue.Block.isMultiBlock && _blockValue.ischild))
		{
			return;
		}
		Entity entity = GameManager.Instance.World.GetEntity(_entityId);
		if (entity == null || !(entity is EntityPlayer))
		{
			return;
		}
		Log.Out("TryAddTrackedBlock {0} from entity {1}", _blockValue.Block.GetBlockName(), _entityId);
		if (_blockValue.Block is BlockPowered || _blockValue.Block is BlockPowerSource)
		{
			if (poweredBlockTracker.TryAddBlock(_blockPosition))
			{
				TriggerSave();
				Log.Out("{0}/{1} Powered Blocks", poweredBlockTracker.blockLocations.Count, poweredBlockTracker.limit);
			}
		}
		else if ((_blockValue.Block is BlockLoot || _blockValue.Block is BlockSecureLoot || (_blockValue.Block is BlockCompositeTileEntity blockCompositeTileEntity && blockCompositeTileEntity.CompositeData.HasFeature<ITileEntityLootable>())) && playerStorageTracker.TryAddBlock(_blockPosition))
		{
			TriggerSave();
			Log.Out("{0}/{1} Storage Blocks", playerStorageTracker.blockLocations.Count, playerStorageTracker.limit);
		}
	}

	public void TryRemoveOrReplaceBlock(BlockValue _oldBlockValue, BlockValue _newBlockValue, Vector3i _blockPosition)
	{
		if (_oldBlockValue.Block.isMultiBlock && _oldBlockValue.ischild)
		{
			return;
		}
		if (_oldBlockValue.Block is BlockPowered || _oldBlockValue.Block is BlockPowerSource)
		{
			if (!(_newBlockValue.Block is BlockPowered) && !(_newBlockValue.Block is BlockPowerSource) && poweredBlockTracker.RemoveBlock(_blockPosition))
			{
				TriggerSave();
				Log.Out("{0}/{1} powered Blocks", poweredBlockTracker.blockLocations.Count, poweredBlockTracker.limit);
			}
		}
		else if ((_oldBlockValue.Block is BlockLoot || _oldBlockValue.Block is BlockSecureLoot || (_oldBlockValue.Block is BlockCompositeTileEntity blockCompositeTileEntity && blockCompositeTileEntity.CompositeData.HasFeature<ITileEntityLootable>())) && !(_newBlockValue.Block is BlockLoot) && !(_newBlockValue.Block is BlockSecureLoot) && (!(_newBlockValue.Block is BlockCompositeTileEntity blockCompositeTileEntity2) || !blockCompositeTileEntity2.CompositeData.HasFeature<ITileEntityLootable>()) && playerStorageTracker.RemoveBlock(_blockPosition))
		{
			TriggerSave();
			Log.Out("{0}/{1} Storage Blocks", playerStorageTracker.blockLocations.Count, playerStorageTracker.limit);
		}
	}

	public void ServerUpdateClients()
	{
		if (clientAmounts.Count == 0 || clientAmounts[0] != poweredBlockTracker.blockLocations.Count || clientAmounts[1] != playerStorageTracker.blockLocations.Count)
		{
			clientAmounts.Clear();
			clientAmounts.Add(poweredBlockTracker.blockLocations.Count);
			clientAmounts.Add(playerStorageTracker.blockLocations.Count);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageBlockLimitTracking>().Setup(clientAmounts));
		}
	}

	public void UpdateClientAmounts(List<int> _amounts)
	{
		if (_amounts.Count != 2)
		{
			Log.Error("Client block limit count not exepcted amount");
			return;
		}
		poweredBlockTracker.clientAmount = _amounts[0];
		playerStorageTracker.clientAmount = _amounts[1];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ClearAll()
	{
		poweredBlockTracker.Clear();
		playerStorageTracker.Clear();
	}

	public static void Cleanup()
	{
		if (instance != null)
		{
			instance.Save();
			instance.ClearAll();
			instance = null;
		}
	}

	public void Load()
	{
		string path = string.Format("{0}/{1}", GameIO.GetSaveGameDir(), "blockLimits.dat");
		if (!SdFile.Exists(path))
		{
			return;
		}
		try
		{
			using Stream baseStream = SdFile.OpenRead(path);
			using PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
			pooledBinaryReader.SetBaseStream(baseStream);
			read(pooledBinaryReader);
		}
		catch (Exception ex)
		{
			Log.Error("BlockLimitTracker Load Exception: " + ex.Message);
			path = string.Format("{0}/{1}", GameIO.GetSaveGameDir(), "blockLimits.dat.bak");
			if (!SdFile.Exists(path))
			{
				return;
			}
			using Stream baseStream2 = SdFile.OpenRead(path);
			using PooledBinaryReader pooledBinaryReader2 = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
			pooledBinaryReader2.SetBaseStream(baseStream2);
			read(pooledBinaryReader2);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void read(PooledBinaryReader _reader)
	{
		if (_reader.ReadByte() != 1)
		{
			Log.Error("BlockLimitTracker Read bad version");
			return;
		}
		ClearAll();
		poweredBlockTracker.Read(_reader);
		playerStorageTracker.Read(_reader);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TriggerSave()
	{
		if (saveThread == null || saveThread.HasTerminated())
		{
			Save();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Save()
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && (saveThread == null || !ThreadManager.ActiveThreads.ContainsKey("blockLimitSaveData")))
		{
			PooledExpandableMemoryStream pooledExpandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(_bReset: true);
			using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false))
			{
				pooledBinaryWriter.SetBaseStream(pooledExpandableMemoryStream);
				write(pooledBinaryWriter);
			}
			saveThread = ThreadManager.StartThread("blockLimitSaveData", null, SaveThread, null, pooledExpandableMemoryStream, null, _useRealThread: false, _isSilent: true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int SaveThread(ThreadManager.ThreadInfo _threadInfo)
	{
		PooledExpandableMemoryStream pooledExpandableMemoryStream = (PooledExpandableMemoryStream)_threadInfo.parameter;
		string text = string.Format("{0}/{1}", GameIO.GetSaveGameDir(), "blockLimits.dat");
		if (SdFile.Exists(text))
		{
			SdFile.Copy(text, string.Format("{0}/{1}.dat.bak", GameIO.GetSaveGameDir(), "blockLimits"), overwrite: true);
		}
		pooledExpandableMemoryStream.Position = 0L;
		StreamUtils.WriteStreamToFile(pooledExpandableMemoryStream, text);
		MemoryPools.poolMemoryStream.FreeSync(pooledExpandableMemoryStream);
		return -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void write(PooledBinaryWriter _writer)
	{
		_writer.Write((byte)1);
		poweredBlockTracker.Write(_writer);
		playerStorageTracker.Write(_writer);
	}
}
