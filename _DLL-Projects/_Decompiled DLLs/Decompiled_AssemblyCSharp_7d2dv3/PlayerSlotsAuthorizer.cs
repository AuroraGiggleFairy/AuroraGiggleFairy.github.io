using System;
using System.Collections.ObjectModel;
using UnityEngine.Scripting;

[Preserve]
public class PlayerSlotsAuthorizer : AuthorizerAbs
{
	public override int Order => 80;

	public override string AuthorizerName => "PlayerSlots";

	public override string StateLocalizationKey => null;

	public override (EAuthorizerSyncResult, GameUtils.KickPlayerData?) Authorize(ClientInfo _clientInfo)
	{
		AdminTools adminTools = GameManager.Instance.adminTools;
		int num = 0;
		int num2 = 0;
		int num3 = GamePrefs.GetInt(EnumGamePrefs.ServerMaxPlayerCount);
		int num4 = GamePrefs.GetInt(EnumGamePrefs.ServerReservedSlots);
		int num5 = GamePrefs.GetInt(EnumGamePrefs.ServerReservedSlotsPermission);
		ReadOnlyCollection<ClientInfo> list = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.List;
		for (int i = 0; i < list.Count; i++)
		{
			ClientInfo clientInfo = list[i];
			if (clientInfo != _clientInfo)
			{
				if ((adminTools?.Users.GetUserPermissionLevel(clientInfo) ?? 1000) <= num5)
				{
					num++;
				}
				else
				{
					num2++;
				}
			}
		}
		if (!GameManager.IsDedicatedServer)
		{
			num++;
		}
		bool flag = (adminTools?.Users.GetUserPermissionLevel(_clientInfo) ?? 1000) <= num5;
		bool flag2 = ((!flag) ? (num2 + num < num3 && num2 < num3 - num4) : (num2 + num < num3));
		if (!flag2)
		{
			int num6 = GamePrefs.GetInt(EnumGamePrefs.ServerAdminSlots);
			if (num6 > 0 && num2 + num < num3 + num6)
			{
				flag2 = (adminTools?.Users.GetUserPermissionLevel(_clientInfo) ?? 1000) <= GamePrefs.GetInt(EnumGamePrefs.ServerAdminSlotsPermission);
			}
		}
		if (!flag2)
		{
			string customReason;
			if (!flag && num2 + num < num3)
			{
				customReason = (num3 - num4).ToString();
				return (EAuthorizerSyncResult.SyncDeny, new GameUtils.KickPlayerData(GameUtils.EKickReason.PlayerLimitExceededNonVIP, 0, default(DateTime), customReason));
			}
			customReason = num3.ToString();
			return (EAuthorizerSyncResult.SyncDeny, new GameUtils.KickPlayerData(GameUtils.EKickReason.PlayerLimitExceeded, 0, default(DateTime), customReason));
		}
		return (EAuthorizerSyncResult.SyncAllow, null);
	}
}
