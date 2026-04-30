namespace Platform;

public interface IAntiCheatServer : IAntiCheatEncryption, IEncryptionModule
{
	void Init(IPlatform _owner);

	void Update();

	bool StartServer(AuthenticationSuccessfulCallbackDelegate _authSuccessfulDelegate, KickPlayerDelegate _kickPlayerDelegate);

	bool RegisterUser(ClientInfo _client);

	void FreeUser(ClientInfo _client);

	void HandleMessageFromClient(ClientInfo _cInfo, byte[] _data);

	void StopServer();

	void Destroy();

	bool ServerEacEnabled();

	bool ServerEacAvailable();

	bool GetHostUserIdAndToken(out (PlatformUserIdentifierAbs userId, string token) _hostUserIdAndToken);
}
