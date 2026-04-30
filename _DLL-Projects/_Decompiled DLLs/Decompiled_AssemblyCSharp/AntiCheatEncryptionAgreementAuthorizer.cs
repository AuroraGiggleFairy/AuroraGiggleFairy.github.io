using Platform;
using UnityEngine.Scripting;

[Preserve]
public class AntiCheatEncryptionAgreementAuthorizer : AuthorizerAbs
{
	public override int Order => 601;

	public override string AuthorizerName => "Encryption";

	public override string StateLocalizationKey => "authstate_encryption";

	public override void ServerStart()
	{
		base.ServerStart();
		SingletonMonoBehaviour<ConnectionManager>.Instance.AntiCheatEncryptionAuthServer.Start(KeyExchangeCompleted, KeyExchangeFailed);
	}

	public override (EAuthorizerSyncResult, GameUtils.KickPlayerData?) Authorize(ClientInfo _clientInfo)
	{
		IAntiCheatServer antiCheatServer = PlatformManager.MultiPlatform.AntiCheatServer;
		if (antiCheatServer != null && antiCheatServer.EncryptionAvailable() && _clientInfo.requiresAntiCheat)
		{
			if (_clientInfo.acAuthDone)
			{
				_clientInfo.SetAntiCheatEncryption(PlatformManager.MultiPlatform.AntiCheatServer);
			}
			else
			{
				Log.Warning("Server EAC AntiCheat encryption is available but " + _clientInfo.playerName + " did not complete EAC auth, encryption is disabled for this client");
			}
			return (EAuthorizerSyncResult.SyncAllow, null);
		}
		if (!GameManager.IsDedicatedServer)
		{
			return (EAuthorizerSyncResult.SyncAllow, null);
		}
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.AntiCheatEncryptionAuthServer.TryStartKeyExchange(_clientInfo))
		{
			return (EAuthorizerSyncResult.SyncAllow, null);
		}
		return (EAuthorizerSyncResult.WaitAsync, null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void KeyExchangeCompleted(ClientInfo _clientInfo, IEncryptionModule _encryptionModule)
	{
		_clientInfo.SetAntiCheatEncryption(_encryptionModule);
		authResponsesHandler.AuthorizationAccepted(this, _clientInfo);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void KeyExchangeFailed(ClientInfo _clientInfo, GameUtils.KickPlayerData _reason)
	{
		authResponsesHandler.AuthorizationDenied(this, _clientInfo, _reason);
	}

	public override void ServerStop()
	{
		base.ServerStop();
		SingletonMonoBehaviour<ConnectionManager>.Instance.AntiCheatEncryptionAuthServer.Stop();
	}
}
