using UnityEngine.Scripting;

namespace Platform.Steam;

[Preserve]
public class SteamOwnerAuthorizer : AuthorizerAbs
{
	public override int Order => 430;

	public override string AuthorizerName => "SteamFamily";

	public override string StateLocalizationKey => null;

	public override EPlatformIdentifier PlatformRestriction => EPlatformIdentifier.Steam;

	public override (EAuthorizerSyncResult, GameUtils.KickPlayerData?) Authorize(ClientInfo _clientInfo)
	{
		UserIdentifierSteam userIdentifierSteam = (UserIdentifierSteam)_clientInfo.PlatformId;
		UserIdentifierSteam ownerId = userIdentifierSteam.OwnerId;
		if (GameManager.Instance.adminTools != null && ownerId != null && !userIdentifierSteam.Equals(ownerId) && GameManager.Instance.adminTools.Blacklist.IsBanned(ownerId, out var _bannedUntil, out var _reason))
		{
			return (EAuthorizerSyncResult.SyncDeny, new GameUtils.KickPlayerData(GameUtils.EKickReason.Banned, 0, _bannedUntil, _reason));
		}
		return (EAuthorizerSyncResult.SyncAllow, null);
	}
}
