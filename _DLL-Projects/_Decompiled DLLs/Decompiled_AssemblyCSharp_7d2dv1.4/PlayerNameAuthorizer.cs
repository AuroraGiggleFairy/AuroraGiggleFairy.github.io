using UnityEngine.Scripting;

[Preserve]
public class PlayerNameAuthorizer : AuthorizerAbs
{
	public override int Order => 20;

	public override string AuthorizerName => "PlayerName";

	public override string StateLocalizationKey => null;

	public override (EAuthorizerSyncResult, GameUtils.KickPlayerData?) Authorize(ClientInfo _clientInfo)
	{
		if (string.IsNullOrEmpty(_clientInfo.playerName))
		{
			return (EAuthorizerSyncResult.SyncDeny, new GameUtils.KickPlayerData(GameUtils.EKickReason.EmptyNameOrPlayerID));
		}
		return (EAuthorizerSyncResult.SyncAllow, null);
	}
}
