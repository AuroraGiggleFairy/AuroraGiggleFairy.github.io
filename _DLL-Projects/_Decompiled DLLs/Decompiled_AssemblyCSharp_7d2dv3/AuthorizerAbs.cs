using Platform;

public abstract class AuthorizerAbs : IAuthorizer
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public IAuthorizationResponses authResponsesHandler;

	public abstract int Order { get; }

	public abstract string AuthorizerName { get; }

	public abstract string StateLocalizationKey { get; }

	public virtual EPlatformIdentifier PlatformRestriction => EPlatformIdentifier.Count;

	public virtual bool AuthorizerActive => true;

	public virtual void Init(IAuthorizationResponses _authResponsesHandler)
	{
		authResponsesHandler = _authResponsesHandler;
	}

	public virtual void Cleanup()
	{
	}

	public virtual void ServerStart()
	{
	}

	public virtual void ServerStop()
	{
	}

	public abstract (EAuthorizerSyncResult, GameUtils.KickPlayerData?) Authorize(ClientInfo _clientInfo);

	public virtual void Disconnect(ClientInfo _clientInfo)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public AuthorizerAbs()
	{
	}
}
