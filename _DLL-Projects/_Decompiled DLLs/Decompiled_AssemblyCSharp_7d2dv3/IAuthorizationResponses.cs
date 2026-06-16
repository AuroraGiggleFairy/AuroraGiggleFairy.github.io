public interface IAuthorizationResponses
{
	void AuthorizationDenied(IAuthorizer _authorizer, ClientInfo _clientInfo, GameUtils.KickPlayerData _kickPlayerData);

	void AuthorizationAccepted(IAuthorizer _authorizer, ClientInfo _clientInfo);
}
