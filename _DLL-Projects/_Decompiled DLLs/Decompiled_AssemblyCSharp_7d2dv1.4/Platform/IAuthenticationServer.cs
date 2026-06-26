namespace Platform;

public interface IAuthenticationServer
{
	void Init(IPlatform _owner);

	EBeginUserAuthenticationResult AuthenticateUser(ClientInfo _cInfo);

	void RemoveUser(ClientInfo _cInfo);

	void StartServer(AuthenticationSuccessfulCallbackDelegate _authSuccessfulDelegate, KickPlayerDelegate _kickPlayerDelegate);

	void StartServerSteamGroups(SteamGroupStatusResponse _groupStatusResponseDelegate);

	void StopServer();

	bool RequestUserInGroupStatus(ClientInfo _cInfo, string _steamIdGroup);
}
