using System.Collections.Generic;
using UnityEngine;

public class GUICompList : GUIComp
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int selectedItemIndex = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lastSelectedItemIndex = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<GUIContent> listContent;

	[PublicizedFrom(EAccessModifier.Private)]
	public string boxStyle;

	[PublicizedFrom(EAccessModifier.Private)]
	public GUIStyle listStyle;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 scroll;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bScrollToSelection;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bInitStyleDone;

	public int SelectedItemIndex
	{
		get
		{
			return selectedItemIndex;
		}
		set
		{
			selectedItemIndex = value;
			lastSelectedItemIndex = value;
			bScrollToSelection = true;
		}
	}

	public string SelectedEntry
	{
		get
		{
			if (selectedItemIndex < 0 || selectedItemIndex >= listContent.Count)
			{
				return null;
			}
			return listContent[selectedItemIndex].text;
		}
	}

	public GUICompList(Rect _rect)
	{
		rect = _rect;
		boxStyle = "box";
		listContent = new List<GUIContent>();
	}

	public GUICompList(Rect _rect, string[] _listContent)
		: this(_rect)
	{
		foreach (string text in _listContent)
		{
			listContent.Add(new GUIContent(text));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initStyle()
	{
		listStyle = new GUIStyle("button");
		listStyle.fontSize = 12;
		listStyle.normal.textColor = Color.white;
		listStyle.alignment = TextAnchor.MiddleLeft;
		listStyle.fixedHeight = listStyle.fontSize + 9;
		listStyle.fontStyle = FontStyle.Normal;
		listStyle.normal.background = null;
		listStyle.padding.left = 2;
		listStyle.padding.right = 2;
		listStyle.padding.top = 0;
		listStyle.padding.bottom = 0;
		listStyle.margin = new RectOffset(0, 0, 0, 0);
		listStyle.hover.textColor = Color.yellow;
		bInitStyleDone = true;
	}

	public void AddLine(string _line)
	{
		listContent.Add(new GUIContent(_line));
	}

	public void RemoveSelectedEntry()
	{
		if (listContent.Count > 0 && selectedItemIndex != -1)
		{
			listContent.RemoveAt(selectedItemIndex);
		}
	}

	public void MoveSelectedEntryUp()
	{
		if (listContent.Count > 0 && selectedItemIndex > 0 && selectedItemIndex < listContent.Count)
		{
			GUIContent item = listContent[selectedItemIndex];
			RemoveSelectedEntry();
			selectedItemIndex--;
			listContent.Insert(selectedItemIndex, item);
		}
	}

	public void MoveSelectedEntryDown()
	{
		if (listContent.Count > 0 && selectedItemIndex < listContent.Count - 1)
		{
			GUIContent item = listContent[selectedItemIndex];
			RemoveSelectedEntry();
			selectedItemIndex++;
			listContent.Insert(selectedItemIndex, item);
		}
	}

	public void Clear()
	{
		listContent.Clear();
	}

	public override void OnGUI()
	{
		if (!bInitStyleDone)
		{
			initStyle();
		}
		if (bScrollToSelection)
		{
			scroll = new Vector2(0f, listStyle.fixedHeight * (float)selectedItemIndex);
			bScrollToSelection = false;
		}
		Rect rect = new Rect(base.rect.x, base.rect.y, base.rect.width - 18f, listStyle.fixedHeight * (float)listContent.Count);
		GUI.Box(base.rect, "", boxStyle);
		scroll = GUI.BeginScrollView(base.rect, scroll, rect);
		selectedItemIndex = GUI.SelectionGrid(rect, selectedItemIndex, listContent.ToArray(), 1, listStyle);
		GUI.EndScrollView();
	}

	public override void OnGUILayout()
	{
		if (!bInitStyleDone)
		{
			initStyle();
		}
		if (bScrollToSelection)
		{
			scroll = new Vector2(0f, listStyle.fixedHeight * (float)selectedItemIndex);
			bScrollToSelection = false;
		}
		GUILayout.BeginVertical("box", GUILayout.Width(rect.width));
		scroll = GUILayout.BeginScrollView(scroll, false, true, GUILayout.Width(rect.width), GUILayout.Height(rect.height));
		lastSelectedItemIndex = selectedItemIndex;
		selectedItemIndex = GUILayout.SelectionGrid(lastSelectedItemIndex, listContent.ToArray(), 1, listStyle, GUILayout.Width(rect.width - 18f));
		GUILayout.EndScrollView();
		GUILayout.EndVertical();
	}

	public bool OnListClicked()
	{
		return lastSelectedItemIndex != selectedItemIndex;
	}

	public bool SelectEntry(string _entry)
	{
		for (int i = 0; i < listContent.Count; i++)
		{
			if (listContent[i].text.Equals(_entry))
			{
				SelectedItemIndex = i;
				return true;
			}
		}
		return false;
	}
}
