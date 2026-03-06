using Platform;
using UnityEngine.Scripting;

[Preserve]
public class FriendsAuthorizer : AuthorizerAbs
{
	public override int Order => 450;

	public override string AuthorizerName => "Friends";

	public override string StateLocalizationKey => null;

	public override bool AuthorizerActive
	{
		get
		{
			if (!GameManager.IsDedicatedServer)
			{
				return GamePrefs.GetInt(EnumGamePrefs.ServerVisibility) == 1;
			}
			return false;
		}
	}

	public override (EAuthorizerSyncResult, GameUtils.KickPlayerData?) Authorize(ClientInfo _clientInfo)
	{
		if (PlatformManager.NativePlatform.User.IsFriend(_clientInfo.PlatformId))
		{
			return (EAuthorizerSyncResult.SyncAllow, null);
		}
		DiscordManager.DiscordUser user;
		if (_clientInfo.DiscordUserId != 0 && (user = DiscordManager.Instance.GetUser(_clientInfo.DiscordUserId)) != null && user.IsFriend)
		{
			return (EAuthorizerSyncResult.SyncAllow, null);
		}
		return (EAuthorizerSyncResult.SyncDeny, new GameUtils.KickPlayerData(GameUtils.EKickReason.FriendsOnly));
	}
}
