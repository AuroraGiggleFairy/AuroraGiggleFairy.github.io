using System;

namespace Platform;

public interface IPlatformApi
{
	EApiStatus ClientApiStatus { get; }

	event Action ClientApiInitialized;

	void Init(IPlatform _owner);

	bool InitClientApis();

	bool InitServerApis();

	void ServerApiLoaded();

	void Update();

	void Destroy();

	float GetScreenBoundsValueFromSystem();
}
