using Platform;
using UnityEngine.Scripting;

[Preserve]
public class CrossplayAuthorizer : AuthorizerAbs
{
	public override int Order => 550;

	public override string AuthorizerName => "Crossplay";

	public override string StateLocalizationKey => null;

	public override (EAuthorizerSyncResult, GameUtils.KickPlayerData?) Authorize(ClientInfo _clientInfo)
	{
		bool num = GamePrefs.GetBool(EnumGamePrefs.ServerAllowCrossplay);
		bool flag = PlatformManager.MultiPlatform.User?.Permissions.HasCrossplay() ?? true;
		if (num && flag)
		{
			return (EAuthorizerSyncResult.SyncAllow, null);
		}
		if (_clientInfo.device.ToPlayGroup() != DeviceFlag.StandaloneWindows.ToPlayGroup())
		{
			return (EAuthorizerSyncResult.SyncDeny, new GameUtils.KickPlayerData(GameUtils.EKickReason.CrossplayDisabled));
		}
		return (EAuthorizerSyncResult.SyncAllow, null);
	}
}
