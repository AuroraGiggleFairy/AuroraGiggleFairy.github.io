using System.Text;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageLobbyRegisterClient : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public PlatformLobbyId lobbyId;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool overwriteExistingLobby;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToServer;

	public NetPackageLobbyRegisterClient Setup(PlatformLobbyId lobbyId, bool overwriteExistingLobby)
	{
		this.lobbyId = lobbyId;
		this.overwriteExistingLobby = overwriteExistingLobby;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		lobbyId = PlatformLobbyId.Read(_br);
		overwriteExistingLobby = _br.ReadBoolean();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		lobbyId.Write(_bw);
		_bw.Write(overwriteExistingLobby);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (lobbyId.PlatformIdentifier != PlatformManager.NativePlatform.PlatformIdentifier)
		{
			if (lobbyId.PlatformIdentifier != base.Sender.PlatformId.PlatformIdentifier)
			{
				Log.Warning(string.Format("Received {0} for lobby with platform {1} but client is from {2}. This is not permitted, lobby will not be registered", "NetPackageLobbyRegisterClient", lobbyId.PlatformIdentifier, base.Sender.PlatformId.PlatformIdentifier));
			}
			else
			{
				PlatformManager.ClientLobbyManager.RegisterLobbyClient(lobbyId, base.Sender, overwriteExistingLobby);
			}
		}
	}

	public override int GetLength()
	{
		Encoding uTF = Encoding.UTF8;
		return lobbyId?.GetWriteLength(uTF) ?? 0;
	}
}
