using UnityEngine.Scripting;

[Preserve]
public class PlayerIdAuthorizer : AuthorizerAbs
{
	public override int Order => 50;

	public override string AuthorizerName => "PlayerId";

	public override string StateLocalizationKey => null;

	public override (EAuthorizerSyncResult, GameUtils.KickPlayerData?) Authorize(ClientInfo _clientInfo)
	{
		if (_clientInfo.PlatformId == null)
		{
			return (EAuthorizerSyncResult.SyncDeny, new GameUtils.KickPlayerData(GameUtils.EKickReason.EmptyNameOrPlayerID));
		}
		return (EAuthorizerSyncResult.SyncAllow, null);
	}
}
