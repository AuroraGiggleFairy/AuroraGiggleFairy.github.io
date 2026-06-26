namespace Platform;

public interface IRemoteFileStorage
{
	public enum EFileDownloadResult
	{
		Ok,
		EmptyFilename,
		FileNotFound,
		Other
	}

	public delegate void FileDownloadCompleteCallback(EFileDownloadResult _result, string _errorName, byte[] _data);

	bool IsReady { get; }

	bool Unavailable { get; }

	void Init(IPlatform _owner);

	void GetFile(string _filename, FileDownloadCompleteCallback _callback);

	void GetCachedFile(string _filename, FileDownloadCompleteCallback _callback);
}
