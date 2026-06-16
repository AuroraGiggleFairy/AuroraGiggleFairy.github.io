using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TabSelector : XUiController
{
	public delegate void TabChangedDelegate(int _tabIndex, XUiC_TabSelectorTab _tab);

	[XuiBindComponent("tabsHeader.", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_TabSelectorButton[] tabButtons;

	[XuiBindComponent("tabsContents.", true)]
	public readonly XUiC_TabSelectorTab[] Tabs;

	[XuiBindComponent("tabsHeader.", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Table headerButtonsTable;

	[XuiBindComponent("border", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Sprite tabSelectorBorder;

	[XuiBindComponent("backgroundMainTabs", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Sprite tabHeaderBackground;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool enabled = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public int selectedTabIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool selectOnOpen;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool pendingOnOpenSelection;

	public bool SelectTabContentsOnChange = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool pendingOnChangeSelection;

	[XuiXmlAttribute("use_page_buttons", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool UsePageButtons
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public int SelectedTabIndex
	{
		get
		{
			return selectedTabIndex;
		}
		set
		{
			if (!enabled)
			{
				return;
			}
			if (value >= Tabs.Length)
			{
				value = Tabs.Length - 1;
			}
			if (selectedTabIndex == value)
			{
				return;
			}
			IsDirty = true;
			if (value < 0)
			{
				if (SelectedTab != null)
				{
					SelectedTab.TabSelected = false;
				}
				selectedTabIndex = value;
				updateTabVisibility();
				this.OnTabChanged?.Invoke(selectedTabIndex, SelectedTab);
			}
			else if (Tabs[value].TabVisible)
			{
				if (SelectedTab != null)
				{
					SelectedTab.TabSelected = false;
				}
				selectedTabIndex = value;
				SelectedTab.TabSelected = true;
				updateTabVisibility();
				this.OnTabChanged?.Invoke(selectedTabIndex, SelectedTab);
				if (xui.playerUI.CursorController.navigationTarget != null && xui.playerUI.CursorController.navigationTarget.Controller.IsChildOf(this))
				{
					xui.playerUI.CursorController.SetNavigationTarget(null);
				}
				if (SelectTabContentsOnChange)
				{
					pendingOnChangeSelection = true;
				}
			}
		}
	}

	public XUiC_TabSelectorTab SelectedTab
	{
		get
		{
			if (Tabs == null)
			{
				return null;
			}
			if (selectedTabIndex < 0 || selectedTabIndex >= Tabs.Length)
			{
				return null;
			}
			return Tabs[selectedTabIndex];
		}
		set
		{
			if (value != SelectedTab)
			{
				int num = Array.IndexOf(Tabs, value);
				if (num >= 0)
				{
					SelectedTabIndex = num;
				}
			}
		}
	}

	[XuiXmlBinding("selected_tab")]
	public string SelectedTabKey => SelectedTab?.TabKey ?? "";

	public XUiC_TabSelectorButton SelectedTabButton => SelectedTab?.TabButton;

	public event TabChangedDelegate OnTabChanged;

	public override void Init()
	{
		base.Init();
		if (tabButtons.Length == 0)
		{
			Log.Error("[XUi] TabSelector without any TabSelectorButtons in 'tabsHeader' in windowGroup '" + windowGroup.Id + "'");
			return;
		}
		if (Tabs.Length == 0)
		{
			Log.Error("[XUi] TabSelector without any TabSelectorTabs in 'tabsContent' in windowGroup '" + windowGroup.Id + "'");
			return;
		}
		for (int i = 0; i < Tabs.Length; i++)
		{
			if (i >= tabButtons.Length)
			{
				Log.Warning($"More tabs ({Tabs.Length}) than tab buttons ({tabButtons.Length}) in windowGroup '{base.WindowGroup.Id}'");
				break;
			}
			XUiC_TabSelectorTab xUiC_TabSelectorTab = Tabs[i];
			XUiC_TabSelectorButton xUiC_TabSelectorButton = tabButtons[i];
			xUiC_TabSelectorButton.Tab = xUiC_TabSelectorTab;
			xUiC_TabSelectorTab.TabButton = xUiC_TabSelectorButton;
		}
		if (tabHeaderBackground != null && tabSelectorBorder != null)
		{
			tabHeaderBackground.Sprite.rightAnchor.target = tabSelectorBorder.UiTransform;
			tabHeaderBackground.Sprite.rightAnchor.relative = 1f;
		}
		Tabs[0].TabSelected = true;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		updateTabVisibility();
		pendingOnOpenSelection = selectOnOpen;
		pendingOnChangeSelection = false;
		if (UsePageButtons)
		{
			xui.CalloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuCategory);
		}
		RefreshBindings();
	}

	public override void OnClose()
	{
		base.OnClose();
		xui.CalloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuCategory);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateTabVisibility()
	{
		int num = -1;
		for (int i = 0; i < tabButtons.Length; i++)
		{
			if (i >= Tabs.Length)
			{
				tabButtons[i].ViewComponent.IsVisible = false;
				continue;
			}
			bool tabVisible = Tabs[i].TabVisible;
			bool isVisible = i == selectedTabIndex;
			tabButtons[i].ViewComponent.IsVisible = tabVisible;
			Tabs[i].ViewComponent.IsVisible = isVisible;
			if (tabVisible)
			{
				num = i;
			}
		}
		if (tabHeaderBackground != null)
		{
			UIRect.AnchorPoint leftAnchor = tabHeaderBackground.Sprite.leftAnchor;
			if (num >= 0)
			{
				leftAnchor.target = tabButtons[num].OuterDimensionsTransform;
				leftAnchor.relative = 1f;
			}
			else
			{
				leftAnchor.target = tabSelectorBorder?.UiTransform;
				leftAnchor.relative = 0f;
			}
			tabHeaderBackground.Sprite.ResetAndUpdateAnchors();
		}
		headerButtonsTable?.Reposition();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void toggleCategory(int _dir, bool _playSound = true)
	{
		int num = selectedTabIndex;
		do
		{
			num = NGUIMath.RepeatIndex(num + _dir, Tabs.Length);
		}
		while (!Tabs[num].TabVisible && num != selectedTabIndex);
		SelectedTabIndex = num;
		if (!SelectedTab.TabVisible)
		{
			SelectedTabIndex = -1;
		}
		if (_playSound)
		{
			SelectedTabButton?.PlayClickSound();
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		bool num = XUiUtils.HotkeysAllowedFor(viewComponent);
		LocalPlayerUI playerUI = xui.playerUI;
		PlayerActionsGUI gUIActions = playerUI.playerInput.GUIActions;
		GUIWindowManager windowManager = playerUI.windowManager;
		if (num)
		{
			if (((!UsePageButtons && gUIActions.WindowPagingLeft.WasReleased) || (UsePageButtons && gUIActions.PageDown.WasReleased)) && windowManager.IsWindowOpen(windowGroup))
			{
				toggleCategory(-1);
			}
			if (((!UsePageButtons && gUIActions.WindowPagingRight.WasReleased) || (UsePageButtons && gUIActions.PageUp.WasReleased)) && windowManager.IsWindowOpen(windowGroup))
			{
				toggleCategory(1);
			}
		}
		XUiC_TabSelectorTab selectedTab = SelectedTab;
		if (selectedTab != null)
		{
			if (pendingOnOpenSelection)
			{
				selectedTab.SelectCursorElement(_withDelay: true);
				pendingOnOpenSelection = false;
			}
			if (pendingOnChangeSelection)
			{
				selectedTab.SelectCursorElement(_withDelay: true);
				pendingOnChangeSelection = false;
			}
		}
		if (IsDirty)
		{
			IsDirty = false;
			RefreshBindings();
		}
	}

	public void SetTabCaption(int _index, string _name)
	{
		if (_index >= Tabs.Length)
		{
			throw new ArgumentOutOfRangeException("_index");
		}
		Tabs[_index].TabHeaderText = _name;
	}

	public XUiC_TabSelectorTab GetTab(int _index)
	{
		if (_index >= Tabs.Length)
		{
			throw new ArgumentOutOfRangeException("_index");
		}
		return Tabs[_index];
	}

	public void SelectTabByName(string _tabKey)
	{
		XUiC_TabSelectorTab[] tabs = Tabs;
		foreach (XUiC_TabSelectorTab xUiC_TabSelectorTab in tabs)
		{
			if (!(xUiC_TabSelectorTab.TabKey != _tabKey))
			{
				SelectedTab = xUiC_TabSelectorTab;
				break;
			}
		}
	}

	public bool IsSelected(string _tabKey)
	{
		return SelectedTab?.TabKey.Equals(_tabKey) ?? false;
	}

	public void TabVisibilityChanged(XUiC_TabSelectorTab _tab, bool _visible)
	{
		if (_tab == SelectedTab && !_visible)
		{
			toggleCategory(-1, _playSound: false);
		}
		else if (_visible && SelectedTabIndex < 0)
		{
			SelectedTab = _tab;
		}
		updateTabVisibility();
	}

	public void TabButtonClicked(XUiC_TabSelectorButton _tabButton)
	{
		if (enabled)
		{
			SelectedTabIndex = Array.IndexOf(tabButtons, _tabButton);
		}
	}

	[XuiXmlAttribute("select_tab_contents_on_open", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeSelectTabContentsOnOpen(string _value)
	{
		if (!string.IsNullOrEmpty(_value))
		{
			selectOnOpen = StringParsers.ParseBool(_value);
		}
	}

	[XuiXmlAttribute("select_tab_contents_on_change", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeSelectTabContentsOnChange(string _value)
	{
		if (!string.IsNullOrEmpty(_value))
		{
			SelectTabContentsOnChange = StringParsers.ParseBool(_value);
		}
	}
}
