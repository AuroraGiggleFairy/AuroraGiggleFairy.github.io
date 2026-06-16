using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUIWindowConsoleComponents : MonoBehaviour
{
	public ScrollRect scrollRect;

	public Transform contentRect;

	public InputField commandField;

	public Button closeButton;

	public Button openLogsButton;

	public GameObject controllerPrompts;

	public GameObject consoleLinePrefab;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<GUIButtonPrompt> buttonPrompts;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		buttonPrompts = new List<GUIButtonPrompt>(GetComponentsInChildren<GUIButtonPrompt>());
	}

	public void RefreshButtonPrompts()
	{
		foreach (GUIButtonPrompt buttonPrompt in buttonPrompts)
		{
			buttonPrompt.RefreshIcon();
		}
	}
}
