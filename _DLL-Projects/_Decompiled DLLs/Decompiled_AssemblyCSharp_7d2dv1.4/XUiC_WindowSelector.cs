using System.Collections.Generic;
using Audio;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_WindowSelector : XUiController
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
		ID = base.WindowGroup.ID;
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
			xUiController.ViewComponent.IsNavigatable = (xUiController.ViewComponent.IsSnappable = false);
		}
		SetSelected("crafting");
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
		lblWindowName.Text = ((selected != null) ? Localization.Get("xui" + Selected.ID) : "");
	}

	public void OpenSelectedWindow()
	{
		if (Selected != null)
		{
			updateWindowTitle();
			string iD = Selected.ID;
			if (!base.xui.playerUI.windowManager.IsWindowOpen(iD))
			{
				base.xui.playerUI.windowManager.CloseAllOpenWindows("windowpaging");
				base.xui.playerUI.windowManager.Open(iD, _bModal: true);
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
		base.xui.dragAndDrop.InMenu = true;
		Manager.PlayInsidePlayerHead("open_inventory");
		RefreshBindings();
	}

	public override void OnClose()
	{
		base.OnClose();
		GameManager.Instance.SetPauseWindowEffects(_bOn: false);
		base.xui.dragAndDrop.InMenu = false;
		Manager.PlayInsidePlayerHead("close_inventory");
		if (base.xui.currentSelectedEntry != null)
		{
			base.xui.currentSelectedEntry.Selected = false;
		}
		(base.xui.playerUI.windowManager.GetWindow("toolbelt") as XUiWindowGroup)?.Controller.GetChildByType<XUiC_Toolbelt>()?.ClearHoveredItems();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void openSelectorAndWindow(string _selectedPage)
	{
		_selectedPage = _selectedPage.ToLower();
		XUiC_FocusedBlockHealth.SetData(base.xui.playerUI, null, 0f);
		if (base.xui.playerUI.windowManager.IsWindowOpen("windowpaging") && SelectedName.EqualsCaseInsensitive(_selectedPage) && !OverrideClose)
		{
			base.xui.playerUI.windowManager.CloseAllOpenWindows();
			if (base.xui.playerUI.windowManager.IsWindowOpen("windowpaging"))
			{
				base.xui.playerUI.windowManager.Close("windowpaging");
			}
			return;
		}
		SetSelected(_selectedPage);
		if (base.xui.playerUI.windowManager.IsWindowOpen("windowpaging"))
		{
			OpenSelectedWindow();
			return;
		}
		base.xui.playerUI.windowManager.CloseAllOpenWindows();
		base.xui.playerUI.windowManager.Open("windowpaging", _bModal: false);
	}

	public static void OpenSelectorAndWindow(EntityPlayerLocal _localPlayer, string selectedPage)
	{
		if (!_localPlayer.IsDead())
		{
			LocalPlayerUI.GetUIForPlayer(_localPlayer).xui.FindWindowGroupByName("windowpaging").GetChildByType<XUiC_WindowSelector>().openSelectorAndWindow(selectedPage);
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
		LocalPlayerUI.GetUIForPlayer(_localPlayer).xui.FindWindowGroupByName("windowpaging").GetChildByType<XUiC_WindowSelector>().toggleCategory(_dir);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		GUIWindowManager windowManager = base.xui.playerUI.windowManager;
		if (base.xui.playerUI.windowManager.IsKeyShortcutsAllowed())
		{
			if (base.xui.playerUI.playerInput.GUIActions.WindowPagingLeft.WasReleased && windowManager.IsWindowOpen(ID))
			{
				ToggleCategory(base.xui.playerUI.entityPlayer, -1);
			}
			if (base.xui.playerUI.playerInput.GUIActions.WindowPagingRight.WasReleased && windowManager.IsWindowOpen(ID))
			{
				ToggleCategory(base.xui.playerUI.entityPlayer, 1);
			}
		}
		OverrideClose = false;
	}
}
