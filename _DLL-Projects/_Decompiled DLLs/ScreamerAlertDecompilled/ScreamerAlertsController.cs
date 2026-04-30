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
		if (((XUiC_ScreamerAlerts.Instance != null) ? XUiC_ScreamerAlerts.Instance.ViewComponent : null) != null)
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
		EntityPlayer entityPlayer = GameManager.Instance.World?.GetPrimaryPlayer();
		bool flag = false;
		if (entityPlayer != null && !entityPlayer.IsDead() && Time.time <= hordeAlertEndTime && Vector3.Distance(entityPlayer.position, hordeAlertPosition) <= 120f)
		{
			flag = true;
		}
		screamerHordeAlertMessage = ((!flag) ? string.Empty : Localization.Get("ScreamerAlert_Horde"));
		if (!flag && XUiC_ScreamerAlerts.Instance != null)
		{
			XUiC_ScreamerAlerts.Instance.RefreshBindingsSelfAndChildren();
		}
	}

	private void Awake()
	{
		if (Instance == null)
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
		Dictionary<Vector3, List<int>> dictionary = (GameManager.IsDedicatedServer ? ScreamerAlertManager.Instance.targetScreamerIds : ((!SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient) ? ScreamerAlertManager.Instance.targetScreamerIds : ScreamerAlertManager.ClientTargetScreamerIds));
		bool flag = false;
		bool flag2 = false;
		_ = Time.time;
		EntityPlayer entityPlayer = GameManager.Instance.World?.GetPrimaryPlayer();
		bool flag3 = false;
		if (entityPlayer != null && !entityPlayer.IsDead())
		{
			foreach (KeyValuePair<Vector3, List<int>> item in dictionary)
			{
				if (Vector3.Distance(entityPlayer.position, item.Key) <= 120f)
				{
					flag3 = true;
					break;
				}
			}
		}
		bool flag4 = dictionary.Any((KeyValuePair<Vector3, List<int>> kvp) => kvp.Value.Any((int id) => id != -1));
		bool flag5 = dictionary.Any((KeyValuePair<Vector3, List<int>> kvp) => kvp.Value.Contains(-1));
		string text = ((!flag3 || (!flag4 && !flag5)) ? "" : Localization.Get("ScreamerAlert_Scout"));
		if (string.IsNullOrEmpty(text))
		{
			if (!string.IsNullOrEmpty(screamerAlertMessage))
			{
				screamerAlertMessage = "";
				UpdateScreamerAlertUI();
				if (XUiC_ScreamerAlerts.Instance != null)
				{
					XUiC_ScreamerAlerts.Instance.RefreshBindingsSelfAndChildren();
				}
			}
		}
		else if (text != screamerAlertMessage)
		{
			screamerAlertMessage = text;
			UpdateScreamerAlertUI();
			if (XUiC_ScreamerAlerts.Instance != null)
			{
				XUiC_ScreamerAlerts.Instance.RefreshBindingsSelfAndChildren();
			}
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
