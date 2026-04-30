using System;

namespace Platform;

public interface IAntiCheatClient : IAntiCheatEncryption, IEncryptionModule
{
	void Init(IPlatform _owner);

	bool ClientAntiCheatEnabled();

	bool GetUnhandledViolationMessage(out string _message);

	void WaitForRemoteAuth(Action onRemoteAuthSkippedOrComplete);

	void ConnectToServer((PlatformUserIdentifierAbs userId, string token) hostUserAndToken, Action onNoAntiCheatOrConnectionComplete, Action<string> onConnectionFailed);

	void HandleMessageFromServer(byte[] _data);

	void DisconnectFromServer();

	void Destroy();
}
