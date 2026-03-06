using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScreamerAlertsController : MonoBehaviour
{
	private static Dictionary<int, bool> playerAlertVisibility = new Dictionary<int, bool>();

	internal string screamerHordeAlertMessage = string.Empty;
	internal float hordeAlertEndTime;
	internal Vector3 hordeAlertPosition = Vector3.zero;

	public static ScreamerAlertsController Instance;

	public string screamerAlertMessage = string.Empty;

	public static bool CompassVisible = false;

	public static bool GetPlayerAlertVisibility(int entityId)
	{
		if (playerAlertVisibility.TryGetValue(entityId, out var value))
		{
			return value;
		}
		return true;
	}

	public static void SetPlayerAlertVisibility(int entityId, bool visible)
	{
		playerAlertVisibility[entityId] = visible;
	}

	private void Update()
	{
		UpdateAlertMessage();
		UpdateHordeAlert();
		// Set rect visible if either alert is present
		if (XUiC_ScreamerAlerts.Instance?.ViewComponent != null)
		{
			bool isVisible = !string.IsNullOrEmpty(screamerAlertMessage) || !string.IsNullOrEmpty(screamerHordeAlertMessage);
			XUiC_ScreamerAlerts.Instance.ViewComponent.IsVisible = isVisible;
			XUiC_ScreamerAlerts.Instance.RefreshBindingsSelfAndChildren();
		}
	}

	public void TriggerScreamerHordeAlert(Vector3 hordePosition)
	{
		hordeAlertPosition = hordePosition;
		hordeAlertEndTime = Time.time + 10f;
		UpdateHordeAlert();
	}

	private void UpdateHordeAlert()
	{
		EntityPlayer localPlayer = GameManager.Instance.World?.GetPrimaryPlayer();
		bool showHordeAlert = false;
		if (localPlayer != null && !localPlayer.IsDead() && Time.time <= hordeAlertEndTime)
		{
			if (Vector3.Distance(localPlayer.position, hordeAlertPosition) <= 120f)
			{
				showHordeAlert = true;
			}
		}
		screamerHordeAlertMessage = showHordeAlert ? Localization.Get("ScreamerAlert_Horde") : string.Empty;
		if (!showHordeAlert)
		{
			XUiC_ScreamerAlerts.Instance?.RefreshBindingsSelfAndChildren();
		}
	}

	private void Awake()
	{
		if ((Object)(object)Instance == (Object)null)
		{
			Instance = this;
		}
	}

	public void OnOpenCompassWindow()
	{
	}

	public void OnCloseCompassWindow()
	{
	}

	public void OnPlayerEventClearAlert()
	{
	}

	public void UpdateAlertMessage()
	{
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		Dictionary<Vector3, List<int>> alertData;
		if (GameManager.IsDedicatedServer)
		{
			alertData = ScreamerAlertManager.Instance.targetScreamerIds;
		}
		else if (ConnectionManager.Instance.IsClient)
		{
			alertData = ScreamerAlertManager.ClientTargetScreamerIds;
		}
		else
		{
			alertData = ScreamerAlertManager.Instance.targetScreamerIds;
		}
		bool flag = false;
		bool flag2 = false;
		_ = Time.time;
		EntityPlayer localPlayer = GameManager.Instance.World?.GetPrimaryPlayer();
		bool inRange = false;
		if (localPlayer != null && !localPlayer.IsDead())
		{
			foreach (KeyValuePair<Vector3, List<int>> targetScreamerId in alertData)
			{
				if (Vector3.Distance(localPlayer.position, targetScreamerId.Key) <= 120f)
				{
					inRange = true;
					break;
				}
			}
		}
		bool hasScreamer = alertData.Any(kvp => kvp.Value.Any(id => id != -1));
		bool hasScout = alertData.Any(kvp => kvp.Value.Contains(-1));
		string text = ((inRange && (hasScreamer || hasScout)) ? Localization.Get("ScreamerAlert_Scout") : "");
		if (string.IsNullOrEmpty(text))
		{
			if (!string.IsNullOrEmpty(screamerAlertMessage))
			{
				screamerAlertMessage = "";
				UpdateScreamerAlertUI();
				XUiC_ScreamerAlerts.Instance?.RefreshBindingsSelfAndChildren();
			}
		}
		else if (text != screamerAlertMessage)
		{
			screamerAlertMessage = text;
			UpdateScreamerAlertUI();
			XUiC_ScreamerAlerts.Instance?.RefreshBindingsSelfAndChildren();
		}
	}

	public void UpdateScreamerAlertUI()
	{
	}

	public string GetScreamerAlertMessage()
	{
		return screamerAlertMessage;
	}

	public string GetScreamerHordeAlertMessage()
	{
		return screamerHordeAlertMessage;
	}
}
