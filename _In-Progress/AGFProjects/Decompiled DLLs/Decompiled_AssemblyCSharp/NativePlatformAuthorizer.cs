using System;
using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class NativePlatformAuthorizer : AuthorizerAbs
{
	public override int Order => 400;

	public override string AuthorizerName => "PlatformAuth";

	public override string StateLocalizationKey => "authstate_nativeplatform";

	public override void ServerStart()
	{
		base.ServerStart();
		foreach (KeyValuePair<EPlatformIdentifier, IPlatform> serverPlatform in PlatformManager.ServerPlatforms)
		{
			if (!serverPlatform.Value.IsCrossplatform)
			{
				serverPlatform.Value.AuthenticationServer?.StartServer(authPlayerSteamSuccessfulCallback, kickPlayerCallback);
			}
		}
	}

	public override void ServerStop()
	{
		base.ServerStop();
		foreach (KeyValuePair<EPlatformIdentifier, IPlatform> serverPlatform in PlatformManager.ServerPlatforms)
		{
			if (!serverPlatform.Value.IsCrossplatform)
			{
				serverPlatform.Value.AuthenticationServer?.StopServer();
			}
		}
	}

	public override (EAuthorizerSyncResult, GameUtils.KickPlayerData?) Authorize(ClientInfo _clientInfo)
	{
		EPlatformIdentifier platformIdentifier = _clientInfo.PlatformId.PlatformIdentifier;
		IPlatform platform = PlatformManager.InstanceForPlatformIdentifier(platformIdentifier);
		if (platform == null)
		{
			string customReason = platformIdentifier.ToStringCached();
			return (EAuthorizerSyncResult.SyncDeny, new GameUtils.KickPlayerData(GameUtils.EKickReason.UnsupportedPlatform, 0, default(DateTime), customReason));
		}
		if (platform.AuthenticationServer == null)
		{
			return (EAuthorizerSyncResult.SyncAllow, null);
		}
		EBeginUserAuthenticationResult eBeginUserAuthenticationResult = platform.AuthenticationServer.AuthenticateUser(_clientInfo);
		if (eBeginUserAuthenticationResult != EBeginUserAuthenticationResult.Ok)
		{
			return (EAuthorizerSyncResult.SyncDeny, new GameUtils.KickPlayerData(GameUtils.EKickReason.PlatformAuthenticationBeginFailed, (int)eBeginUserAuthenticationResult));
		}
		return (EAuthorizerSyncResult.WaitAsync, null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void authPlayerSteamSuccessfulCallback(ClientInfo _clientInfo)
	{
		authResponsesHandler.AuthorizationAccepted(this, _clientInfo);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void kickPlayerCallback(ClientInfo _cInfo, GameUtils.KickPlayerData _kickData)
	{
		authResponsesHandler.AuthorizationDenied(this, _cInfo, _kickData);
	}

	public override void Disconnect(ClientInfo _clientInfo)
	{
		if (_clientInfo != null && _clientInfo.PlatformId?.ReadablePlatformUserIdentifier != null)
		{
			PlatformManager.InstanceForPlatformIdentifier(_clientInfo.PlatformId.PlatformIdentifier)?.AuthenticationServer?.RemoveUser(_clientInfo);
		}
	}
}
