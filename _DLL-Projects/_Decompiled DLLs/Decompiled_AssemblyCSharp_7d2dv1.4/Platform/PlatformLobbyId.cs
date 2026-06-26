using System.IO;
using System.Text;

namespace Platform;

public class PlatformLobbyId
{
	public static readonly PlatformLobbyId None = new PlatformLobbyId(EPlatformIdentifier.None, string.Empty);

	public readonly EPlatformIdentifier PlatformIdentifier;

	public readonly string LobbyId;

	public PlatformLobbyId(EPlatformIdentifier _platformId, string _lobbyId)
	{
		PlatformIdentifier = _platformId;
		LobbyId = _lobbyId;
	}

	public int GetWriteLength(Encoding encoding)
	{
		return 1 + LobbyId.GetBinaryWriterLength(encoding);
	}

	public void Write(BinaryWriter _writer)
	{
		_writer.Write((byte)PlatformIdentifier);
		if (PlatformIdentifier != EPlatformIdentifier.None)
		{
			_writer.Write(LobbyId);
		}
	}

	public static PlatformLobbyId Read(BinaryReader _reader)
	{
		byte num = _reader.ReadByte();
		string lobbyId = ((num != 0) ? _reader.ReadString() : string.Empty);
		return new PlatformLobbyId((EPlatformIdentifier)num, lobbyId);
	}
}
