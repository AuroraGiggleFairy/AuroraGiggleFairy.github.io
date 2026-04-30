using UnityEngine;

public class ScreamerAlertPlayerDisconnectHandler : MonoBehaviour
{
	private void Awake()
	{
		ModEvents.PlayerDisconnected.RegisterHandler(OnPlayerDisconnected);
	}

	public void OnPlayerDisconnected(ref ModEvents.SPlayerDisconnectedData _data)
	{
		if ((object)ScreamerAlertsController.Instance != null)
		{
			ScreamerAlertsController.Instance.OnPlayerEventClearAlert();
		}
	}
}
