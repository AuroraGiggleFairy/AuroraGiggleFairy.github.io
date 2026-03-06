namespace Platform;

public interface IApplicationStateController
{
	public delegate void ApplicationStateChanged(ApplicationState newState);

	public delegate void NetworkStateChanged(bool connectionState);

	bool NetworkConnectionState { get; }

	ApplicationState CurrentApplicationState { get; }

	event ApplicationStateChanged OnApplicationStateChanged;

	event NetworkStateChanged OnNetworkStateChanged;

	void Init(IPlatform owner);

	void Destroy();

	void Update();
}
