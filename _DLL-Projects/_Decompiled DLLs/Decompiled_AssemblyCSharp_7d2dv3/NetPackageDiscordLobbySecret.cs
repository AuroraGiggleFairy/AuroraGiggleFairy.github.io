using UnityEngine.Scripting;

[Preserve]
public class NetPackageDiscordLobbySecret : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public DiscordManager.ELobbyType lobbyType;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lobbySecret;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageDiscordLobbySecret Setup(DiscordManager.ELobbyType _lobbyType, string _lobbySecret)
	{
		lobbyType = _lobbyType;
		lobbySecret = _lobbySecret;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		lobbyType = (DiscordManager.ELobbyType)_reader.ReadByte();
		lobbySecret = StreamUtils.ReadString(_reader);
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write((byte)lobbyType);
		StreamUtils.Write(_writer, lobbySecret);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		DiscordManager.Instance.ReceivedLobbySecret(lobbyType, lobbySecret);
	}

	public override int GetLength()
	{
		return 3 + (lobbySecret?.Length ?? 0);
	}
}
