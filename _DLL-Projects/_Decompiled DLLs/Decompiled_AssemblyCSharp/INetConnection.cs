using System.Collections.Generic;

public interface INetConnection
{
	void Disconnect(bool _kick);

	bool IsDisconnected();

	void GetPackages(List<NetPackage> _dstBuf);

	void AddToSendQueue(NetPackage _package);

	void AddToSendQueue(List<NetPackage> _packages);

	void FlushSendQueue();

	void AppendToReaderStream(byte[] _data, int _size);

	void UpgradeToFullConnection();

	NetConnectionStatistics GetStats();

	void SetEncryptionModule(IEncryptionModule _module);
}
