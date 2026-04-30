using System.IO;

namespace Platform;

public interface IRemotePlayerStorageObject
{
	void ReadInto(BinaryReader _reader);

	void WriteFrom(BinaryWriter _writer);
}
