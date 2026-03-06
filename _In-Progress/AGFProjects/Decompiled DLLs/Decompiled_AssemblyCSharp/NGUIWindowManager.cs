using System;
using System.Collections.Generic;
using UnityEngine;

public class NGUIWindowManager : MonoBehaviour
{
	public Transform[] Windows;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<EnumNGUIWindow, Transform> windowMap = new EnumDictionary<EnumNGUIWindow, Transform>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly HashSet<EnumNGUIWindow> windowsVisibleBeforeHide = new HashSet<EnumNGUIWindow>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool bGlobalShowFlag;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public LocalPlayerUI playerUI;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool parsedWindows;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public TextEllipsisAnimator ellipsisAnimator;

	public GUIWindowManager WindowManager
	{
		get
		{
			if (!(playerUI != null))
			{
				return null;
			}
			return playerUI.windowManager;
		}
	}

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public NGuiWdwInGameHUD InGameHUD
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool AlwaysShowVersionUi { get; set; } = true;

	public bool VersionUiVisible => GetWindow(EnumNGUIWindow.Version).gameObject.activeSelf;

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Awake()
	{
		playerUI = GetComponent<LocalPlayerUI>();
		bGlobalShowFlag = true;
		ParseWindows();
	}

	public void ParseWindows()
	{
		if (parsedWindows)
		{
			return;
		}
		parsedWindows = true;
		Transform[] windows = Windows;
		foreach (Transform transform in windows)
		{
			if (!(transform == null))
			{
				EnumNGUIWindow key = EnumUtils.Parse<EnumNGUIWindow>(transform.name.Substring(3));
				windowMap[key] = transform;
			}
		}
		Transform window = GetWindow(EnumNGUIWindow.InGameHUD);
		if (window != null)
		{
			InGameHUD = window.GetComponent<NGuiWdwInGameHUD>();
		}
		else
		{
			Log.Error("Window wdwInGameHUD not found!");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform GetWindow(EnumNGUIWindow _wdw)
	{
		if (!windowMap.TryGetValue(_wdw, out var value))
		{
			Log.Error("NGUIWindowManager.GetWindow: Window " + _wdw.ToStringCached() + " not found!");
		}
		return value;
	}

	public void ShowAll(bool _bShow)
	{
		bGlobalShowFlag = _bShow;
		if (!_bShow)
		{
			windowsVisibleBeforeHide.Clear();
			{
				foreach (var (enumNGUIWindow2, _) in windowMap)
				{
					if (IsShowing(enumNGUIWindow2))
					{
						windowsVisibleBeforeHide.Add(enumNGUIWindow2);
						Show(enumNGUIWindow2, _bEnable: false);
					}
				}
				return;
			}
		}
		foreach (EnumNGUIWindow item in windowsVisibleBeforeHide)
		{
			Show(item, _bEnable: true);
		}
	}

	public bool IsShowing(EnumNGUIWindow _eWindow)
	{
		Transform window = GetWindow(_eWindow);
		if (window != null)
		{
			return window.gameObject.activeSelf;
		}
		return false;
	}

	public void Show(EnumNGUIWindow _eWindow, bool _bEnable)
	{
		Transform window = GetWindow(_eWindow);
		if (!(window == null))
		{
			window.gameObject.SetActive(_bEnable && bGlobalShowFlag);
			if (_eWindow == EnumNGUIWindow.Loading && ellipsisAnimator != null)
			{
				ellipsisAnimator = null;
			}
		}
	}

	public void SetLabelText(EnumNGUIWindow _eElement, string _text, bool _toUpper = true)
	{
		Transform window = GetWindow(_eElement);
		if (window == null)
		{
			return;
		}
		Show(_eElement, !string.IsNullOrEmpty(_text));
		if (string.IsNullOrEmpty(_text))
		{
			return;
		}
		UILabel component = window.GetComponent<UILabel>();
		if ((bool)component)
		{
			_text = ((!_toUpper) ? _text : _text.ToUpper());
			component.text = _text;
			if (_eElement == EnumNGUIWindow.Loading)
			{
				ellipsisAnimator = new TextEllipsisAnimator(_text, component);
			}
		}
	}

	public void SetLabel(EnumNGUIWindow _eElement, string _text, Color? _color = null, bool _toUpper = true)
	{
		Transform window = GetWindow(_eElement);
		if (window == null)
		{
			return;
		}
		Show(_eElement, !string.IsNullOrEmpty(_text));
		UILabel component = window.GetComponent<UILabel>();
		if ((bool)component)
		{
			_text = ((!_toUpper) ? _text : _text?.ToUpper());
			component.text = _text ?? "";
			if (_color.HasValue)
			{
				component.color = _color.Value;
			}
			if (_eElement == EnumNGUIWindow.Loading)
			{
				ellipsisAnimator = new TextEllipsisAnimator(_text + "...", component);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (playerUI == null)
		{
			playerUI = GetComponent<LocalPlayerUI>();
		}
		if (playerUI.isPrimaryUI)
		{
			_ = WindowManager;
			bool alwaysShowVersionUi = AlwaysShowVersionUi;
			ShowVersionUi(alwaysShowVersionUi);
			if (ellipsisAnimator != null)
			{
				ellipsisAnimator.GetNextAnimatedString(Time.unscaledDeltaTime);
			}
		}
	}

	public void ToggleVersionUi()
	{
		ShowVersionUi(!VersionUiVisible);
	}

	public void ShowVersionUi(bool _show)
	{
		GetWindow(EnumNGUIWindow.Version).gameObject.SetActive(_show);
	}

	public void SetBackgroundScale(float _uiScale)
	{
		GetWindow(EnumNGUIWindow.MainMenuBackground).localScale = new Vector3(_uiScale, _uiScale, _uiScale);
	}
}
