namespace Platform;

public interface IAuthenticationClient
{
	void Init(IPlatform _owner);

	string GetAuthTicket();

	void AuthenticateServer(ClientAuthenticateServerContext _context);

	void Destroy();
}
