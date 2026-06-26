using System;
using System.Collections.Generic;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_Paging : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int currentPageNumber;

	public bool showMaxPage;

	public string separator = "/";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool primaryPager = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lastPageNumber;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action handlePageDownAction;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action handlePageUpAction;

	[PublicizedFrom(EAccessModifier.Private)]
	public string contentParentName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController contentsParent;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnPageUp;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnPageDown;

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<XUiC_Paging> activePagers = new List<XUiC_Paging>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt pagenumberFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt maxpagenumberFormatter = new CachedStringFormatterInt();

	public int CurrentPageNumber
	{
		get
		{
			return currentPageNumber;
		}
		set
		{
			value = Mathf.Clamp(value, 0, LastPageNumber);
			if (value != currentPageNumber)
			{
				currentPageNumber = value;
				RefreshBindings();
			}
		}
	}

	public int LastPageNumber
	{
		get
		{
			return lastPageNumber;
		}
		set
		{
			if (value != lastPageNumber)
			{
				lastPageNumber = value;
				RefreshBindings();
				if (currentPageNumber > lastPageNumber)
				{
					CurrentPageNumber = lastPageNumber;
					this.OnPageChanged?.Invoke();
				}
			}
		}
	}

	public event XUiEvent_PageChangedEventHandler OnPageChanged;

	public override void Init()
	{
		base.Init();
		btnPageDown = GetChildById("pageDown");
		if (btnPageDown != null)
		{
			btnPageDown.OnPress += [PublicizedFrom(EAccessModifier.Private)] (XUiController _sender, int _i) =>
			{
				PageDown();
			};
		}
		btnPageUp = GetChildById("pageUp");
		if (btnPageUp != null)
		{
			btnPageUp.OnPress += [PublicizedFrom(EAccessModifier.Private)] (XUiController _sender, int _i) =>
			{
				PageUp();
			};
		}
		if (!string.IsNullOrEmpty(contentParentName))
		{
			contentsParent = base.WindowGroup.Controller.GetChildById(contentParentName);
		}
		handlePageDownAction = PageDown;
		handlePageUpAction = PageUp;
		currentPageNumber = 0;
		RefreshBindings();
	}

	public void PageUp()
	{
		if (currentPageNumber < LastPageNumber)
		{
			currentPageNumber++;
			this.OnPageChanged?.Invoke();
			RefreshBindings();
			if (currentPageNumber == lastPageNumber && base.xui.playerUI.CursorController.navigationTarget == btnPageUp.ViewComponent)
			{
				btnPageDown.SelectCursorElement();
			}
		}
	}

	public void PageDown()
	{
		if (currentPageNumber > 0)
		{
			currentPageNumber--;
			this.OnPageChanged?.Invoke();
			RefreshBindings();
			if (currentPageNumber == 0 && base.xui.playerUI.CursorController.navigationTarget == btnPageDown.ViewComponent)
			{
				btnPageUp.SelectCursorElement();
			}
		}
	}

	public int GetPage()
	{
		return CurrentPageNumber;
	}

	public void SetPage(int _page)
	{
		CurrentPageNumber = _page;
	}

	public int GetLastPage()
	{
		return LastPageNumber;
	}

	public void SetLastPageByElementsAndPageLength(int _elementCount, int _pageLength)
	{
		LastPageNumber = Math.Max(0, Mathf.CeilToInt((float)_elementCount / (float)_pageLength) - 1);
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		switch (_name)
		{
		case "show_max_page":
			showMaxPage = StringParsers.ParseBool(_value);
			return true;
		case "separator":
			separator = _value;
			return true;
		case "primary_pager":
			primaryPager = StringParsers.ParseBool(_value);
			return true;
		case "contents_parent":
			contentParentName = _value;
			return true;
		default:
			return base.ParseAttribute(_name, _value, _parent);
		}
	}

	public override bool GetBindingValue(ref string value, string bindingName)
	{
		switch (bindingName)
		{
		case "pagenumber":
			value = pagenumberFormatter.Format(currentPageNumber + 1);
			return true;
		case "maxpagenumber":
			value = maxpagenumberFormatter.Format(LastPageNumber + 1);
			return true;
		case "showmaxpage":
			value = showMaxPage.ToString();
			return true;
		case "separator":
			value = separator;
			return true;
		default:
			return base.GetBindingValue(ref value, bindingName);
		}
	}

	public void Reset()
	{
		currentPageNumber = 0;
		RefreshBindings();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		activePagers.Add(this);
	}

	public override void OnClose()
	{
		base.OnClose();
		activePagers.Remove(this);
		if (activePagers.Count == 0)
		{
			base.xui.calloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuPaging);
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard || activePagers.Count == 0 || activePagers[0] != this || LocalPlayerUI.IsAnyComboBoxFocused)
		{
			return;
		}
		bool flag = false;
		foreach (XUiC_Paging activePager in activePagers)
		{
			if (activePagers.Count == 1 || activePager.contentsParent == null || (base.xui.playerUI.CursorController.CurrentTarget != null && base.xui.playerUI.CursorController.CurrentTarget.Controller.IsChildOf(activePager.contentsParent)))
			{
				XUi.HandlePaging(base.xui, activePager.handlePageUpAction, activePager.handlePageDownAction);
				base.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuPaging);
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			base.xui.calloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuPaging);
		}
	}
}
