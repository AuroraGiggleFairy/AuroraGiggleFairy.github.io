using UnityEngine;

public abstract class GUIWindowUGUI : GUIWindow
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public GameObject uiPrefab;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Canvas canvas;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool shouldOpen;

	public abstract string UIPrefabPath { get; }

	public GUIWindowUGUI(string _id)
		: base(_id)
	{
		uiPrefab = DataLoader.LoadAsset<GameObject>(UIPrefabPath);
		canvas = Object.Instantiate(uiPrefab).GetComponent<Canvas>();
		canvas.gameObject.SetActive(value: false);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (ThreadManager.IsMainThread())
		{
			canvas.gameObject.SetActive(value: true);
		}
		else
		{
			shouldOpen = true;
		}
	}

	public override void Update()
	{
		if (shouldOpen && !canvas.gameObject.activeSelf)
		{
			canvas.gameObject.SetActive(value: true);
			shouldOpen = false;
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		canvas.gameObject.SetActive(value: false);
	}

	public override void Cleanup()
	{
		Object.Destroy(canvas.gameObject);
		uiPrefab = null;
	}
}
