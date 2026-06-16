using UnityEngine.Scripting;

[Preserve]
public class HostMpAllowedAuthorizer : AuthorizerAbs
{
	public override int Order => 41;

	public override string AuthorizerName => "MpHostAllowed";

	public override string StateLocalizationKey => null;

	public override (EAuthorizerSyncResult, GameUtils.KickPlayerData?) Authorize(ClientInfo _clientInfo)
	{
		if (!PermissionsManager.IsMultiplayerAllowed() || !PermissionsManager.CanHostMultiplayer())
		{
			return (EAuthorizerSyncResult.SyncDeny, new GameUtils.KickPlayerData(GameUtils.EKickReason.MultiplayerBlockedForHostAccount));
		}
		return (EAuthorizerSyncResult.SyncAllow, null);
	}
}
