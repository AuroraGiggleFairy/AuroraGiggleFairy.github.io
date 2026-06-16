using System;
using System.Collections.Generic;
using Audio;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_WindowSelector : XUiController
{
	public delegate void WindowSelectedDelegate(XUiC_WindowSelector _sender, string _windowId);

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ButtonSelectable[] buttons;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, XUiC_ButtonSelectable> buttonsByName = new CaseInsensitiveStringDictionary<XUiC_ButtonSelectable>();

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ButtonSelectable selectedButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool nameBasedWindows = true;

	[XuiXmlAttribute("sound_on_open", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string SoundOnOpen { get; set; }

	[XuiXmlAttribute("sound_on_close", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string SoundOnClose { get; set; }

	public XUiC_ButtonSelectable SelectedButton
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return selectedButton;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (selectedButton != null)
			{
				selectedButton.IsSelected = false;
			}
			selectedButton = value;
			if (selectedButton != null)
			{
				selectedButton.IsSelected = true;
			}
			RefreshBindings();
		}
	}

	[XuiXmlAttribute("name_based_windows", false)]
	public bool NameBasedWindows
	{
		get
		{
			return nameBasedWindows;
		}
		set
		{
			nameBasedWindows = value;
		}
	}

	[XuiXmlBinding("open_window_key")]
	public string SelectedName => SelectedButton?.ViewComponent.ID ?? "";

	[XuiXmlBinding("open_window")]
	public string SelectedNameLocalized
	{
		get
		{
			if (SelectedButton == null)
			{
				return "";
			}
			return Localization.Get("xui" + SelectedButton.ViewComponent.ID);
		}
	}

	public event WindowSelectedDelegate WindowSelected;

	public override void Init()
	{
		base.Init();
		for (int i = 0; i < buttons.Length; i++)
		{
			XUiC_ButtonSelectable xUiC_ButtonSelectable = buttons[i];
			buttonsByName[xUiC_ButtonSelectable.ViewComponent.ID] = xUiC_ButtonSelectable;
		}
		if (buttons.Length != 0)
		{
			SetSelected(buttons[0].ViewComponent.ID);
		}
	}

	[XuiBindEvent("OnPress", "buttons")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnPress(XUiController _sender, int _mouseButton)
	{
		tryCloseCurrentWindow([PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			SetSelected(_sender.ViewComponent.ID);
			OpenSelectedWindow();
		});
	}

	public void OpenSelectedWindow()
	{
		if (SelectedButton == null)
		{
			return;
		}
		if (!SelectedButton.ViewComponent.IsVisible || !SelectedButton.ViewComponent.Enabled)
		{
			toggleCategory(1);
			return;
		}
		string iD = SelectedButton.ViewComponent.ID;
		this.WindowSelected?.Invoke(this, iD);
		if (NameBasedWindows && !xui.playerUI.windowManager.IsWindowOpen(iD))
		{
			xui.playerUI.windowManager.CloseAllOpenModalWindows(windowGroup);
			xui.playerUI.windowManager.Open(iD, _bModal: true);
		}
	}

	public void SetSelected(string _name)
	{
		if (buttonsByName.TryGetValue(_name, out var value))
		{
			SelectedButton = value;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		OpenSelectedWindow();
		if (xui.DragAndDropWindow != null)
		{
			xui.DragAndDropWindow.InMenu = true;
		}
		Manager.PlayInsidePlayerHead(SoundOnOpen);
	}

	public override void OnClose()
	{
		base.OnClose();
		if (xui.DragAndDropWindow != null)
		{
			xui.DragAndDropWindow.InMenu = false;
		}
		Manager.PlayInsidePlayerHead(SoundOnClose);
		if (xui.CurrentSelectedEntry != null)
		{
			xui.CurrentSelectedEntry.IsSelected = false;
		}
		if (!xui.playerUI.isPrimaryUI && xui.playerUI.windowManager.GetWindow("toolbelt") is XUiWindowGroup xUiWindowGroup)
		{
			xUiWindowGroup.Controller.GetChildByType<XUiC_Toolbelt>()?.ClearHoveredItems();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void openSelectorAndWindow(string _selectedPage)
	{
		XUiC_FocusedBlockHealth.SetData(xui.playerUI, null, 0f);
		if (xui.playerUI.windowManager.IsWindowOpen(windowGroup) && SelectedButton.ViewComponent.ID.EqualsCaseInsensitive(_selectedPage))
		{
			xui.playerUI.windowManager.CloseAllOpenModalWindows();
			if (xui.playerUI.windowManager.IsWindowOpen(windowGroup))
			{
				xui.playerUI.windowManager.Close(windowGroup);
			}
			return;
		}
		SetSelected(_selectedPage);
		if (xui.playerUI.windowManager.IsWindowOpen(windowGroup))
		{
			OpenSelectedWindow();
			return;
		}
		xui.playerUI.windowManager.CloseAllOpenModalWindows();
		xui.playerUI.windowManager.Open(windowGroup, _bModal: false);
	}

	public static void OpenSelectorAndWindow(EntityPlayerLocal _localPlayer, string _selectedPage)
	{
		if (!_localPlayer.IsDead())
		{
			LocalPlayerUI.GetUIForPlayer(_localPlayer).xui.FindWindowGroupByName("windowpaging").GetChildByType<XUiC_WindowSelector>().openSelectorAndWindow(_selectedPage);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void toggleCategory(int _dir)
	{
		int i;
		for (i = 0; i < buttons.Length && buttons[i] != SelectedButton; i++)
		{
		}
		if (i >= buttons.Length)
		{
			i = -1;
		}
		XUiC_ButtonSelectable xUiC_ButtonSelectable;
		do
		{
			i = NGUIMath.RepeatIndex(i + _dir, buttons.Length);
			xUiC_ButtonSelectable = buttons[i];
		}
		while (!xUiC_ButtonSelectable.ViewComponent.IsVisible || !xUiC_ButtonSelectable.ViewComponent.Enabled);
		SetSelected(buttons[i].ViewComponent.ID);
		buttons[i].ViewComponent.PlayClickSound();
		OpenSelectedWindow();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (!XUiUtils.HotkeysAllowedFor(viewComponent))
		{
			return;
		}
		PlayerActionsGUI gUIActions = xui.playerUI.playerInput.GUIActions;
		if (gUIActions.WindowPagingLeft.WasReleased)
		{
			ThreadManager.RunTaskAfterFrames([PublicizedFrom(EAccessModifier.Private)] () =>
			{
				tryCloseCurrentWindow([PublicizedFrom(EAccessModifier.Private)] () =>
				{
					toggleCategory(-1);
				});
			});
		}
		if (!gUIActions.WindowPagingRight.WasReleased)
		{
			return;
		}
		ThreadManager.RunTaskAfterFrames([PublicizedFrom(EAccessModifier.Private)] () =>
		{
			tryCloseCurrentWindow([PublicizedFrom(EAccessModifier.Private)] () =>
			{
				toggleCategory(1);
			});
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void tryCloseCurrentWindow(Action _onDone)
	{
		if (!(xui.playerUI.windowManager.GetModalWindow() is XUiWindowGroup xUiWindowGroup))
		{
			xui.playerUI.windowManager.Close(xui.playerUI.windowManager.GetModalWindow());
			_onDone();
			return;
		}
		if (xUiWindowGroup.Controller is IXUiWindowConditionalClosing iXUiWindowConditionalClosing)
		{
			iXUiWindowConditionalClosing.TryClose(_onDone, null);
			return;
		}
		foreach (XUiV_Window window in xUiWindowGroup.Windows)
		{
			if (window.Controller is IXUiWindowConditionalClosing iXUiWindowConditionalClosing2)
			{
				iXUiWindowConditionalClosing2.TryClose(_onDone, null);
				return;
			}
		}
		_onDone();
	}
}
