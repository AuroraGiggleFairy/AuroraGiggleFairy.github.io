using System;
using UnityEngine.Scripting;

[Preserve]
public class LegacyModAuthorizer : AuthorizerAbs
{
	public override int Order => 150;

	public override string AuthorizerName => "LegacyModAuthorizations";

	public override string StateLocalizationKey => null;

	public override (EAuthorizerSyncResult, GameUtils.KickPlayerData?) Authorize(ClientInfo _clientInfo)
	{
		ModEvents.SPlayerLoginData _data = new ModEvents.SPlayerLoginData(_clientInfo, _clientInfo.compatibilityVersion);
		var (eModEventResult, mod) = ModEvents.PlayerLogin.Invoke(ref _data);
		if (eModEventResult == ModEvents.EModEventResult.StopHandlersAndVanilla)
		{
			Log.Out("Denying login from mod: " + mod.Name);
			string customMessage = _data.CustomMessage;
			return (EAuthorizerSyncResult.SyncDeny, new GameUtils.KickPlayerData(GameUtils.EKickReason.ModDecision, 0, default(DateTime), customMessage));
		}
		return (EAuthorizerSyncResult.SyncAllow, null);
	}
}
