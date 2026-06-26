using System;
using System.Collections.Generic;
using Epic.OnlineServices;
using Epic.OnlineServices.PlayerDataStorage;

namespace Platform.EOS;

public class RemotePlayerFileStorage : IRemotePlayerFileStorage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public struct StorageOperation
	{
		public bool ToRead;

		public string Filename;

		public bool OverwriteCache;

		public byte[] Data;

		public IRemotePlayerFileStorage.FileReadCompleteCallback ReadCallback;

		public IRemotePlayerFileStorage.FileWriteCompleteCallback WriteCallback;

		public StorageOperation(string _filename, bool _overwriteCache, IRemotePlayerFileStorage.FileReadCompleteCallback _callback)
		{
			ToRead = true;
			Filename = _filename;
			OverwriteCache = _overwriteCache;
			Data = null;
			ReadCallback = _callback;
			WriteCallback = null;
		}

		public StorageOperation(string _filename, byte[] _data, bool _overwriteCache, IRemotePlayerFileStorage.FileWriteCompleteCallback _callback)
		{
			ToRead = false;
			Filename = _filename;
			Data = _data;
			OverwriteCache = _overwriteCache;
			ReadCallback = null;
			WriteCallback = _callback;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class ReadRequestDetails
	{
		public readonly IRemotePlayerFileStorage.FileReadCompleteCallback Callback;

		public List<ArraySegment<byte>> Chunks;

		public bool OverwriteCache;

		public ReadRequestDetails(IRemotePlayerFileStorage.FileReadCompleteCallback _callback, bool _overwriteCache)
		{
			Callback = _callback;
			OverwriteCache = _overwriteCache;
			Chunks = new List<ArraySegment<byte>>();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class WriteRequestDetails
	{
		public bool WriteToCache;

		public byte[] Data;

		public readonly IRemotePlayerFileStorage.FileWriteCompleteCallback Callback;

		[PublicizedFrom(EAccessModifier.Private)]
		public int DataPointer;

		public WriteRequestDetails(bool _writeToCache, byte[] _data, IRemotePlayerFileStorage.FileWriteCompleteCallback _callback)
		{
			WriteToCache = _writeToCache;
			Data = _data;
			Callback = _callback;
		}

		public ArraySegment<byte>? GetNextChunk()
		{
			if (DataPointer >= Data.Length)
			{
				return null;
			}
			int val = Data.Length - DataPointer;
			int num = Math.Min(524288, val);
			ArraySegment<byte> value = new ArraySegment<byte>(Data, DataPointer, num);
			DataPointer += num;
			return value;
		}

		public bool HasNextChunk()
		{
			return DataPointer < Data.Length;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cReadWriteByteLimit = 524288;

	[PublicizedFrom(EAccessModifier.Private)]
	public IPlatform owner;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerDataStorageInterface playerDataStorage;

	[PublicizedFrom(EAccessModifier.Private)]
	public Queue<StorageOperation> operationQueue = new Queue<StorageOperation>();

	[PublicizedFrom(EAccessModifier.Private)]
	public object queueLock = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool queueProcessing;

	public bool IsReady
	{
		get
		{
			IPlatform platform = owner;
			if (platform == null)
			{
				return false;
			}
			return platform.User?.UserStatus == EUserStatus.LoggedIn;
		}
	}

	public void Init(IPlatform _owner)
	{
		owner = _owner;
		owner.Api.ClientApiInitialized += OnClientApiInitialized;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnClientApiInitialized()
	{
		playerDataStorage = ((Api)owner.Api).PlatformInterface.GetPlayerDataStorageInterface();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProcessNextOperation()
	{
		lock (queueLock)
		{
			if (!queueProcessing && operationQueue.TryDequeue(out var result))
			{
				queueProcessing = true;
				if (result.ToRead)
				{
					ProcessReadOperation(result.Filename, result.OverwriteCache, result.ReadCallback);
				}
				else
				{
					ProcessWriteOperation(result.Filename, result.Data, result.OverwriteCache, result.WriteCallback);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CompleteActiveOperation()
	{
		lock (queueLock)
		{
			queueProcessing = false;
			ProcessNextOperation();
		}
	}

	public void ReadRemoteData(string _filename, bool _overwriteCache, IRemotePlayerFileStorage.FileReadCompleteCallback _callback)
	{
		lock (queueLock)
		{
			operationQueue.Enqueue(new StorageOperation(_filename, _overwriteCache, _callback));
			ProcessNextOperation();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProcessReadOperation(string _filename, bool _overwriteCache, IRemotePlayerFileStorage.FileReadCompleteCallback _callback)
	{
		if (_callback == null)
		{
			Log.Warning("[EOS] PlayerDataStorage Read Operation failed as no callback supplied.");
			CompleteActiveOperation();
			return;
		}
		if (!IsReady)
		{
			Log.Warning("[EOS] Tried to read from PlayerDataStorage user is not logged in.");
			_callback(IRemotePlayerFileStorage.CallbackResult.NoConnection, null);
			CompleteActiveOperation();
			return;
		}
		if (playerDataStorage == null)
		{
			Log.Warning("[EOS] Tried to read from PlayerDataStorage but it was null.");
			_callback(IRemotePlayerFileStorage.CallbackResult.NoConnection, null);
			CompleteActiveOperation();
			return;
		}
		if (string.IsNullOrEmpty(_filename))
		{
			Log.Warning("[EOS] Supplied filename was null or empty.");
			_callback(IRemotePlayerFileStorage.CallbackResult.FileNotFound, null);
			CompleteActiveOperation();
			return;
		}
		ProductUserId productUserId = ((UserIdentifierEos)PlatformManager.CrossplatformPlatform.User.PlatformUserId).ProductUserId;
		ReadRequestDetails clientData = new ReadRequestDetails(_callback, _overwriteCache);
		ReadFileOptions readOptions = new ReadFileOptions
		{
			Filename = _filename,
			LocalUserId = productUserId,
			ReadChunkLengthBytes = 524288u,
			ReadFileDataCallback = ReadChunkCallback
		};
		lock (AntiCheatCommon.LockObject)
		{
			playerDataStorage.ReadFile(ref readOptions, clientData, ReadFileCompleteCallback);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ReadResult ReadChunkCallback(ref ReadFileDataCallbackInfo _callbackData)
	{
		((ReadRequestDetails)_callbackData.ClientData).Chunks.Add(_callbackData.DataChunk);
		return ReadResult.ContinueReading;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ReadFileCompleteCallback(ref ReadFileCallbackInfo _callbackData)
	{
		try
		{
			ReadRequestDetails readRequestDetails = (ReadRequestDetails)_callbackData.ClientData;
			if (_callbackData.ResultCode != Result.Success)
			{
				if (_callbackData.ResultCode == Result.NotFound)
				{
					readRequestDetails.Callback(IRemotePlayerFileStorage.CallbackResult.FileNotFound, null);
				}
				else if (_callbackData.ResultCode != Result.OperationWillRetry)
				{
					Log.Warning($"[EOS] Read from PlayerDataStorage failed ({_callbackData.Filename}): {_callbackData.ResultCode.ToStringCached()}");
					readRequestDetails.Callback(IRemotePlayerFileStorage.CallbackResult.Other, null);
				}
				return;
			}
			int num = 0;
			foreach (ArraySegment<byte> chunk in readRequestDetails.Chunks)
			{
				num += chunk.Count;
			}
			byte[] array = new byte[num];
			int num2 = 0;
			for (int i = 0; i < readRequestDetails.Chunks.Count; i++)
			{
				Array.Copy(readRequestDetails.Chunks[i].Array, readRequestDetails.Chunks[i].Offset, array, num2, readRequestDetails.Chunks[i].Count);
				num2 += readRequestDetails.Chunks[i].Count;
			}
			if (readRequestDetails.OverwriteCache)
			{
				IRemotePlayerFileStorage.WriteCachedObject(owner.User, _callbackData.Filename, array);
			}
			Log.Out($"[EOS] Read ({_callbackData.Filename}) completed: {_callbackData.ResultCode}, received {num} bytes");
			readRequestDetails.Callback(IRemotePlayerFileStorage.CallbackResult.Success, array);
		}
		finally
		{
			CompleteActiveOperation();
		}
	}

	public void WriteRemoteData(string _filename, byte[] _data, bool _overwriteCache, IRemotePlayerFileStorage.FileWriteCompleteCallback _callback)
	{
		lock (queueLock)
		{
			operationQueue.Enqueue(new StorageOperation(_filename, _data, _overwriteCache, _callback));
			ProcessNextOperation();
		}
	}

	public void ProcessWriteOperation(string _filename, byte[] _data, bool _overwriteCache, IRemotePlayerFileStorage.FileWriteCompleteCallback _callback)
	{
		if (_callback == null)
		{
			Log.Warning("[EOS] PlayerDataStorage Write Operation failed as no callback supplied.");
			CompleteActiveOperation();
			return;
		}
		if (!IsReady)
		{
			Log.Warning("[EOS] Tried to write to PlayerDataStorage user is not logged in.");
			CompleteActiveOperation();
			_callback(IRemotePlayerFileStorage.CallbackResult.NoConnection);
			return;
		}
		if (playerDataStorage == null)
		{
			Log.Warning("[EOS] Tried to write to PlayerDataStorage but it was null.");
			CompleteActiveOperation();
			_callback(IRemotePlayerFileStorage.CallbackResult.NoConnection);
			return;
		}
		if (string.IsNullOrEmpty(_filename))
		{
			Log.Warning("[EOS] Supplied filename was null or empty.");
			CompleteActiveOperation();
			_callback(IRemotePlayerFileStorage.CallbackResult.FileNotFound);
			return;
		}
		if (_data == null)
		{
			Log.Warning("[EOS] Supplied data to store was null.");
			CompleteActiveOperation();
			_callback(IRemotePlayerFileStorage.CallbackResult.MalformedData);
			return;
		}
		WriteRequestDetails clientData = new WriteRequestDetails(_overwriteCache, _data, _callback);
		ProductUserId productUserId = ((UserIdentifierEos)PlatformManager.CrossplatformPlatform.User.PlatformUserId).ProductUserId;
		WriteFileOptions writeOptions = new WriteFileOptions
		{
			Filename = _filename,
			LocalUserId = productUserId,
			ChunkLengthBytes = 524288u,
			WriteFileDataCallback = WriteFileChunkCallback
		};
		lock (AntiCheatCommon.LockObject)
		{
			playerDataStorage.WriteFile(ref writeOptions, clientData, WriteFileCompleteCallback);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public WriteResult WriteFileChunkCallback(ref WriteFileDataCallbackInfo _callbackData, out ArraySegment<byte> _outDataBuffer)
	{
		WriteRequestDetails writeRequestDetails = (WriteRequestDetails)_callbackData.ClientData;
		_outDataBuffer = writeRequestDetails.GetNextChunk() ?? ((ArraySegment<byte>)null);
		if (writeRequestDetails.HasNextChunk())
		{
			return WriteResult.ContinueWriting;
		}
		return WriteResult.CompleteRequest;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WriteFileCompleteCallback(ref WriteFileCallbackInfo _callbackData)
	{
		try
		{
			WriteRequestDetails writeRequestDetails = (WriteRequestDetails)_callbackData.ClientData;
			IRemotePlayerFileStorage.CallbackResult result = IRemotePlayerFileStorage.CallbackResult.Success;
			if (_callbackData.ResultCode != Result.Success && _callbackData.ResultCode != Result.OperationWillRetry)
			{
				Log.Warning($"[EOS] Write to PlayerDataStorage failed ({_callbackData.Filename}): {_callbackData.ResultCode.ToStringCached()}");
				result = IRemotePlayerFileStorage.CallbackResult.Other;
			}
			if (writeRequestDetails.WriteToCache && !IRemotePlayerFileStorage.WriteCachedObject(owner.User, _callbackData.Filename, writeRequestDetails.Data))
			{
				Log.Warning($"[EOS] Write to PlayerDataStorage succeeded ({_callbackData.Filename}), but failed while saving to local cache.");
			}
			writeRequestDetails.Callback?.Invoke(result);
		}
		finally
		{
			CompleteActiveOperation();
		}
	}
}
