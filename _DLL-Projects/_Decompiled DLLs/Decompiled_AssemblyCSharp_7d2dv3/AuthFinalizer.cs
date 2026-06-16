using UnityEngine.Scripting;

[Preserve]
public class AuthFinalizer : AuthorizerAbs
{
	public static AuthFinalizer Instance;

	public override int Order => 999;

	public override string AuthorizerName => "Finalizer";

	public override string StateLocalizationKey => null;

	public AuthFinalizer()
	{
		Instance = this;
	}

	public override (EAuthorizerSyncResult, GameUtils.KickPlayerData?) Authorize(ClientInfo _clientInfo)
	{
		_clientInfo.SendPackage(NetPackageManager.GetPackage<NetPackageAuthConfirmation>().Setup());
		return (EAuthorizerSyncResult.WaitAsync, null);
	}

	public void ReplyReceived(ClientInfo _cInfo)
	{
		authResponsesHandler.AuthorizationAccepted(this, _cInfo);
	}
}
