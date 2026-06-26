using Platform;
using UnityEngine.Scripting;

[Preserve]
public class FriendsAuthorizer : AuthorizerAbs
{
	public override int Order => 450;

	public override string AuthorizerName => "Friends";

	public override string StateLocalizationKey => null;

	public override EPlatformIdentifier PlatformRestriction => EPlatformIdentifier.Steam;

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
		if (!PlatformManager.NativePlatform.User.IsFriend(_clientInfo.PlatformId))
		{
			return (EAuthorizerSyncResult.SyncDeny, new GameUtils.KickPlayerData(GameUtils.EKickReason.FriendsOnly));
		}
		return (EAuthorizerSyncResult.SyncAllow, null);
	}
}
