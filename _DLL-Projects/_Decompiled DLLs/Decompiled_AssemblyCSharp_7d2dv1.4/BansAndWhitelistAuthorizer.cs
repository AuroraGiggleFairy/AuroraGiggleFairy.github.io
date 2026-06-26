using UnityEngine.Scripting;

[Preserve]
public class BansAndWhitelistAuthorizer : AuthorizerAbs
{
	public override int Order => 500;

	public override string AuthorizerName => "BansAndWhitelist";

	public override string StateLocalizationKey => null;

	public override bool AuthorizerActive => GameManager.Instance.adminTools != null;

	public override (EAuthorizerSyncResult, GameUtils.KickPlayerData?) Authorize(ClientInfo _clientInfo)
	{
		AdminTools adminTools = GameManager.Instance.adminTools;
		if (adminTools.Blacklist.IsBanned(_clientInfo.PlatformId, out var _bannedUntil, out var _reason))
		{
			return (EAuthorizerSyncResult.SyncDeny, new GameUtils.KickPlayerData(GameUtils.EKickReason.Banned, 0, _bannedUntil, _reason));
		}
		if (_clientInfo.CrossplatformId != null && adminTools.Blacklist.IsBanned(_clientInfo.CrossplatformId, out var _bannedUntil2, out var _reason2))
		{
			return (EAuthorizerSyncResult.SyncDeny, new GameUtils.KickPlayerData(GameUtils.EKickReason.Banned, 0, _bannedUntil2, _reason2));
		}
		if (adminTools.Whitelist.IsWhiteListEnabled() && !adminTools.Whitelist.IsWhitelisted(_clientInfo) && !adminTools.Users.HasEntry(_clientInfo))
		{
			return (EAuthorizerSyncResult.SyncDeny, new GameUtils.KickPlayerData(GameUtils.EKickReason.NotOnWhitelist));
		}
		return (EAuthorizerSyncResult.SyncAllow, null);
	}
}
