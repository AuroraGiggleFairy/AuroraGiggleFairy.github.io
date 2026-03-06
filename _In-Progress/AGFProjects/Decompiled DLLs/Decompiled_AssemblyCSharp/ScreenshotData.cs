using UnityEngine;

public class ScreenshotData : MonoBehaviour
{
	public void Start()
	{
	}

	public void Update()
	{
	}

	public void OnGUI()
	{
		if (!(GameManager.Instance == null) && GameManager.Instance.World != null && !(GameManager.Instance.World.GetPrimaryPlayer() == null))
		{
			LocalPlayerUI uIForPrimaryPlayer = LocalPlayerUI.GetUIForPrimaryPlayer();
			if (!(uIForPrimaryPlayer == null) && uIForPrimaryPlayer.windowManager.IsHUDEnabled() && !GameManager.Instance.ShowBackground())
			{
				GUI.Label(new Rect(10f, 10f, 200f, 20f), GameManager.Instance.backgroundColor.ToCultureInvariantString());
			}
		}
	}
}
