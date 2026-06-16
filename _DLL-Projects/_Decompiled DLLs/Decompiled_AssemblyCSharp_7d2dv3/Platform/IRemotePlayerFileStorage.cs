using System;

namespace Platform;

public interface IRemotePlayerFileStorage
{
	public enum CallbackResult
	{
		Success,
		FileNotFound,
		NoConnection,
		MalformedData,
		Other
	}

	public delegate void FileReadCompleteCallback(CallbackResult _result, byte[] _data);

	public delegate void FileReadObjectCompleteCallback<T>(CallbackResult _result, T _object) where T : IRemotePlayerStorageObject;

	public delegate void FileWriteCompleteCallback(CallbackResult _result);

	static object LocalCacheLock;

	void Init(IPlatform _owner);

	void ReadRemoteData(string _filename, bool _overwriteCache, FileReadCompleteCallback _callback);

	void WriteRemoteData(string _filename, byte[] _data, bool _overwriteCache, FileWriteCompleteCallback _callback);

	sealed void ReadRemoteObject<T>(string _filename, bool _overwriteCache, FileReadObjectCompleteCallback<T> _callback) where T : IRemotePlayerStorageObject, new()
	{
		if (_callback == null)
		{
			Log.Error("[RPFS] Read failed as no callback was supplied");
			return;
		}
		ReadRemoteData(_filename, _overwriteCache, [PublicizedFrom(EAccessModifier.Internal)] (CallbackResult result, byte[] data) =>
		{
			if (data == null)
			{
				_callback(result, default(T));
			}
			else
			{
				T val = BytesToObject<T>(data);
				if (val == null)
				{
					Log.Error($"[RPFS] Reading data into type {typeof(T)} yields malformed result.");
					result = CallbackResult.MalformedData;
					_callback(result, default(T));
				}
				else
				{
					_callback(result, val);
				}
			}
		});
	}

	sealed void WriteRemoteObject(string _filename, IRemotePlayerStorageObject _object, bool _overwriteCache, FileWriteCompleteCallback _callback)
	{
		if (_callback == null)
		{
			Log.Error("[RPFS] Write failed as no callback was supplied");
		}
		else
		{
			WriteRemoteData(_filename, ObjectToBytes(_object), _overwriteCache, _callback);
		}
	}

	static T BytesToObject<T>(byte[] _data) where T : IRemotePlayerStorageObject, new()
	{
		if (_data == null || _data.Length == 0)
		{
			Log.Warning("[RPFS] Byte data was empty or null.");
			return default(T);
		}
		try
		{
			using PooledExpandableMemoryStream pooledExpandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(_bReset: true);
			pooledExpandableMemoryStream.Write(_data, 0, _data.Length);
			pooledExpandableMemoryStream.Position = 0L;
			using PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: true);
			pooledBinaryReader.SetBaseStream(pooledExpandableMemoryStream);
			T result = new T();
			result.ReadInto(pooledBinaryReader);
			return result;
		}
		catch (Exception arg)
		{
			Log.Error($"[RPFS] Error while reading object from byte data. Error: {arg}.");
			return default(T);
		}
	}

	static byte[] ObjectToBytes(IRemotePlayerStorageObject _obj)
	{
		if (_obj == null)
		{
			Log.Warning("[RPFS] Object was null.");
			return null;
		}
		try
		{
			using PooledExpandableMemoryStream pooledExpandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(_bReset: true);
			using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: true))
			{
				pooledBinaryWriter.SetBaseStream(pooledExpandableMemoryStream);
				_obj.WriteFrom(pooledBinaryWriter);
				pooledBinaryWriter.Flush();
			}
			return pooledExpandableMemoryStream.ToArray();
		}
		catch (Exception arg)
		{
			Log.Error($"[RPFS] Error while writing object into byte data. Error: {arg}.");
			return null;
		}
	}

	static string GetCacheFolder(IUserClient _user)
	{
		if (_user == null || _user.UserStatus != EUserStatus.LoggedIn)
		{
			return null;
		}
		return GameIO.GetUserGameDataDir() + "/RfsPlayerCache/" + _user.PlatformUserId.ReadablePlatformUserIdentifier;
	}

	static T ReadCachedObject<T>(IUserClient _user, string _filename) where T : IRemotePlayerStorageObject, new()
	{
		return BytesToObject<T>(ReadCachedData(_user, _filename));
	}

	static byte[] ReadCachedData(IUserClient _user, string _filename)
	{
		lock (LocalCacheLock)
		{
			string path = GetCacheFolder(_user) + "/" + _filename;
			if (string.IsNullOrEmpty(_filename) || !SdFile.Exists(path))
			{
				Log.Warning("[RPFS] File path was not found.");
				return null;
			}
			return SdFile.ReadAllBytes(path);
		}
	}

	static bool WriteCachedObject(IUserClient _user, string _filename, IRemotePlayerStorageObject _object)
	{
		return WriteCachedObject(_user, _filename, ObjectToBytes(_object));
	}

	static bool WriteCachedObject(IUserClient _user, string _filename, byte[] _data)
	{
		lock (LocalCacheLock)
		{
			try
			{
				if (_data == null)
				{
					Log.Warning("[RPFS] Error while converting object to bytes.");
					return false;
				}
				SdDirectory.CreateDirectory(GetCacheFolder(_user));
				SdFile.WriteAllBytes(GetCacheFolder(_user) + "/" + _filename, _data);
				return true;
			}
			catch (Exception arg)
			{
				Log.Warning($"[RPFS] Error while writing object to cache. Error: {arg}.");
				return false;
			}
		}
	}

	static void ClearCache(IUserClient _user)
	{
		lock (LocalCacheLock)
		{
			if (SdDirectory.Exists(GetCacheFolder(_user)))
			{
				string[] files = SdDirectory.GetFiles(GetCacheFolder(_user));
				for (int i = 0; i < files.Length; i++)
				{
					SdFile.Delete(files[i]);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	static IRemotePlayerFileStorage()
	{
		LocalCacheLock = new object();
	}
}
