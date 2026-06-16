using System;
using System.Collections.Generic;
using InControl;
using Platform;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GUIWindowManager : MonoBehaviour
{
	public enum HudEnabledStates
	{
		Enabled,
		PartialHide,
		FullHide
	}

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<GUIWindow> windowsToOpen = new List<GUIWindow>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<GUIWindow> windowsToRemove = new List<GUIWindow>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<GUIWindow> openWindows = new List<GUIWindow>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, GUIWindow> nameToWindowMap = new CaseInsensitiveStringDictionary<GUIWindow>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public GUIWindow topmostWindow;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public GUIWindow modalWindow;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool cursorWindowOpen;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<NGuiAction> globalActions = new List<NGuiAction>();

	public bool IsInputLocked;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public NGUIWindowManager nguiWindowManager;

	public LocalPlayerUI playerUI;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<NGuiAction> lastActionClicked = new List<NGuiAction>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<NGuiAction> actionsToClear = new List<NGuiAction>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<NGuiAction> actionsForGlobalHotkeys = new List<NGuiAction>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public HudEnabledStates bHUDEnabled;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public HudEnabledStates bTempEnabled;

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Awake()
	{
		nguiWindowManager = GetComponent<NGUIWindowManager>();
		playerUI = GetComponent<LocalPlayerUI>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnGUI()
	{
		for (int i = 0; i < windowsToOpen.Count; i++)
		{
			GUIWindow gUIWindow = windowsToOpen[i];
			gUIWindow.isShowing = true;
			openWindows.Add(gUIWindow);
			topmostWindow = gUIWindow;
		}
		windowsToOpen.Clear();
		modalWindow = null;
		for (int j = 0; j < openWindows.Count; j++)
		{
			GUIWindow gUIWindow2 = openWindows[j];
			if (gUIWindow2.isModal)
			{
				modalWindow = gUIWindow2;
				break;
			}
		}
		cursorWindowOpen = false;
		for (int k = 0; k < openWindows.Count; k++)
		{
			GUIWindow gUIWindow3 = openWindows[k];
			if (gUIWindow3.isShowing)
			{
				cursorWindowOpen |= gUIWindow3.alwaysUsesMouseCursor;
				GUI.matrix = Matrix4x4.identity;
				gUIWindow3.OnGUI();
			}
		}
		GUI.enabled = true;
		List<GUIWindow> list = windowsToRemove;
		list.Clear();
		for (int l = 0; l < openWindows.Count; l++)
		{
			GUIWindow gUIWindow4 = openWindows[l];
			if (!gUIWindow4.isShowing)
			{
				list.Add(gUIWindow4);
				topmostWindow = ((openWindows.Count > 0) ? openWindows[openWindows.Count - 1] : null);
			}
		}
		for (int m = 0; m < list.Count; m++)
		{
			GUIWindow item = list[m];
			openWindows.Remove(item);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Update()
	{
		for (int i = 0; i < openWindows.Count; i++)
		{
			openWindows[i].Update();
		}
		if (lastActionClicked.Count != 0)
		{
			for (int j = 0; j < lastActionClicked.Count; j++)
			{
				NGuiAction nGuiAction = lastActionClicked[j];
				PlayerAction hotkey = nGuiAction.GetHotkey();
				if (hotkey != null && hotkey.WasReleased)
				{
					nGuiAction.OnRelease();
					actionsToClear.Add(nGuiAction);
				}
			}
			for (int k = 0; k < actionsToClear.Count; k++)
			{
				NGuiAction item = actionsToClear[k];
				lastActionClicked.Remove(item);
			}
			actionsToClear.Clear();
		}
		if (IsInputActive())
		{
			if (playerUI.playerInput != null && playerUI.playerInput.PermanentActions.Cancel.WasPressed && UIInput.selection != null)
			{
				UIInput.selection.RemoveFocus();
			}
		}
		else
		{
			if (IsInputLocked)
			{
				return;
			}
			List<NGuiAction> list = actionsForGlobalHotkeys;
			list.Clear();
			for (int l = 0; l < globalActions.Count; l++)
			{
				PlayerAction hotkey2 = globalActions[l].GetHotkey();
				if (hotkey2 != null && (((globalActions[l].KeyMode & NGuiAction.EnumKeyMode.FireOnRelease) == NGuiAction.EnumKeyMode.FireOnRelease && hotkey2.WasReleased) || ((globalActions[l].KeyMode & NGuiAction.EnumKeyMode.FireOnPress) == NGuiAction.EnumKeyMode.FireOnPress && hotkey2.WasPressed) || ((globalActions[l].KeyMode & NGuiAction.EnumKeyMode.FireOnRepeat) == NGuiAction.EnumKeyMode.FireOnRepeat && hotkey2.WasRepeated)))
				{
					list.Add(globalActions[l]);
				}
			}
			if (list.Count > 0)
			{
				for (int m = 0; m < list.Count; m++)
				{
					NGuiAction nGuiAction2 = list[m];
					nGuiAction2.OnClick();
					lastActionClicked.Add(nGuiAction2);
				}
				list.Clear();
			}
			if (playerUI.playerInput != null && playerUI.playerInput.PermanentActions.Cancel.WasPressed && !IsWindowOpen("popupGroup") && modalWindow != null && modalWindow.isEscClosable)
			{
				CloseAllOpenModalWindows(null, _fromEsc: true);
			}
		}
	}

	public bool IsInputActive()
	{
		if ((!(UIInput.selection != null) || !UIInput.selection.gameObject.activeInHierarchy) && (topmostWindow == null || !topmostWindow.isInputActive) && !IsUGUIInputActive())
		{
			return GUIWindowConsole.IsOpen();
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsUGUIInputActive()
	{
		EventSystem current = EventSystem.current;
		if (current == null)
		{
			return false;
		}
		GameObject currentSelectedGameObject = current.currentSelectedGameObject;
		if (currentSelectedGameObject == null)
		{
			return false;
		}
		if (currentSelectedGameObject.TryGetComponent<InputField>(out var component))
		{
			return component.isFocused;
		}
		if (currentSelectedGameObject.TryGetComponent<TMP_InputField>(out var component2))
		{
			return component2.isFocused;
		}
		return false;
	}

	public bool IsKeyShortcutsAllowed()
	{
		if (!IsInputLocked)
		{
			if (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard)
			{
				return !IsInputActive();
			}
			return true;
		}
		return false;
	}

	public void Add(GUIWindow _window)
	{
		nameToWindowMap.Add(_window.Id, _window);
		_window.windowManager = this;
		if (nguiWindowManager == null)
		{
			nguiWindowManager = GetComponent<NGUIWindowManager>();
		}
		if (playerUI == null)
		{
			playerUI = GetComponent<LocalPlayerUI>();
		}
		_window.playerUI = playerUI;
	}

	public void Remove(GUIWindow _w)
	{
		if (_w.isShowing)
		{
			Close(_w);
		}
		_w.Cleanup();
		nameToWindowMap.Remove(_w.Id);
	}

	public GUIWindow GetWindow(string _windowName)
	{
		if (!nameToWindowMap.TryGetValue(_windowName, out var value))
		{
			Log.Warning("GUIWindowManager.GetWindow: Window \"{0}\" unknown!", _windowName);
			return null;
		}
		return value;
	}

	public bool TryGetWindow(string _windowName, out GUIWindow _window)
	{
		return nameToWindowMap.TryGetValue(_windowName, out _window);
	}

	public T GetWindow<T>(string _windowName) where T : GUIWindow
	{
		return (T)nameToWindowMap[_windowName];
	}

	public void SwitchVisible(string _windowName, bool _modal = true)
	{
		if (nameToWindowMap.TryGetValue(_windowName, out var value))
		{
			SwitchVisible(value, _modal);
		}
	}

	public void SwitchVisible(GUIWindow _guiWindow, bool _modal = true)
	{
		if ((!_modal || _guiWindow.isModal) && _guiWindow.isShowing)
		{
			Close(_guiWindow);
		}
		else
		{
			Open(_guiWindow, _modal);
		}
	}

	public bool CloseAllOpenModalWindows(string _exceptWindowName)
	{
		nameToWindowMap.TryGetValue(_exceptWindowName, out var value);
		return CloseAllOpenModalWindows(value);
	}

	public bool CloseAllOpenModalWindows(GUIWindow _exceptWindow = null, bool _fromEsc = false)
	{
		bool result = false;
		for (int i = 0; i < openWindows.Count; i++)
		{
			GUIWindow gUIWindow = openWindows[i];
			if (gUIWindow.isModal && (_exceptWindow == null || _exceptWindow != gUIWindow))
			{
				Close(gUIWindow, _fromEsc);
				result = true;
			}
		}
		if (playerUI.CursorController != null)
		{
			if (playerUI.CursorController.navigationTarget != null)
			{
				playerUI.CursorController.navigationTarget.Controller.Hovered(_isOver: false);
			}
			playerUI.CursorController.SetNavigationTarget(null);
			playerUI.CursorController.SetNavigationLockView(null);
		}
		return result;
	}

	public void Open(string _windowName, bool _bModal)
	{
		if (!nameToWindowMap.TryGetValue(_windowName, out var value))
		{
			Log.Warning("GUIWindowManager.Open: Window \"{0}\" unknown!", _windowName);
			Log.Out("Trace: " + StackTraceUtility.ExtractStackTrace());
		}
		else
		{
			openInternal(value, _bModal, _bIsNotEscClosable: false);
		}
	}

	public void Open(string _windowName, bool _bModal, bool _bIsNotEscClosable)
	{
		if (!nameToWindowMap.TryGetValue(_windowName, out var value))
		{
			Log.Warning("GUIWindowManager.Open: Window \"{0}\" unknown!", _windowName);
			Log.Out("Trace: " + StackTraceUtility.ExtractStackTrace());
		}
		else
		{
			openInternal(value, _bModal, _bIsNotEscClosable);
		}
	}

	public void Open(GUIWindow _w, bool _bModal)
	{
		openInternal(_w, _bModal, _bIsNotEscClosable: false);
	}

	public void Open(GUIWindow _w, bool _bModal, bool _bIsNotEscClosable)
	{
		openInternal(_w, _bModal, _bIsNotEscClosable);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void openInternal(GUIWindow _w, bool _bModal, bool _bIsNotEscClosable)
	{
		if (_w == null || IsFullHUDDisabled() || IsWindowOpen(_w))
		{
			return;
		}
		QuestEventManager.Current.ChangedWindow(_w.Id);
		if (_bModal)
		{
			CloseAllOpenModalWindows();
			for (int i = 0; i < windowsToOpen.Count; i++)
			{
				GUIWindow gUIWindow = windowsToOpen[i];
				if (gUIWindow.isModal && gUIWindow != _w)
				{
					windowsToOpen.Remove(gUIWindow);
					gUIWindow.OnClose();
					if (gUIWindow.HasActionSet())
					{
						DisableWindowActionSet(gUIWindow);
					}
					break;
				}
			}
		}
		_w.isModal = _bModal;
		if (!_w.isShowing)
		{
			_w.windowManager = this;
			_w.isEscClosable = !_bIsNotEscClosable;
			bool num = _w.isShowing || windowsToOpen.Contains(_w);
			if (!openWindows.Contains(_w))
			{
				windowsToOpen.Add(_w);
			}
			else
			{
				_w.isShowing = true;
			}
			if (!num)
			{
				_w.OnOpen();
			}
			if (_w.HasActionSet() && !_w.bActionSetEnabled && (_w.isShowing || windowsToOpen.Contains(_w)))
			{
				EnableWindowActionSet(_w);
			}
		}
	}

	public bool IsWindowOpen(string _wdwID)
	{
		if (!nameToWindowMap.TryGetValue(_wdwID, out var value))
		{
			return false;
		}
		return IsWindowOpen(value);
	}

	public bool IsWindowOpen(GUIWindow _window)
	{
		if (_window != null)
		{
			if (!_window.isShowing)
			{
				return windowsToOpen.Contains(_window);
			}
			return true;
		}
		return false;
	}

	public bool IsModalWindowOpen()
	{
		return modalWindow != null;
	}

	public GUIWindow GetModalWindow()
	{
		return modalWindow;
	}

	public bool IsCursorWindowOpen()
	{
		return cursorWindowOpen;
	}

	public void Close(string _windowName)
	{
		if (nameToWindowMap.TryGetValue(_windowName, out var value))
		{
			Close(value);
		}
	}

	public void Close(GUIWindow _w, bool _fromEsc = false)
	{
		if (_w == null)
		{
			return;
		}
		if (_w.isShowing)
		{
			_w.isShowing = false;
			_w.OnClose();
			if (_fromEsc && !string.IsNullOrEmpty(_w.openWindowOnEsc))
			{
				Open(_w.openWindowOnEsc, _w.isModal);
			}
		}
		else if (windowsToOpen.Contains(_w))
		{
			windowsToOpen.Remove(_w);
			_w.OnClose();
		}
		if (_w.bActionSetEnabled)
		{
			DisableWindowActionSet(_w);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DisableWindowActionSet(GUIWindow _w)
	{
		if (_w.playerUI != null && _w.playerUI.ActionSetManager != null)
		{
			_w.playerUI.ActionSetManager.Pop(_w);
			_w.bActionSetEnabled = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EnableWindowActionSet(GUIWindow _w)
	{
		if (_w.playerUI != null && _w.playerUI.ActionSetManager != null)
		{
			_w.playerUI.ActionSetManager.Push(_w);
			_w.bActionSetEnabled = true;
		}
	}

	public void ResetActionSets()
	{
		ActionSetManager actionSetManager = playerUI.ActionSetManager;
		if (actionSetManager == null)
		{
			return;
		}
		actionSetManager.Reset();
		actionSetManager.Push(playerUI.playerInput);
		foreach (GUIWindow openWindow in openWindows)
		{
			if (openWindow.isShowing && openWindow.HasActionSet())
			{
				EnableWindowActionSet(openWindow);
			}
		}
		foreach (GUIWindow item in windowsToOpen)
		{
			if (item.bActionSetEnabled)
			{
				EnableWindowActionSet(item);
			}
		}
	}

	public void RemoveGlobalAction(NGuiAction _action)
	{
		globalActions.Remove(_action);
	}

	public void AddGlobalAction(NGuiAction _action)
	{
		globalActions.Add(_action);
	}

	public bool IsHUDEnabled()
	{
		return bHUDEnabled == HudEnabledStates.Enabled;
	}

	public bool IsHUDPartialHidden()
	{
		return bHUDEnabled == HudEnabledStates.PartialHide;
	}

	public bool IsFullHUDDisabled()
	{
		return bHUDEnabled == HudEnabledStates.FullHide;
	}

	public void ToggleHUDEnabled()
	{
		bHUDEnabled = bHUDEnabled.CycleEnum();
		SetHUDEnabled(bHUDEnabled);
	}

	public void TempHUDDisable()
	{
		bTempEnabled = bHUDEnabled;
		bHUDEnabled = HudEnabledStates.FullHide;
		SetHUDEnabled(bHUDEnabled);
	}

	public void ReEnableHUD()
	{
		bHUDEnabled = bTempEnabled;
		SetHUDEnabled(bHUDEnabled);
	}

	public void SetHUDEnabled(HudEnabledStates _hudState)
	{
		bHUDEnabled = _hudState;
		switch (_hudState)
		{
		case HudEnabledStates.Enabled:
		case HudEnabledStates.PartialHide:
			nguiWindowManager.ShowAll(_bShow: true);
			playerUI.xui.transform.gameObject.SetActive(value: true);
			break;
		case HudEnabledStates.FullHide:
			nguiWindowManager.ShowAll(_bShow: false);
			playerUI.xui.transform.gameObject.SetActive(value: false);
			break;
		}
	}
}
