namespace Platform.XBL;

public class ApplicationStateController : IApplicationStateController
{
	public ApplicationState CurrentApplicationState => ApplicationState.Foreground;

	public bool NetworkConnectionState => true;

	public event IApplicationStateController.ApplicationStateChanged OnApplicationStateChanged;

	public event IApplicationStateController.NetworkStateChanged OnNetworkStateChanged;

	public void Init(IPlatform owner)
	{
	}

	public void Destroy()
	{
	}

	public void Update()
	{
	}
}
