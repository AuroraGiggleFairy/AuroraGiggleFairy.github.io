using System;
using System.Text;
using UnityEngine.Scripting;

[Preserve]
public class LegacyModAuthorizer : AuthorizerAbs
{
	public override int Order => 150;

	public override string AuthorizerName => "LegacyModAuthorizations";

	public override string StateLocalizationKey => null;

	public override (EAuthorizerSyncResult, GameUtils.KickPlayerData?) Authorize(ClientInfo _clientInfo)
	{
		StringBuilder stringBuilder = new StringBuilder();
		Mod mod = ModEvents.PlayerLogin.Invoke(_clientInfo, _clientInfo.compatibilityVersion, stringBuilder);
		if (mod != null)
		{
			Log.Out("Denying login from mod: " + mod.Name);
			string customReason = stringBuilder.ToString();
			return (EAuthorizerSyncResult.SyncDeny, new GameUtils.KickPlayerData(GameUtils.EKickReason.ModDecision, 0, default(DateTime), customReason));
		}
		return (EAuthorizerSyncResult.SyncAllow, null);
	}
}
