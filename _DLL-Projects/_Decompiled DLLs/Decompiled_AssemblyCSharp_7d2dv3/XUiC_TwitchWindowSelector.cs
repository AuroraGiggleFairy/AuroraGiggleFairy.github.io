using System.Collections.Generic;
using Audio;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TwitchWindowSelector : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblWindowName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button selected;

	public static string ID = "";

	public bool OverrideClose;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<string> categories = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentCategoryIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool extras = false;

	public new XUiV_Button Selected
	{
		get
		{
			return selected;
		}
		set
		{
			if (selected != null)
			{
				selected.Selected = false;
			}
			selected = value;
			if (selected != null)
			{
				selected.Selected = true;
				HandleSelectedChange();
			}
		}
	}

	public string SelectedName
	{
		get
		{
			if (selected == null)
			{
				return "";
			}
			return selected.ID;
		}
	}

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.Id;
		XUiController childById = GetChildById("lblWindowName");
		if (childById != null)
		{
			lblWindowName = (XUiV_Label)childById.ViewComponent;
		}
		categories.Clear();
		for (int i = 0; i < children.Count; i++)
		{
			XUiController xUiController = children[i];
			if (xUiController.ViewComponent.EventOnPress)
			{
				xUiController.OnPress += HandleOnPress;
				categories.Add(xUiController.ViewComponent.ID.ToLower());
			}
		}
		SetSelected("actions");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnPress(XUiController _sender, int _mouseButton)
	{
		Selected = (XUiV_Button)_sender.ViewComponent;
		OpenSelectedWindow();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleSelectedChange()
	{
		updateWindowTitle();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateWindowTitle()
	{
		lblWindowName.Text = ((selected != null) ? Localization.Get("TwitchInfo_" + Selected.ID) : "");
	}

	public void OpenSelectedWindow()
	{
		if (Selected != null)
		{
			updateWindowTitle();
			string iD = Selected.ID;
			GUIWindowManager windowManager = xui.playerUI.windowManager;
			switch (iD)
			{
			case "Actions":
				xui.FindWindowGroupByName("twitchInfo").GetChildByType<XUiC_TwitchEntryListWindow>().SetOpenToActions(extras);
				extras = false;
				windowManager.Open("twitchInfo", _bModal: true);
				break;
			case "Votes":
				xui.FindWindowGroupByName("twitchInfo").GetChildByType<XUiC_TwitchEntryListWindow>().SetOpenToVotes();
				windowManager.Open("twitchInfo", _bModal: true);
				break;
			case "ActionHistory":
				xui.FindWindowGroupByName("twitchInfo").GetChildByType<XUiC_TwitchEntryListWindow>().SetOpenToHistory();
				windowManager.Open("twitchInfo", _bModal: true);
				break;
			case "Leaderboard":
				xui.FindWindowGroupByName("twitchInfo").GetChildByType<XUiC_TwitchEntryListWindow>().SetOpenToLeaderboard();
				windowManager.Open("twitchInfo", _bModal: true);
				break;
			}
		}
	}

	public void SetSelected(string name)
	{
		XUiController childById = GetChildById(name.ToLower());
		if (childById?.ViewComponent is XUiV_Button)
		{
			Selected = (XUiV_Button)childById.ViewComponent;
			currentCategoryIndex = categories.IndexOf(Selected.ID.ToLower());
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		OpenSelectedWindow();
		xui.DragAndDropWindow.InMenu = true;
		Manager.PlayInsidePlayerHead("open_inventory");
		RefreshBindings();
	}

	public override void OnClose()
	{
		base.OnClose();
		xui.DragAndDropWindow.InMenu = false;
		Manager.PlayInsidePlayerHead("close_inventory");
		if (xui.CurrentSelectedEntry != null)
		{
			xui.CurrentSelectedEntry.IsSelected = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void openSelectorAndWindow(string _selectedPage)
	{
		_selectedPage = _selectedPage.ToLower();
		XUiC_FocusedBlockHealth.SetData(xui.playerUI, null, 0f);
		if (xui.playerUI.windowManager.IsWindowOpen("twitchWindowpaging") && SelectedName.EqualsCaseInsensitive(_selectedPage) && !OverrideClose)
		{
			xui.playerUI.windowManager.CloseAllOpenModalWindows();
			if (xui.playerUI.windowManager.IsWindowOpen("twitchWindowpaging"))
			{
				xui.playerUI.windowManager.Close("twitchWindowpaging");
			}
			return;
		}
		SetSelected(_selectedPage);
		if (xui.playerUI.windowManager.IsWindowOpen("twitchWindowpaging"))
		{
			OpenSelectedWindow();
			return;
		}
		xui.playerUI.windowManager.CloseAllOpenModalWindows();
		xui.playerUI.windowManager.Open("twitchWindowpaging", _bModal: false);
	}

	public static void OpenSelectorAndWindow(EntityPlayerLocal _localPlayer, string _selectedPage, bool _extras = false)
	{
		if (!_localPlayer.IsDead())
		{
			extras = _extras;
			LocalPlayerUI.GetUIForPlayer(_localPlayer).xui.FindWindowGroupByName("twitchWindowpaging").GetChildByType<XUiC_TwitchWindowSelector>().openSelectorAndWindow(_selectedPage);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void toggleCategory(int _dir)
	{
		int index = NGUIMath.RepeatIndex(currentCategoryIndex + _dir, categories.Count);
		XUiController childById = GetChildById(categories[index]);
		if (childById?.ViewComponent is XUiV_Button)
		{
			if (childById.ViewComponent.IsVisible)
			{
				SetSelected(categories[index]);
				OpenSelectedWindow();
			}
			else
			{
				currentCategoryIndex = index;
				toggleCategory(_dir);
			}
		}
	}

	public static void ToggleCategory(EntityPlayerLocal _localPlayer, int _dir)
	{
		LocalPlayerUI.GetUIForPlayer(_localPlayer).xui.FindWindowGroupByName("twitchWindowpaging").GetChildByType<XUiC_TwitchWindowSelector>().toggleCategory(_dir);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		GUIWindowManager windowManager = xui.playerUI.windowManager;
		if (xui.playerUI.windowManager.IsKeyShortcutsAllowed())
		{
			if (xui.playerUI.playerInput.GUIActions.WindowPagingLeft.WasReleased && windowManager.IsWindowOpen(ID))
			{
				ToggleCategory(xui.playerUI.entityPlayer, -1);
			}
			if (xui.playerUI.playerInput.GUIActions.WindowPagingRight.WasReleased && windowManager.IsWindowOpen(ID))
			{
				ToggleCategory(xui.playerUI.entityPlayer, 1);
			}
		}
		OverrideClose = false;
	}
}
