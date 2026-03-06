using System;
using UnityEngine.Scripting;

[Preserve]
public class VersionAuthorizer : AuthorizerAbs
{
	public override int Order => 70;

	public override string AuthorizerName => "VersionCheck";

	public override string StateLocalizationKey => null;

	public override (EAuthorizerSyncResult, GameUtils.KickPlayerData?) Authorize(ClientInfo _clientInfo)
	{
		if (!string.Equals(Constants.cVersionInformation.LongStringNoBuild, _clientInfo.compatibilityVersion, StringComparison.Ordinal))
		{
			string longStringNoBuild = Constants.cVersionInformation.LongStringNoBuild;
			return (EAuthorizerSyncResult.SyncDeny, new GameUtils.KickPlayerData(GameUtils.EKickReason.VersionMismatch, 0, default(DateTime), longStringNoBuild));
		}
		return (EAuthorizerSyncResult.SyncAllow, null);
	}
}
