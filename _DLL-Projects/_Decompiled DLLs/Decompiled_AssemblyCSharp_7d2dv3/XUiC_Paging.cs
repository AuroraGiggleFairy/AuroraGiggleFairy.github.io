using System;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_Paging : XUiController
{
	public static bool IsOverPagingOverrideElement;

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentPageNumber;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool controllerOverContentArea;

	[PublicizedFrom(EAccessModifier.Private)]
	public Func<bool> handlePageDownAction;

	[PublicizedFrom(EAccessModifier.Private)]
	public Func<bool> handlePageUpAction;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lastPageNumber;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController contentsParent;

	[XuiBindComponent("pageUp", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiController btnPageUp;

	[XuiBindComponent("pageDown", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiController btnPageDown;

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
				IsDirty = true;
			}
		}
	}

	[XuiXmlBinding("pagenumber")]
	public int CurrentPageNumberNatural => CurrentPageNumber + 1;

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
				IsDirty = true;
				if (CurrentPageNumber > lastPageNumber)
				{
					CurrentPageNumber = lastPageNumber;
					this.OnPageChanged?.Invoke();
				}
			}
		}
	}

	[XuiXmlBinding("maxpagenumber")]
	public int LastPageNumberNatural => LastPageNumber + 1;

	[XuiXmlBinding("over_content_area")]
	public bool ControllerOverContentArea
	{
		get
		{
			return controllerOverContentArea;
		}
		set
		{
			if (controllerOverContentArea != value)
			{
				controllerOverContentArea = value;
				IsDirty = true;
			}
		}
	}

	[XuiXmlAttribute("show_max_page", false)]
	[XuiXmlBinding("showmaxpage")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool ShowMaxPage { get; set; }

	[XuiXmlAttribute("separator", false)]
	[XuiXmlBinding("separator")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string Separator { get; set; } = "/";

	[XuiXmlAttribute("contents_parent", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string ContentParentName { get; set; }

	[XuiXmlAttribute("hotkeys_enabled", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool HotkeysEnabled { get; set; } = true;

	public event XUiEvent_PageChangedEventHandler OnPageChanged;

	public override void Init()
	{
		base.Init();
		if (btnPageDown != null)
		{
			btnPageDown.OnPress += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
			{
				PageDown();
			};
		}
		if (btnPageUp != null)
		{
			btnPageUp.OnPress += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
			{
				PageUp();
			};
		}
		if (!string.IsNullOrEmpty(ContentParentName))
		{
			XUiController xUiController = this;
			while (!xUiController.TryGetChildController<XUiController>(ContentParentName, out contentsParent))
			{
				xUiController = xUiController.Parent;
				if (xUiController == null)
				{
					break;
				}
			}
			if (contentsParent == null)
			{
				Log.Warning("[XUi] Could not find contents parent '" + ContentParentName + "' for pager. Hierarchy: " + GetXuiHierarchy());
			}
		}
		else
		{
			Log.Warning("[XUi] No contents parent set for pager. Hierarchy: " + GetXuiHierarchy());
		}
		handlePageDownAction = PageDown;
		handlePageUpAction = PageUp;
		CurrentPageNumber = 0;
	}

	public bool PageUp()
	{
		if (CurrentPageNumber >= LastPageNumber)
		{
			return false;
		}
		CurrentPageNumber++;
		this.OnPageChanged?.Invoke();
		return true;
	}

	public bool PageDown()
	{
		if (CurrentPageNumber <= 0)
		{
			return false;
		}
		CurrentPageNumber--;
		this.OnPageChanged?.Invoke();
		return true;
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

	public void Reset()
	{
		CurrentPageNumber = 0;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		ControllerOverContentArea = updateControllerState();
		if (ControllerOverContentArea)
		{
			XUi.HandlePaging(xui, handlePageUpAction, handlePageDownAction);
		}
		handleDirtyUpdateDefault();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool updateControllerState()
	{
		IPlatform nativePlatform = PlatformManager.NativePlatform;
		if (nativePlatform != null && nativePlatform.Input?.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard)
		{
			return false;
		}
		if (contentsParent == null)
		{
			return false;
		}
		if (!HotkeysEnabled || IsOverPagingOverrideElement)
		{
			return false;
		}
		if (xui.playerUI.CursorController.CurrentTarget == null || !xui.playerUI.CursorController.CurrentTarget.Controller.IsSelfOrChildOf(contentsParent))
		{
			return false;
		}
		return true;
	}
}
