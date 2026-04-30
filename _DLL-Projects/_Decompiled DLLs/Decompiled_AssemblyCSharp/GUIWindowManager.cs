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
	public readonly List<GUIWindow> windows = new List<GUIWindow>();

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
		GameOptionsManager.ResolutionChanged += OnResolutionChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		GameOptionsManager.ResolutionChanged -= OnResolutionChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnResolutionChanged(int _width, int _height)
	{
		RecenterAllWindows(_width, _height);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnGUI()
	{
		for (int i = 0; i < windowsToOpen.Count; i++)
		{
			GUIWindow gUIWindow = windowsToOpen[i];
			gUIWindow.isShowing = true;
			windows.Add(gUIWindow);
			topmostWindow = gUIWindow;
		}
		windowsToOpen.Clear();
		modalWindow = null;
		for (int j = 0; j < windows.Count; j++)
		{
			GUIWindow gUIWindow2 = windows[j];
			if (gUIWindow2.isModal)
			{
				modalWindow = gUIWindow2;
				break;
			}
		}
		if (modalWindow != null)
		{
			_ = modalWindow.isDimBackground;
		}
		cursorWindowOpen = false;
		for (int k = 0; k < windows.Count; k++)
		{
			GUIWindow gUIWindow3 = windows[k];
			if (gUIWindow3.isShowing)
			{
				cursorWindowOpen |= gUIWindow3.alwaysUsesMouseCursor;
				GUI.matrix = gUIWindow3.matrix;
				gUIWindow3.OnGUI(topmostWindow == gUIWindow3);
			}
		}
		GUI.enabled = true;
		List<GUIWindow> list = windowsToRemove;
		list.Clear();
		for (int l = 0; l < windows.Count; l++)
		{
			GUIWindow gUIWindow4 = windows[l];
			if (!gUIWindow4.isShowing)
			{
				list.Add(gUIWindow4);
				topmostWindow = ((windows.Count > 0) ? windows[windows.Count - 1] : null);
			}
		}
		for (int m = 0; m < list.Count; m++)
		{
			GUIWindow item = list[m];
			windows.Remove(item);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Update()
	{
		for (int i = 0; i < windows.Count; i++)
		{
			windows[i].Update();
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
				CloseAllOpenWindows(null, _fromEsc: true);
			}
		}
	}

	public bool IsInputActive()
	{
		if ((!(UIInput.selection != null) || !UIInput.selection.gameObject.activeInHierarchy) && (topmostWindow == null || !topmostWindow.isInputActive) && !IsUGUIInputActive())
		{
			return GameManager.Instance.m_GUIConsole.isShowing;
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
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard)
		{
			if (!IsInputLocked)
			{
				return !IsInputActive();
			}
			return false;
		}
		return true;
	}

	public void Add(string _windowName, GUIWindow _window)
	{
		nameToWindowMap.Add(_windowName, _window);
		_window.windowManager = this;
		if (nguiWindowManager == null)
		{
			nguiWindowManager = GetComponent<NGUIWindowManager>();
		}
		if (playerUI == null)
		{
			playerUI = GetComponent<LocalPlayerUI>();
		}
		_window.nguiWindowManager = nguiWindowManager;
		_window.playerUI = playerUI;
	}

	public void Remove(string _windowName)
	{
		if (!nameToWindowMap.TryGetValue(_windowName, out var value))
		{
			Log.Warning("GUIWindowManager.Remove: Window \"{0}\" unknown!", _windowName);
			return;
		}
		if (value.isShowing)
		{
			Close(value);
		}
		value.Cleanup();
		nameToWindowMap.Remove(_windowName);
	}

	public GUIWindow GetWindow(string _windowName)
	{
		if (!nameToWindowMap.TryGetValue(_windowName, out var value))
		{
			Log.Warning("GUIWindowManager.Remove: Window \"{0}\" unknown!", _windowName);
			return null;
		}
		return value;
	}

	public T GetWindow<T>(string _windowName) where T : GUIWindow
	{
		return (T)nameToWindowMap[_windowName];
	}

	public void SwitchVisible(string _windowName, bool _bIsNotEscClosable = false, bool _modal = true)
	{
		SwitchVisible(nameToWindowMap[_windowName], _bIsNotEscClosable, _modal);
	}

	public void SwitchVisible(GUIWindow _guiWindow, bool _bIsNotEscClosable = false, bool _modal = true)
	{
		if ((!_modal || _guiWindow.isModal) && _guiWindow.isShowing)
		{
			Close(_guiWindow);
		}
		else
		{
			Open(_guiWindow, _modal, _bIsNotEscClosable);
		}
	}

	public bool CloseAllOpenWindows(GUIWindow _exceptThis = null, bool _fromEsc = false)
	{
		bool result = false;
		for (int i = 0; i < windows.Count; i++)
		{
			GUIWindow gUIWindow = windows[i];
			if (gUIWindow.isModal && (_exceptThis == null || _exceptThis != gUIWindow))
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

	public bool CloseAllOpenWindows(string _windowName)
	{
		GUIWindow exceptThis = nameToWindowMap[_windowName];
		return CloseAllOpenWindows(exceptThis);
	}

	public void Open(string _windowName, int _x, int _y, bool _bModal, bool _bIsNotEscClosable = false)
	{
		GUIWindow gUIWindow = nameToWindowMap[_windowName];
		gUIWindow.windowRect = new Rect(_x, _y, gUIWindow.windowRect.width, gUIWindow.windowRect.height);
		Open(gUIWindow, _bModal, _bIsNotEscClosable);
	}

	public void OpenIfNotOpen(string _windowName, bool _bModal, bool _bIsNotEscClosable = false, bool _bCloseAllOpenWindows = true)
	{
		if (!IsWindowOpen(_windowName))
		{
			Open(_windowName, _bModal, _bIsNotEscClosable, _bCloseAllOpenWindows);
		}
	}

	public void CloseIfOpen(string _windowName)
	{
		if (IsWindowOpen(_windowName))
		{
			Close(_windowName);
		}
	}

	public void Open(string _windowName, bool _bModal, bool _bIsNotEscClosable = false, bool _bCloseAllOpenWindows = true)
	{
		if (!IsFullHUDDisabled())
		{
			if (!nameToWindowMap.TryGetValue(_windowName, out var value))
			{
				Log.Warning("GUIWindowManager.Open: Window \"{0}\" unknown!", _windowName);
				Log.Out("Trace: " + StackTraceUtility.ExtractStackTrace());
			}
			else
			{
				QuestEventManager.Current.ChangedWindow(_windowName);
				Open(value, _bModal, _bIsNotEscClosable, _bCloseAllOpenWindows);
			}
		}
	}

	public void Open(GUIWindow _w, bool _bModal, bool _bIsNotEscClosable = false, bool _bCloseAllOpenWindows = true)
	{
		if (IsFullHUDDisabled())
		{
			return;
		}
		if (_bModal)
		{
			if (_bCloseAllOpenWindows)
			{
				CloseAllOpenWindows();
			}
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
		if (_w.isShowing)
		{
			_w.isModal = _bModal;
			return;
		}
		_w.windowManager = this;
		_w.isModal = _bModal;
		_w.isEscClosable = !_bIsNotEscClosable;
		bool num = _w.isShowing || windowsToOpen.Contains(_w);
		if (!windows.Contains(_w))
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

	public bool IsWindowOpen(string _wdwID)
	{
		if (!nameToWindowMap.TryGetValue(_wdwID, out var value))
		{
			return false;
		}
		if (!value.isShowing)
		{
			return windowsToOpen.Contains(value);
		}
		return true;
	}

	public bool HasWindow(string _wdwID)
	{
		return nameToWindowMap.ContainsKey(_wdwID);
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
		if (HasWindow(_windowName))
		{
			Close(nameToWindowMap[_windowName]);
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

	public void Close(GUIWindow _w, bool _fromEsc = false)
	{
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

	public void RecenterAllWindows(int _w, int _h)
	{
		foreach (var (_, gUIWindow2) in nameToWindowMap)
		{
			if (gUIWindow2.bCenterWindow)
			{
				gUIWindow2.SetPosition(((float)_w - gUIWindow2.windowRect.width) / 2f, ((float)_h - gUIWindow2.windowRect.height) / 2f);
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
		if (bHUDEnabled == HudEnabledStates.FullHide)
		{
			bHUDEnabled = HudEnabledStates.Enabled;
		}
		else
		{
			bHUDEnabled++;
		}
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

	public void ResetActionSets()
	{
		ActionSetManager actionSetManager = playerUI.ActionSetManager;
		if (actionSetManager == null)
		{
			return;
		}
		actionSetManager.Reset();
		actionSetManager.Push(playerUI.playerInput);
		foreach (GUIWindow window in windows)
		{
			if (window.isShowing && window.HasActionSet())
			{
				EnableWindowActionSet(window);
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
}
