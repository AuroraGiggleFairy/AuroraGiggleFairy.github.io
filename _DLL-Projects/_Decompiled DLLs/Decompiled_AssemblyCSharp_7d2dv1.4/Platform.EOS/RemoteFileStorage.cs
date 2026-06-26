using System;
using System.Collections.Generic;
using System.IO;
using Epic.OnlineServices;
using Epic.OnlineServices.TitleStorage;

namespace Platform.EOS;

public class RemoteFileStorage : IRemoteFileStorage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class RequestDetails
	{
		public readonly string Filename;

		public readonly ProductUserId LocalUserId;

		public readonly List<ArraySegment<byte>> Chunks = new List<ArraySegment<byte>>();

		public event IRemoteFileStorage.FileDownloadCompleteCallback Callback;

		public RequestDetails(string _filename, ProductUserId _localUserId, IRemoteFileStorage.FileDownloadCompleteCallback _callback)
		{
			Filename = _filename;
			LocalUserId = _localUserId;
			this.Callback = _callback;
		}

		public void ExecuteCallback(IRemoteFileStorage.EFileDownloadResult _result, string _errorName, byte[] _data)
		{
			this.Callback?.Invoke(_result, _errorName, _data);
		}
	}

	public const string CacheFolderName = "RfsCache";

	[PublicizedFrom(EAccessModifier.Private)]
	public IPlatform owner;

	[PublicizedFrom(EAccessModifier.Private)]
	public TitleStorageInterface titleStorageInterface;

	[PublicizedFrom(EAccessModifier.Private)]
	public object requestsLock = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, RequestDetails> requests = new Dictionary<string, RequestDetails>();

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

	public bool Unavailable
	{
		get
		{
			switch (owner?.User?.UserStatus)
			{
			case EUserStatus.OfflineMode:
			case EUserStatus.PermanentError:
			case EUserStatus.NotAttempted:
				return true;
			default:
				return false;
			}
		}
	}

	public string CacheFilePrefix
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return "Rfs_";
		}
	}

	public string CacheFolder
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return GameIO.GetUserGameDataDir() + "/RfsCache";
		}
	}

	public void Init(IPlatform _owner)
	{
		owner = _owner;
		owner.Api.ClientApiInitialized += OnClientApiInitialized;
		owner.User.UserLoggedIn += OnUserLoggedIn;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnUserLoggedIn(IPlatform _obj)
	{
		if (IsReady)
		{
			clearCache();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnClientApiInitialized()
	{
		titleStorageInterface = ((Api)owner.Api).PlatformInterface.GetTitleStorageInterface();
	}

	public void GetFile(string _filename, IRemoteFileStorage.FileDownloadCompleteCallback _callback)
	{
		if (_callback == null)
		{
			return;
		}
		if (string.IsNullOrEmpty(_filename))
		{
			_callback(IRemoteFileStorage.EFileDownloadResult.EmptyFilename, null, null);
			return;
		}
		ProductUserId productUserId = ((UserIdentifierEos)PlatformManager.CrossplatformPlatform.User.PlatformUserId).ProductUserId;
		bool flag;
		RequestDetails value;
		lock (requestsLock)
		{
			flag = !requests.TryGetValue(_filename, out value);
			if (flag)
			{
				Log.Out("[EOS] Created RFS Request: " + _filename);
				value = new RequestDetails(_filename, productUserId, _callback);
				requests.Add(_filename, value);
			}
			else
			{
				Log.Out("[EOS] Adding callback to existing RFS Request: " + _filename);
				value.Callback += _callback;
			}
		}
		if (flag)
		{
			EosHelpers.AssertMainThread("RFS.Get");
			getMetadata(value);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void getMetadata(RequestDetails _details)
	{
		QueryFileOptions options = new QueryFileOptions
		{
			Filename = _details.Filename,
			LocalUserId = _details.LocalUserId
		};
		lock (AntiCheatCommon.LockObject)
		{
			titleStorageInterface.QueryFile(ref options, _details, queryFileCallback);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void queryFileCallback(ref QueryFileCallbackInfo _callbackData)
	{
		RequestDetails requestDetails = (RequestDetails)_callbackData.ClientData;
		if (_callbackData.ResultCode != Result.Success)
		{
			Log.Error("[EOS] QueryFile (" + requestDetails.Filename + ") failed: " + _callbackData.ResultCode.ToStringCached());
			CompleteRequest(requestDetails, IRemoteFileStorage.EFileDownloadResult.FileNotFound, _callbackData.ResultCode.ToStringCached(), null);
			return;
		}
		CopyFileMetadataByFilenameOptions options = new CopyFileMetadataByFilenameOptions
		{
			Filename = requestDetails.Filename,
			LocalUserId = requestDetails.LocalUserId
		};
		Result result;
		lock (AntiCheatCommon.LockObject)
		{
			result = titleStorageInterface.CopyFileMetadataByFilename(ref options, out var _);
		}
		if (result != Result.Success)
		{
			Log.Error("[EOS] CopyFileMetadataByFilename (" + requestDetails.Filename + ") failed: " + result.ToStringCached());
			CompleteRequest(requestDetails, IRemoteFileStorage.EFileDownloadResult.Other, _callbackData.ResultCode.ToStringCached(), null);
		}
		else
		{
			readFile(requestDetails);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void readFile(RequestDetails _details)
	{
		ReadFileOptions options = new ReadFileOptions
		{
			Filename = _details.Filename,
			LocalUserId = _details.LocalUserId,
			ReadFileDataCallback = readFileDataCallback,
			ReadChunkLengthBytes = 524288u
		};
		lock (AntiCheatCommon.LockObject)
		{
			titleStorageInterface.ReadFile(ref options, _details, readCompletedCallback);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void fileTransferProgressCallback(ref FileTransferProgressCallbackInfo _callbackData)
	{
		_ = (RequestDetails)_callbackData.ClientData;
		Log.Out($"[EOS] TransferProgress: {_callbackData.Filename}, {_callbackData.BytesTransferred} / {_callbackData.TotalFileSizeBytes}");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ReadResult readFileDataCallback(ref ReadFileDataCallbackInfo _callbackData)
	{
		((RequestDetails)_callbackData.ClientData).Chunks.Add(_callbackData.DataChunk);
		return ReadResult.RrContinuereading;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void readCompletedCallback(ref ReadFileCallbackInfo _callbackData)
	{
		RequestDetails requestDetails = (RequestDetails)_callbackData.ClientData;
		if (_callbackData.ResultCode != Result.Success)
		{
			Log.Error("[EOS] Read (" + requestDetails.Filename + ") failed: " + _callbackData.ResultCode.ToStringCached());
			if (_callbackData.ResultCode != Result.OperationWillRetry)
			{
				CompleteRequest(requestDetails, IRemoteFileStorage.EFileDownloadResult.Other, _callbackData.ResultCode.ToStringCached(), null);
			}
			return;
		}
		int num = 0;
		foreach (ArraySegment<byte> chunk in requestDetails.Chunks)
		{
			num += chunk.Count;
		}
		byte[] array = new byte[num];
		int num2 = 0;
		for (int i = 0; i < requestDetails.Chunks.Count; i++)
		{
			Array.Copy(requestDetails.Chunks[i].Array, requestDetails.Chunks[i].Offset, array, num2, requestDetails.Chunks[i].Count);
			num2 += requestDetails.Chunks[i].Count;
		}
		Log.Out($"[EOS] Read ({_callbackData.Filename}) completed: {_callbackData.ResultCode}, received {num} bytes");
		cacheFile(requestDetails.Filename, array);
		CompleteRequest(requestDetails, IRemoteFileStorage.EFileDownloadResult.Ok, null, array);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CompleteRequest(RequestDetails _details, IRemoteFileStorage.EFileDownloadResult _result, string _errorName, byte[] _data)
	{
		lock (requestsLock)
		{
			if (!requests.Remove(_details.Filename))
			{
				Log.Warning("[EOS] Unexpected RFS request being completed: " + _details.Filename);
			}
		}
		_details.ExecuteCallback(_result, _errorName, _data);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void clearCache()
	{
		if (!SdDirectory.Exists(CacheFolder))
		{
			return;
		}
		string[] files = SdDirectory.GetFiles(CacheFolder);
		foreach (string path in files)
		{
			if (Path.GetFileName(path).StartsWith(CacheFilePrefix))
			{
				SdFile.Delete(path);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void cacheFile(string _filename, byte[] _data)
	{
		SdDirectory.CreateDirectory(CacheFolder);
		SdFile.WriteAllBytes(CacheFolder + "/" + CacheFilePrefix + _filename, _data);
	}

	public void GetCachedFile(string _filename, IRemoteFileStorage.FileDownloadCompleteCallback _callback)
	{
		if (_callback == null)
		{
			return;
		}
		if (string.IsNullOrEmpty(_filename))
		{
			_callback(IRemoteFileStorage.EFileDownloadResult.EmptyFilename, null, null);
			return;
		}
		string path = CacheFolder + "/" + CacheFilePrefix + _filename;
		if (!SdFile.Exists(path))
		{
			_callback(IRemoteFileStorage.EFileDownloadResult.FileNotFound, "File not found", null);
			return;
		}
		byte[] array = SdFile.ReadAllBytes(path);
		Log.Out($"[EOS] Read cached ({_filename}) completed: {array.Length} bytes");
		_callback(IRemoteFileStorage.EFileDownloadResult.Ok, null, array);
	}
}
