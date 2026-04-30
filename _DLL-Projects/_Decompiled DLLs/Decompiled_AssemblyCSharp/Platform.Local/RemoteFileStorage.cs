using System;
using System.IO;

namespace Platform.Local;

public class RemoteFileStorage : IRemoteFileStorage
{
	public bool IsReady => true;

	public bool Unavailable => false;

	public void Init(IPlatform _owner)
	{
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
		string path = GameIO.GetApplicationPath() + "/fakeRemoteFileStorage/" + _filename;
		if (!File.Exists(path))
		{
			_callback(IRemoteFileStorage.EFileDownloadResult.FileNotFound, "", null);
			return;
		}
		try
		{
			byte[] data = File.ReadAllBytes(path);
			_callback(IRemoteFileStorage.EFileDownloadResult.Ok, null, data);
		}
		catch (Exception ex)
		{
			Log.Error("[Local] ReadFile (" + _filename + ") failed:");
			Log.Exception(ex);
			_callback(IRemoteFileStorage.EFileDownloadResult.Other, ex.Message, null);
		}
	}

	public void GetCachedFile(string _filename, IRemoteFileStorage.FileDownloadCompleteCallback _callback)
	{
		throw new NotImplementedException();
	}
}
