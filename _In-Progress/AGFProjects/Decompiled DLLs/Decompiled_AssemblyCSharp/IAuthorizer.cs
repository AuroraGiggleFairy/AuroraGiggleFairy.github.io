using Platform;

public interface IAuthorizer
{
	int Order { get; }

	string AuthorizerName { get; }

	string StateLocalizationKey { get; }

	EPlatformIdentifier PlatformRestriction { get; }

	bool AuthorizerActive { get; }

	void Init(IAuthorizationResponses _authResponsesHandler);

	void Cleanup();

	void ServerStart();

	void ServerStop();

	(EAuthorizerSyncResult, GameUtils.KickPlayerData?) Authorize(ClientInfo _clientInfo);

	void Disconnect(ClientInfo _clientInfo);
}
