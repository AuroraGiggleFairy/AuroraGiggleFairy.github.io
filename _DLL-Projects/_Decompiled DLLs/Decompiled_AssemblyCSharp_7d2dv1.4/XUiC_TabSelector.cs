using System;
using System.Collections.Generic;
using GUI_2;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TabSelector : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<XUiC_SimpleButton> tabButtons = new List<XUiC_SimpleButton>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<XUiV_Rect> tabs = new List<XUiV_Rect>();

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite tabHeaderBackground;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController content;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool enabled = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public int selectedTabIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public int tabCount;

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

	public bool Enabled
	{
		get
		{
			return enabled;
		}
		set
		{
			if (enabled != value)
			{
				enabled = value;
				updateTabVisibility();
				RefreshBindings(_forceAll: true);
			}
		}
	}

	public int SelectedTabIndex
	{
		get
		{
			return selectedTabIndex;
		}
		set
		{
			if (enabled && selectedTabIndex != value)
			{
				if (value < 0)
				{
					value = 0;
				}
				else if (value >= tabCount)
				{
					value = tabCount - 1;
				}
				selectedTabIndex = value;
				updateTabVisibility();
				if (this.OnTabChanged != null)
				{
					this.OnTabChanged(selectedTabIndex, tabButtons[selectedTabIndex].Text);
				}
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

	public int TabCount
	{
		get
		{
			return tabCount;
		}
		set
		{
			if (tabCount != value)
			{
				if (value >= tabs.Count)
				{
					value = tabs.Count;
				}
				if (value >= tabButtons.Count)
				{
					value = tabButtons.Count;
				}
				tabCount = value;
				if (tabHeaderBackground != null)
				{
					tabHeaderBackground.Sprite.leftAnchor.target = tabButtons[value - 1].ViewComponent.UiTransform.Find("border");
					tabHeaderBackground.Sprite.leftAnchor.relative = 1f;
				}
			}
		}
	}

	public event Action<int, string> OnTabChanged;

	public override void Init()
	{
		base.Init();
		XUiC_SimpleButton[] childrenByType = GetChildById("tabsHeader").GetChildrenByType<XUiC_SimpleButton>();
		foreach (XUiC_SimpleButton xUiC_SimpleButton in childrenByType)
		{
			tabButtons.Add(xUiC_SimpleButton);
			xUiC_SimpleButton.OnPressed += HandleOnPress;
			xUiC_SimpleButton.Button.Type = UIBasicSprite.Type.Advanced;
			xUiC_SimpleButton.Button.Sprite.bottomType = UIBasicSprite.AdvancedType.Invisible;
			xUiC_SimpleButton.Button.Sprite.rightType = UIBasicSprite.AdvancedType.Invisible;
			xUiC_SimpleButton.Button.Sprite.leftType = UIBasicSprite.AdvancedType.Invisible;
			XUiV_Button button = xUiC_SimpleButton.Button;
			bool isSnappable = (xUiC_SimpleButton.Button.IsNavigatable = false);
			button.IsSnappable = isSnappable;
		}
		content = GetChildById("tabsContents");
		foreach (XUiController child in content.Children)
		{
			if (child.ViewComponent is XUiV_Rect item)
			{
				tabs.Add(item);
			}
		}
		for (int j = 0; j < tabs.Count; j++)
		{
			if (j >= tabButtons.Count)
			{
				Log.Warning("More tabs (" + tabs.Count + ") than tab buttons (" + tabButtons.Count + ") in " + base.WindowGroup.ID);
				break;
			}
			if (tabs[j].Controller.CustomAttributes.ContainsKey("tab_key"))
			{
				tabButtons[j].Text = Localization.Get(tabs[j].Controller.CustomAttributes["tab_key"]);
			}
			tabButtons[j].Tooltip = tabs[j].ToolTip;
		}
		XUiController childById = GetChildById("backgroundMainTabs");
		if (childById != null)
		{
			tabHeaderBackground = childById.ViewComponent as XUiV_Sprite;
			XUiController childById2 = GetChildById("border");
			if (childById2 != null)
			{
				Transform uiTransform = childById2.ViewComponent.UiTransform;
				tabHeaderBackground.Sprite.rightAnchor.target = uiTransform;
				tabHeaderBackground.Sprite.rightAnchor.relative = 1f;
			}
		}
		TabCount = tabs.Count;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		for (int i = tabCount; i < tabButtons.Count; i++)
		{
			tabButtons[i].Parent.ViewComponent.IsVisible = false;
		}
		updateTabVisibility();
		pendingOnOpenSelection = !selectOnOpen;
		pendingOnChangeSelection = false;
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.LeftBumper, "igcoTabLeft", XUiC_GamepadCalloutWindow.CalloutType.Tabs);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.RightBumper, "igcoTabRight", XUiC_GamepadCalloutWindow.CalloutType.Tabs);
		base.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Tabs);
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.Tabs);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnPress(XUiController _sender, int _mouseButton)
	{
		if (enabled)
		{
			SelectedTabIndex = tabButtons.IndexOf(_sender as XUiC_SimpleButton);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateTabVisibility()
	{
		for (int i = 0; i < tabs.Count && i < tabButtons.Count; i++)
		{
			tabButtons[i].Button.Selected = i == selectedTabIndex;
			tabButtons[i].Button.IsVisible = enabled;
			tabButtons[i].Label.IsVisible = enabled;
			tabs[i].IsVisible = enabled && i == selectedTabIndex;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void ToggleCategory(int _dir)
	{
		int num = NGUIMath.RepeatIndex(selectedTabIndex + _dir, tabCount);
		SelectedTabIndex = num;
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
			if (gUIActions.WindowPagingLeft.WasReleased && windowManager.IsWindowOpen(windowGroup.ID))
			{
				ToggleCategory(-1);
			}
			if (gUIActions.WindowPagingRight.WasReleased && windowManager.IsWindowOpen(windowGroup.ID))
			{
				ToggleCategory(1);
			}
		}
		if (!pendingOnOpenSelection)
		{
			pendingOnOpenSelection = tabs[selectedTabIndex].Controller.SelectCursorElement(_withDelay: true);
		}
		if (pendingOnChangeSelection && isActiveTabSelector)
		{
			tabs[selectedTabIndex].Controller.SelectCursorElement(_withDelay: true);
			pendingOnChangeSelection = false;
		}
	}

	public string GetTabCaption(int _index)
	{
		if (_index >= tabCount)
		{
			throw new ArgumentOutOfRangeException("_index");
		}
		return tabButtons[_index].Text;
	}

	public void SetTabCaption(int _index, string _name)
	{
		if (_index >= tabCount)
		{
			throw new ArgumentOutOfRangeException("_index");
		}
		tabButtons[_index].Text = _name;
	}

	public string GetTabTooltip(int _index)
	{
		if (_index >= tabCount)
		{
			throw new ArgumentOutOfRangeException("_index");
		}
		return tabButtons[_index].Tooltip;
	}

	public void SetTabTooltip(int _index, string _name)
	{
		if (_index >= tabCount)
		{
			throw new ArgumentOutOfRangeException("_index");
		}
		tabButtons[_index].Tooltip = _name;
	}

	public XUiV_Rect GetTabRect(int _index)
	{
		if (_index >= tabCount)
		{
			throw new ArgumentOutOfRangeException("_index");
		}
		return tabs[_index];
	}

	public XUiC_SimpleButton GetTabButton(int _index)
	{
		if (_index >= tabCount)
		{
			throw new ArgumentOutOfRangeException("_index");
		}
		return tabButtons[_index];
	}

	public bool IsSelected(string _tabKey)
	{
		if (SelectedTabIndex >= 0 && tabs.Count > 0)
		{
			if (tabs[SelectedTabIndex].Controller.CustomAttributes.TryGetValue("tab_key", out var value))
			{
				return value.Equals(_tabKey);
			}
			return false;
		}
		return false;
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		if (!(_name == "select_tab_contents_on_open"))
		{
			if (_name == "select_tab_contents_on_change")
			{
				if (!string.IsNullOrEmpty(_value))
				{
					selectTabContentsOnChange = StringParsers.ParseBool(_value);
				}
				return true;
			}
			return base.ParseAttribute(_name, _value, _parent);
		}
		if (!string.IsNullOrEmpty(_value))
		{
			selectOnOpen = StringParsers.ParseBool(_value);
		}
		return true;
	}

	public override bool GetBindingValue(ref string _value, string _bindingName)
	{
		if (_bindingName == "tabsenabled")
		{
			_value = enabled.ToString();
			return true;
		}
		return base.GetBindingValue(ref _value, _bindingName);
	}
}
