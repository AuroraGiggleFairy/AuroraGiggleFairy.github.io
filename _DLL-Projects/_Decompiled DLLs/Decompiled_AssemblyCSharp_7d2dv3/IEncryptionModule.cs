using System.IO;

public interface IEncryptionModule
{
	bool EncryptStream(ClientInfo _cInfo, MemoryStream _stream);

	bool DecryptStream(ClientInfo _cInfo, MemoryStream _stream);
}
