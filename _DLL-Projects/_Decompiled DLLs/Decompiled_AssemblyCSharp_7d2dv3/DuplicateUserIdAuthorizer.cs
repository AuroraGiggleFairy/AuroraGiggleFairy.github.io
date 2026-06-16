using System.Collections.ObjectModel;
using UnityEngine.Scripting;

[Preserve]
public class DuplicateUserIdAuthorizer : AuthorizerAbs
{
	public override int Order => 60;

	public override string AuthorizerName => "DuplicateUserId";

	public override string StateLocalizationKey => null;

	public override (EAuthorizerSyncResult, GameUtils.KickPlayerData?) Authorize(ClientInfo _clientInfo)
	{
		ReadOnlyCollection<ClientInfo> list = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.List;
		for (int i = 0; i < list.Count; i++)
		{
			ClientInfo clientInfo = list[i];
			if (clientInfo == _clientInfo)
			{
				continue;
			}
			if (!_clientInfo.PlatformId.Equals(clientInfo.PlatformId))
			{
				PlatformUserIdentifierAbs crossplatformId = _clientInfo.CrossplatformId;
				if (crossplatformId == null || !crossplatformId.Equals(clientInfo.CrossplatformId))
				{
					continue;
				}
			}
			GameUtils.KickPlayerForClientInfo(clientInfo, new GameUtils.KickPlayerData(GameUtils.EKickReason.DuplicatePlayerID));
			return (EAuthorizerSyncResult.SyncDeny, new GameUtils.KickPlayerData(GameUtils.EKickReason.DuplicatePlayerID));
		}
		return (EAuthorizerSyncResult.SyncAllow, null);
	}
}
