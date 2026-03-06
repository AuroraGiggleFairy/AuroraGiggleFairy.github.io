using System;
using System.Collections.Generic;
using GUI_2;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TabSelector : XUiController
{
	public delegate void TabChangedDelegate(int _tabIndex, XUiC_TabSelectorTab _tab);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<XUiC_TabSelectorButton> tabButtons = new List<XUiC_TabSelectorButton>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<XUiC_TabSelectorTab> tabs = new List<XUiC_TabSelectorTab>();

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite tabSelectorBorder;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite tabHeaderBackground;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool enabled = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public int selectedTabIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool selectOnOpen;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool pendingOnOpenSelection;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool tabInputAllowed = true;

	public bool selectTabContentsOnChange = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool pendingOnChangeSelection;

	public bool isActiveTabSelector = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool usePageButtons;

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
			if (value >= tabs.Count)
			{
				value = tabs.Count - 1;
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
			else if (tabs[value].TabVisible)
			{
				if (SelectedTab != null)
				{
					SelectedTab.TabSelected = false;
				}
				selectedTabIndex = value;
				SelectedTab.TabSelected = true;
				updateTabVisibility();
				this.OnTabChanged?.Invoke(selectedTabIndex, SelectedTab);
				if (base.xui.playerUI.CursorController.navigationTarget != null && base.xui.playerUI.CursorController.navigationTarget.Controller.IsChildOf(this))
				{
					base.xui.playerUI.CursorController.SetNavigationTarget(null);
				}
				if (selectTabContentsOnChange)
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
			if (selectedTabIndex < 0 || selectedTabIndex >= tabs.Count)
			{
				return null;
			}
			return tabs[selectedTabIndex];
		}
		set
		{
			if (value != SelectedTab)
			{
				int num = tabs.IndexOf(value);
				if (num >= 0)
				{
					SelectedTabIndex = num;
				}
			}
		}
	}

	public XUiC_TabSelectorButton SelectedTabButton => SelectedTab?.TabButton;

	public event TabChangedDelegate OnTabChanged;

	public override void Init()
	{
		base.Init();
		XUiController childById = GetChildById("tabsHeader");
		if (childById == null)
		{
			Log.Error("[XUi] TabSelector without 'tabsHeader' in windowGroup '" + windowGroup.ID + "'");
			return;
		}
		XUiController childById2 = GetChildById("tabsContents");
		if (childById2 == null)
		{
			Log.Error("[XUi] TabSelector without 'tabsContents' in windowGroup '" + windowGroup.ID + "'");
			return;
		}
		childById.GetChildrenByType(tabButtons);
		childById2.GetChildrenByType(tabs);
		if (tabButtons.Count == 0)
		{
			Log.Error("[XUi] TabSelector without any TabSelectorButtons in 'tabsHeader' in windowGroup '" + windowGroup.ID + "'");
			return;
		}
		if (tabs.Count == 0)
		{
			Log.Error("[XUi] TabSelector without any TabSelectorTabs in 'tabsContent' in windowGroup '" + windowGroup.ID + "'");
			return;
		}
		for (int i = 0; i < tabs.Count; i++)
		{
			if (i >= tabButtons.Count)
			{
				Log.Warning($"More tabs ({tabs.Count}) than tab buttons ({tabButtons.Count}) in windowGroup '{base.WindowGroup.ID}'");
				break;
			}
			XUiC_TabSelectorTab xUiC_TabSelectorTab = tabs[i];
			XUiC_TabSelectorButton xUiC_TabSelectorButton = tabButtons[i];
			xUiC_TabSelectorButton.Tab = xUiC_TabSelectorTab;
			xUiC_TabSelectorTab.TabButton = xUiC_TabSelectorButton;
		}
		if (GetChildById("backgroundMainTabs")?.ViewComponent is XUiV_Sprite xUiV_Sprite)
		{
			tabHeaderBackground = xUiV_Sprite;
			if (GetChildById("border")?.ViewComponent is XUiV_Sprite xUiV_Sprite2)
			{
				tabSelectorBorder = xUiV_Sprite2;
				tabHeaderBackground.Sprite.rightAnchor.target = tabSelectorBorder.UiTransform;
				tabHeaderBackground.Sprite.rightAnchor.relative = 1f;
			}
		}
		tabs[0].TabSelected = true;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		updateTabVisibility();
		pendingOnOpenSelection = !selectOnOpen;
		pendingOnChangeSelection = false;
		base.xui.calloutWindow.AddCallout(usePageButtons ? UIUtils.ButtonIcon.LeftTrigger : UIUtils.ButtonIcon.LeftBumper, "igcoTabLeft", XUiC_GamepadCalloutWindow.CalloutType.Tabs);
		base.xui.calloutWindow.AddCallout(usePageButtons ? UIUtils.ButtonIcon.RightTrigger : UIUtils.ButtonIcon.RightBumper, "igcoTabRight", XUiC_GamepadCalloutWindow.CalloutType.Tabs);
		base.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Tabs);
		RefreshBindings();
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.Tabs);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateTabVisibility()
	{
		int num = -1;
		for (int i = 0; i < tabButtons.Count; i++)
		{
			if (i >= tabs.Count)
			{
				tabButtons[i].ViewComponent.IsVisible = false;
				continue;
			}
			bool tabVisible = tabs[i].TabVisible;
			bool isVisible = i == selectedTabIndex;
			tabButtons[i].ViewComponent.IsVisible = tabVisible;
			tabs[i].ViewComponent.IsVisible = isVisible;
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
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void ToggleCategory(int _dir, bool _playSound = true)
	{
		int num = selectedTabIndex;
		do
		{
			num = NGUIMath.RepeatIndex(num + _dir, tabs.Count);
		}
		while (!tabs[num].TabVisible && num != selectedTabIndex);
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
		bool flag = tabInputAllowed;
		if (base.xui.playerUI.CursorController.lockNavigationToView != null)
		{
			tabInputAllowed = IsChildOf(base.xui.playerUI.CursorController.lockNavigationToView.Controller);
		}
		else
		{
			tabInputAllowed = true;
		}
		if (tabInputAllowed != flag)
		{
			if (tabInputAllowed)
			{
				base.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Tabs);
			}
			else
			{
				base.xui.calloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Tabs);
			}
		}
		LocalPlayerUI playerUI = base.xui.playerUI;
		PlayerActionsGUI gUIActions = playerUI.playerInput.GUIActions;
		GUIWindowManager windowManager = playerUI.windowManager;
		if (tabInputAllowed && windowManager.IsKeyShortcutsAllowed() && isActiveTabSelector)
		{
			if (((!usePageButtons && gUIActions.WindowPagingLeft.WasReleased) || (usePageButtons && gUIActions.PageDown.WasReleased)) && windowManager.IsWindowOpen(windowGroup.ID))
			{
				ToggleCategory(-1);
			}
			if (((!usePageButtons && gUIActions.WindowPagingRight.WasReleased) || (usePageButtons && gUIActions.PageUp.WasReleased)) && windowManager.IsWindowOpen(windowGroup.ID))
			{
				ToggleCategory(1);
			}
		}
		XUiC_TabSelectorTab selectedTab = SelectedTab;
		if (selectedTab != null)
		{
			if (!pendingOnOpenSelection)
			{
				pendingOnOpenSelection = selectedTab.SelectCursorElement(_withDelay: true);
			}
			if (pendingOnChangeSelection && isActiveTabSelector)
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
		if (_index >= tabs.Count)
		{
			throw new ArgumentOutOfRangeException("_index");
		}
		tabs[_index].TabHeaderText = _name;
	}

	public XUiC_TabSelectorTab GetTab(int _index)
	{
		if (_index >= tabs.Count)
		{
			throw new ArgumentOutOfRangeException("_index");
		}
		return tabs[_index];
	}

	public void SelectTabByName(string _tabKey)
	{
		foreach (XUiC_TabSelectorTab tab in tabs)
		{
			if (!(tab.TabKey != _tabKey))
			{
				SelectedTab = tab;
				break;
			}
		}
	}

	public bool IsSelected(string _tabKey)
	{
		return SelectedTab?.TabKey.Equals(_tabKey) ?? false;
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		switch (_name)
		{
		case "select_tab_contents_on_open":
			if (!string.IsNullOrEmpty(_value))
			{
				selectOnOpen = StringParsers.ParseBool(_value);
			}
			return true;
		case "select_tab_contents_on_change":
			if (!string.IsNullOrEmpty(_value))
			{
				selectTabContentsOnChange = StringParsers.ParseBool(_value);
			}
			return true;
		case "use_page_buttons":
			usePageButtons = StringParsers.ParseBool(_value);
			return true;
		default:
			return base.ParseAttribute(_name, _value, _parent);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (_bindingName == "selected_tab")
		{
			_value = SelectedTab?.TabKey ?? "";
			return true;
		}
		return base.GetBindingValueInternal(ref _value, _bindingName);
	}

	public void TabVisibilityChanged(XUiC_TabSelectorTab _tab, bool _visible)
	{
		if (_tab == SelectedTab && !_visible)
		{
			ToggleCategory(-1, _playSound: false);
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
			SelectedTabIndex = tabButtons.IndexOf(_tabButton);
		}
	}
}
