using Platform;
using UnityEngine.Scripting;

[Preserve]
public class ServerStateAuthorizer : AuthorizerAbs
{
	public override int Order => 30;

	public override string AuthorizerName => "ServerState";

	public override string StateLocalizationKey => null;

	public override (EAuthorizerSyncResult, GameUtils.KickPlayerData?) Authorize(ClientInfo _clientInfo)
	{
		IMasterServerAnnouncer serverListAnnouncer = PlatformManager.MultiPlatform.ServerListAnnouncer;
		if ((serverListAnnouncer != null && !serverListAnnouncer.GameServerInitialized) || !GameManager.Instance.gameStateManager.IsGameStarted())
		{
			return (EAuthorizerSyncResult.SyncDeny, new GameUtils.KickPlayerData(GameUtils.EKickReason.GameStillLoading));
		}
		if (GameStats.GetInt(EnumGameStats.GameState) == 2)
		{
			return (EAuthorizerSyncResult.SyncDeny, new GameUtils.KickPlayerData(GameUtils.EKickReason.GamePaused));
		}
		return (EAuthorizerSyncResult.SyncAllow, null);
	}
}
