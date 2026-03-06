using System.Collections.Generic;
using UnityEngine;

public class GUIWindowWOChooseCategory : GUIWindow
{
	public static string ID = "GUIWindowWOChooseCategory";

	public GUIWindowWOChooseCategory()
		: base(ID, 0, 0, _bDrawBackground: true)
	{
	}

	public override void OnGUI(bool _inputActive)
	{
		base.OnGUI(_inputActive);
		GUILayout.BeginVertical();
		GUILayout.Space(10f);
		GUILayout.BeginHorizontal();
		GUILayout.Space(10f);
		GUILayout.Label(new GUIContent("Choose properties:"));
		GUILayout.EndHorizontal();
		GUILayout.Space(10f);
		foreach (KeyValuePair<string, SelectionCategory> category in SelectionBoxManager.Instance.GetCategories())
		{
			SelectionCategory value = category.Value;
			ISelectionBoxCallback callback = value.callback;
			if (callback != null && callback.OnSelectionBoxIsAvailable(value.name, EnumSelectionBoxAvailabilities.CanShowProperties))
			{
				GUILayout.BeginHorizontal();
				GUILayout.Space(20f);
				if (GUILayoutButton(value.name, GUILayout.Width(200f)))
				{
					value.callback.OnSelectionBoxShowProperties(_bVisible: true, windowManager);
				}
				GUILayout.EndHorizontal();
			}
		}
		GUILayout.EndVertical();
	}

	public override void OnOpen()
	{
		if (SelectionBoxManager.Instance.GetSelected(out var _selectedCategory, out var _))
		{
			foreach (KeyValuePair<string, SelectionCategory> category in SelectionBoxManager.Instance.GetCategories())
			{
				SelectionCategory value = category.Value;
				if (value.name.Equals(_selectedCategory))
				{
					ISelectionBoxCallback callback = value.callback;
					if (callback != null && callback.OnSelectionBoxIsAvailable(value.name, EnumSelectionBoxAvailabilities.CanShowProperties))
					{
						windowManager.Close(this);
						value.callback.OnSelectionBoxShowProperties(_bVisible: true, windowManager);
					}
					break;
				}
			}
		}
		int num = 0;
		foreach (KeyValuePair<string, SelectionCategory> category2 in SelectionBoxManager.Instance.GetCategories())
		{
			SelectionCategory value2 = category2.Value;
			ISelectionBoxCallback callback2 = value2.callback;
			if (callback2 != null && callback2.OnSelectionBoxIsAvailable(value2.name, EnumSelectionBoxAvailabilities.CanShowProperties))
			{
				num++;
			}
		}
		SetSize(240f, 50 + num * 30);
	}
}
