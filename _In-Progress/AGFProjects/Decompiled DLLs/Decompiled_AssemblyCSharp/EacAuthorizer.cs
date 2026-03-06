using Platform;
using UnityEngine.Scripting;

[Preserve]
public class EacAuthorizer : AuthorizerAbs
{
	public override int Order => 600;

	public override string AuthorizerName => "EAC";

	public override string StateLocalizationKey => "authstate_eac";

	public override bool AuthorizerActive => PlatformManager.MultiPlatform.AntiCheatServer?.ServerEacEnabled() ?? false;

	public override void ServerStart()
	{
		base.ServerStart();
		PlatformManager.MultiPlatform.AntiCheatServer?.StartServer(authPlayerEacSuccessfulCallback, kickPlayerCallback);
	}

	public override void ServerStop()
	{
		base.ServerStop();
		PlatformManager.MultiPlatform.AntiCheatServer?.StopServer();
	}

	public override (EAuthorizerSyncResult, GameUtils.KickPlayerData?) Authorize(ClientInfo _clientInfo)
	{
		PlatformManager.MultiPlatform.AntiCheatServer?.RegisterUser(_clientInfo);
		return (EAuthorizerSyncResult.WaitAsync, null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void authPlayerEacSuccessfulCallback(ClientInfo _cInfo)
	{
		_cInfo.acAuthDone = true;
		authResponsesHandler.AuthorizationAccepted(this, _cInfo);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void kickPlayerCallback(ClientInfo _cInfo, GameUtils.KickPlayerData _kickData)
	{
		authResponsesHandler.AuthorizationDenied(this, _cInfo, _kickData);
	}

	public override void Disconnect(ClientInfo _clientInfo)
	{
		PlatformManager.MultiPlatform.AntiCheatServer?.FreeUser(_clientInfo);
	}
}
