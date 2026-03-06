using System;
using System.Collections.Generic;
using Unity.Mathematics;

public class WaterSimulationApplyChanges
{
	public class ChangesForChunk : IMemoryPoolableObject
	{
		public struct Writer : IDisposable
		{
			[PublicizedFrom(EAccessModifier.Private)]
			public ChangesForChunk changes;

			public Writer(ChangesForChunk _changes)
			{
				lock (_changes)
				{
					_changes.isRecordingChanges = true;
				}
				changes = _changes;
			}

			public void RecordChange(int _voxelIndex, WaterValue _waterValue)
			{
				changes.changedVoxels[_voxelIndex] = _waterValue;
			}

			public void Dispose()
			{
				lock (changes)
				{
					changes.isRecordingChanges = false;
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool isRecordingChanges;

		public Dictionary<int, WaterValue> changedVoxels = new Dictionary<int, WaterValue>();

		public bool IsRecordingChanges
		{
			get
			{
				lock (this)
				{
					return isRecordingChanges;
				}
			}
		}

		public void Cleanup()
		{
			Reset();
		}

		public void Reset()
		{
			isRecordingChanges = false;
			changedVoxels.Clear();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public MemoryPooledObject<ChangesForChunk> changesPool = new MemoryPooledObject<ChangesForChunk>(300);

	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkCluster chunks;

	[PublicizedFrom(EAccessModifier.Private)]
	public ThreadManager.ThreadInfo applyThread;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int noWorkPauseDurationMs = 15;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<long, ChangesForChunk> changeCache = new Dictionary<long, ChangesForChunk>();

	[PublicizedFrom(EAccessModifier.Private)]
	public LinkedList<long> changedChunkList = new LinkedList<long>();

	[PublicizedFrom(EAccessModifier.Private)]
	public NetPackageMeasure networkMeasure = new NetPackageMeasure(1.0);

	public long networkMaxBytesPerSecond = 524288L;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<ClientInfo> clientsNearChunkBuffer = new List<ClientInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isServer;

	public WaterSimulationApplyChanges(ChunkCluster _cc)
	{
		chunks = _cc;
		isServer = SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer;
		applyThread = ThreadManager.StartThread("WaterSimulationApplyChanges", null, ThreadLoop, null, null, null, _useRealThread: true);
	}

	public ChangesForChunk.Writer GetChangeWriter(long _chunkKey)
	{
		lock (changeCache)
		{
			if (!changeCache.TryGetValue(_chunkKey, out var value))
			{
				value = changesPool.AllocSync(_bReset: true);
				changeCache.Add(_chunkKey, value);
				changedChunkList.AddLast(_chunkKey);
			}
			return new ChangesForChunk.Writer(value);
		}
	}

	public void DiscardChangesForChunks(List<long> _chunkKeys)
	{
		lock (changeCache)
		{
			foreach (long _chunkKey in _chunkKeys)
			{
				if (changeCache.TryGetValue(_chunkKey, out var value))
				{
					changesPool.FreeSync(value);
					changeCache.Remove(_chunkKey);
					changedChunkList.Remove(_chunkKey);
					Log.Out($"[DiscardChangesForChunks] Discarding pending water changes for chunk: {_chunkKey}");
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int ThreadLoop(ThreadManager.ThreadInfo _threadInfo)
	{
		if (_threadInfo.TerminationRequested())
		{
			return -1;
		}
		if (isServer && networkMaxBytesPerSecond > 0)
		{
			lock (networkMeasure)
			{
				networkMeasure.RecalculateTotals();
			}
		}
		if (!TryFindChangeToApply(out var _chunk, out var _changes))
		{
			return 15;
		}
		ApplyChanges(_chunk, _changes.changedVoxels);
		_chunk.EnterWriteLock();
		_chunk.InProgressWaterSim = false;
		_chunk.ExitWriteLock();
		changesPool.FreeSync(_changes);
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryFindChangeToApply(out Chunk _chunk, out ChangesForChunk _changes)
	{
		lock (changeCache)
		{
			if (changedChunkList.Count == 0)
			{
				_chunk = null;
				_changes = null;
				return false;
			}
			LinkedListNode<long> linkedListNode = changedChunkList.First;
			while (linkedListNode != null)
			{
				long value = linkedListNode.Value;
				if (!changeCache.TryGetValue(value, out _changes))
				{
					LinkedListNode<long> node = linkedListNode;
					linkedListNode = linkedListNode.Next;
					changedChunkList.Remove(node);
					continue;
				}
				if (_changes.IsRecordingChanges)
				{
					linkedListNode = linkedListNode.Next;
					continue;
				}
				if (!WaterUtils.TryOpenChunkForUpdate(chunks, value, out _chunk))
				{
					linkedListNode = linkedListNode.Next;
					continue;
				}
				LinkedListNode<long> node2 = linkedListNode;
				linkedListNode = linkedListNode.Next;
				changedChunkList.Remove(node2);
				changeCache.Remove(value);
				return true;
			}
			_chunk = null;
			_changes = null;
			return false;
		}
	}

	public void ApplyChanges(Chunk _chunk, Dictionary<int, WaterValue> changedVoxels)
	{
		NetPackageWaterSimChunkUpdate netPackageWaterSimChunkUpdate = null;
		if (isServer && SingletonMonoBehaviour<ConnectionManager>.Instance.ClientCount() > 0)
		{
			netPackageWaterSimChunkUpdate = SetupForSend(_chunk);
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		foreach (KeyValuePair<int, WaterValue> changedVoxel in changedVoxels)
		{
			int key = changedVoxel.Key;
			int3 voxelCoords = WaterDataHandle.GetVoxelCoords(key);
			WaterValue value = changedVoxel.Value;
			_chunk.SetWaterSimUpdate(voxelCoords.x, voxelCoords.y, voxelCoords.z, value, out var _lastData);
			if (value.GetMass() == _lastData.GetMass())
			{
				continue;
			}
			if (WaterUtils.GetWaterLevel(_lastData) != WaterUtils.GetWaterLevel(value))
			{
				int num7 = 1 << voxelCoords.y / 16;
				num2 |= num7;
				if (voxelCoords.x == 0)
				{
					num3 |= num7;
				}
				else if (voxelCoords.x == 15)
				{
					num4 |= num7;
				}
				if (voxelCoords.z == 0)
				{
					num5 |= num7;
				}
				else if (voxelCoords.z == 15)
				{
					num6 |= num7;
				}
			}
			netPackageWaterSimChunkUpdate?.AddChange((ushort)key, value);
		}
		if (netPackageWaterSimChunkUpdate != null)
		{
			netPackageWaterSimChunkUpdate.FinalizeSend();
			num += SendUpdateToClients(netPackageWaterSimChunkUpdate);
		}
		if (num2 != 0)
		{
			lock (_chunk)
			{
				int needsRegenerationAt = _chunk.NeedsRegenerationAt;
				_chunk.SetNeedsRegenerationRaw(needsRegenerationAt |= num2);
			}
		}
		if (num3 != 0)
		{
			Chunk chunkSync = chunks.GetChunkSync(_chunk.X - 1, _chunk.Z);
			if (chunkSync != null)
			{
				int needsRegenerationAt2 = _chunk.NeedsRegenerationAt;
				chunkSync.SetNeedsRegenerationRaw(needsRegenerationAt2 |= num3);
			}
		}
		if (num4 != 0)
		{
			Chunk chunkSync2 = chunks.GetChunkSync(_chunk.X + 1, _chunk.Z);
			if (chunkSync2 != null)
			{
				int needsRegenerationAt3 = _chunk.NeedsRegenerationAt;
				chunkSync2.SetNeedsRegenerationRaw(needsRegenerationAt3 |= num4);
			}
		}
		if (num5 != 0)
		{
			Chunk chunkSync3 = chunks.GetChunkSync(_chunk.X, _chunk.Z - 1);
			if (chunkSync3 != null)
			{
				int needsRegenerationAt4 = _chunk.NeedsRegenerationAt;
				chunkSync3.SetNeedsRegenerationRaw(needsRegenerationAt4 |= num5);
			}
		}
		if (num6 != 0)
		{
			Chunk chunkSync4 = chunks.GetChunkSync(_chunk.X, _chunk.Z + 1);
			if (chunkSync4 != null)
			{
				int needsRegenerationAt5 = _chunk.NeedsRegenerationAt;
				chunkSync4.SetNeedsRegenerationRaw(needsRegenerationAt5 |= num6);
			}
		}
		if (num <= 0)
		{
			return;
		}
		if (networkMaxBytesPerSecond > 0)
		{
			lock (networkMeasure)
			{
				networkMeasure.AddSample(num);
			}
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.FlushClientSendQueues();
	}

	public bool HasNetWorkLimitBeenReached()
	{
		if (isServer && networkMaxBytesPerSecond > 0 && SingletonMonoBehaviour<ConnectionManager>.Instance.ClientCount() > 0)
		{
			lock (networkMeasure)
			{
				return networkMeasure.totalSent > networkMaxBytesPerSecond;
			}
		}
		return false;
	}

	public NetPackageWaterSimChunkUpdate SetupForSend(Chunk _chunk)
	{
		clientsNearChunkBuffer.Clear();
		long key = _chunk.Key;
		foreach (ClientInfo item in SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.List)
		{
			if (GameManager.Instance.World.Players.dict.TryGetValue(item.entityId, out var value) && value.ChunkObserver.chunksAround.Contains(key))
			{
				clientsNearChunkBuffer.Add(item);
			}
		}
		NetPackageWaterSimChunkUpdate package = NetPackageManager.GetPackage<NetPackageWaterSimChunkUpdate>();
		package.SetupForSend(_chunk);
		return package;
	}

	public int SendUpdateToClients(NetPackageWaterSimChunkUpdate _package)
	{
		int num = 0;
		_package.RegisterSendQueue();
		foreach (ClientInfo item in clientsNearChunkBuffer)
		{
			item.SendPackage(_package);
			num += _package.GetLength();
		}
		_package.SendQueueHandled();
		return num;
	}

	public void Cleanup()
	{
		applyThread.WaitForEnd();
	}
}
