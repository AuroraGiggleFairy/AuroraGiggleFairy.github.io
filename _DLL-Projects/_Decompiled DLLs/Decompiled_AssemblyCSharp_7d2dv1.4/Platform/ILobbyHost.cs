using System;

namespace Platform;

public interface ILobbyHost
{
	string LobbyId { get; }

	bool IsInLobby { get; }

	bool AllowClientLobby => false;

	void Init(IPlatform _owner);

	void UpdateLobby(GameServerInfo _gameServerInfo);

	void JoinLobby(string _lobbyId, Action<LobbyHostJoinResult> _onComplete);

	void ExitLobby();

	void UpdateGameTimePlayers(ulong _time, int _players);

	[PublicizedFrom(EAccessModifier.Protected)]
	static void NotifyJoinedSession(string sessionId, bool overwriteHostLobby)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
		{
			PlatformLobbyId lobbyId = new PlatformLobbyId(PlatformManager.NativePlatform.PlatformIdentifier, sessionId);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageLobbyRegisterClient>().Setup(lobbyId, overwriteHostLobby));
		}
	}
}
