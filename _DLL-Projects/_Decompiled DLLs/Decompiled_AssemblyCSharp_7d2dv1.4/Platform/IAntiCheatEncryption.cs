using System.IO;

namespace Platform;

public interface IAntiCheatEncryption
{
	bool EncryptionAvailable();

	bool EncryptStream(ClientInfo _cInfo, MemoryStream _stream, long _startOffset);

	bool DecryptStream(ClientInfo _cInfo, MemoryStream _stream, long _startOffset);
}
