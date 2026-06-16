using System;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class TooManyPlayerSlotsAuthorizer : AuthorizerAbs
{
	public override int Order => 81;

	public override string AuthorizerName => "PlayerSlots";

	public override string StateLocalizationKey => null;

	public override (EAuthorizerSyncResult, GameUtils.KickPlayerData?) Authorize(ClientInfo _clientInfo)
	{
		int num = GamePrefs.GetInt(EnumGamePrefs.ServerMaxPlayerCount);
		bool flag = true;
		EPlayGroup ePlayGroup = _clientInfo.device.ToPlayGroup();
		if (ePlayGroup == EPlayGroup.XBS || ePlayGroup == EPlayGroup.PS5)
		{
			flag = num <= 8;
		}
		if (!flag)
		{
			string customReason = 8.ToString();
			return (EAuthorizerSyncResult.SyncDeny, new GameUtils.KickPlayerData(GameUtils.EKickReason.PlatformPlayerLimitExceeded, 0, default(DateTime), customReason));
		}
		return (EAuthorizerSyncResult.SyncAllow, null);
	}
}
