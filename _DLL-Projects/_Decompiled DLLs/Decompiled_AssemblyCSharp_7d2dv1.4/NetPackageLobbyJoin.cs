using System.Text;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageLobbyJoin : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public PlatformLobbyId serverLobbyId;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageLobbyJoin Setup(PlatformLobbyId lobbyId)
	{
		serverLobbyId = lobbyId;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		serverLobbyId = PlatformLobbyId.Read(_br);
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		serverLobbyId.Write(_bw);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		ILobbyHost lobbyHost = PlatformManager.NativePlatform.LobbyHost;
		if (lobbyHost == null)
		{
			Log.Warning(string.Format("Unexpected {0}, no lobby host for {1}", "NetPackageLobbyJoin", PlatformManager.NativePlatform.PlatformIdentifier));
			return;
		}
		if (PlatformManager.NativePlatform.PlatformIdentifier != serverLobbyId.PlatformIdentifier)
		{
			Log.Warning(string.Format("Received {0} for different platform: {1}", "NetPackageLobbyJoin", serverLobbyId.PlatformIdentifier));
			return;
		}
		string lobbyId = lobbyHost.LobbyId;
		if (lobbyId != null && lobbyId.Equals(serverLobbyId.LobbyId))
		{
			Log.Out("Received NetPackageLobbyJoin with " + serverLobbyId.LobbyId + " but we're already in the lobby");
			return;
		}
		lobbyHost.JoinLobby(serverLobbyId.LobbyId, [PublicizedFrom(EAccessModifier.Internal)] (LobbyHostJoinResult joinResult) =>
		{
			if (!joinResult.success)
			{
				Log.Warning("Failed to join server requested lobby, this client may be out of sync with the native lobby");
			}
		});
	}

	public override int GetLength()
	{
		Encoding uTF = Encoding.UTF8;
		return serverLobbyId?.GetWriteLength(uTF) ?? 0;
	}
}
