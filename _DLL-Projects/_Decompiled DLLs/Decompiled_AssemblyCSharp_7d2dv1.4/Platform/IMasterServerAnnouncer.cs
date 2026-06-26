using System;

namespace Platform;

public interface IMasterServerAnnouncer
{
	bool GameServerInitialized { get; }

	void Init(IPlatform _owner);

	void Update();

	string GetServerPorts();

	void AdvertiseServer(Action _onServerRegistered);

	void StopServer();
}
