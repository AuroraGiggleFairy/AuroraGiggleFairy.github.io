using System;
using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ServerFilters : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<IServerBrowserFilterControl> allFilterControls = new List<IServerBrowserFilterControl>();

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ServerBrowserGamePrefSelectorCombo regionFilter;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ServerBrowserGamePrefString languageFilter;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ServerBrowserGamePrefSelectorCombo crossplayFilter;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ServerBrowserGamePrefSelectorCombo moddedConfigsFilter;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ServerBrowserGamePrefSelectorCombo requiresModsFilter;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ServerBrowserGamePrefSelectorCombo eacFilter;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ServerBrowserGamePrefSelectorCombo ignoreSanctionsFilter;

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentResults;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool openedBefore;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnStartSearch;

	public event Action<IServerBrowserFilterControl> OnFilterChanged;

	public override void Init()
	{
		base.Init();
		GetChildById("outclick").OnPress += CloseFiltersButton_OnPress;
		GetChildById("btnCloseFilters").OnPress += CloseFiltersButton_OnPress;
		((XUiC_SimpleButton)GetChildById("btnResetFilters")).OnPressed += ResetFiltersButton_OnPress;
		((XUiC_SimpleButton)GetChildById("btnBack")).OnPressed += BackButton_OnPress;
		btnStartSearch = (XUiC_SimpleButton)GetChildById("btnStartSearch");
		btnStartSearch.OnPressed += StartSearchButton_OnPress;
		btnStartSearch.Text = "[action:gui:GUI Apply] " + Localization.Get("xuiServerStartSearch").ToUpper();
		XUiC_ServerBrowserGamePrefSelectorCombo[] childrenByType = GetChildrenByType<XUiC_ServerBrowserGamePrefSelectorCombo>();
		foreach (XUiC_ServerBrowserGamePrefSelectorCombo xUiC_ServerBrowserGamePrefSelectorCombo in childrenByType)
		{
			switch (xUiC_ServerBrowserGamePrefSelectorCombo.GameInfoInt)
			{
			case GameInfoInt.AirDropFrequency:
				xUiC_ServerBrowserGamePrefSelectorCombo.ValuePreDisplayModifierFunc = [PublicizedFrom(EAccessModifier.Internal)] (int _n) => _n / 24;
				break;
			case GameInfoInt.CurrentServerTime:
				xUiC_ServerBrowserGamePrefSelectorCombo.CustomValuePreFilterModifierFunc = [PublicizedFrom(EAccessModifier.Internal)] (int _n) => (_n - 1) * 24000;
				break;
			case GameInfoInt.WorldSize:
				if (PlatformOptimizations.EnforceMaxWorldSizeClient)
				{
					xUiC_ServerBrowserGamePrefSelectorCombo.ValueRangeMin = 0;
					xUiC_ServerBrowserGamePrefSelectorCombo.ValueRangeMax = PlatformOptimizations.MaxWorldSizeClient;
				}
				break;
			}
			if (xUiC_ServerBrowserGamePrefSelectorCombo.GameInfoString == GameInfoString.Region)
			{
				regionFilter = xUiC_ServerBrowserGamePrefSelectorCombo;
			}
			switch (xUiC_ServerBrowserGamePrefSelectorCombo.GameInfoBool)
			{
			case GameInfoBool.EACEnabled:
				eacFilter = xUiC_ServerBrowserGamePrefSelectorCombo;
				break;
			case GameInfoBool.SanctionsIgnored:
				ignoreSanctionsFilter = xUiC_ServerBrowserGamePrefSelectorCombo;
				break;
			case GameInfoBool.AllowCrossplay:
				crossplayFilter = xUiC_ServerBrowserGamePrefSelectorCombo;
				break;
			case GameInfoBool.ModdedConfig:
				moddedConfigsFilter = xUiC_ServerBrowserGamePrefSelectorCombo;
				break;
			case GameInfoBool.RequiresMod:
				requiresModsFilter = xUiC_ServerBrowserGamePrefSelectorCombo;
				break;
			}
			xUiC_ServerBrowserGamePrefSelectorCombo.OnValueChanged = (Action<IServerBrowserFilterControl>)Delegate.Combine(xUiC_ServerBrowserGamePrefSelectorCombo.OnValueChanged, new Action<IServerBrowserFilterControl>(OnFilterValueChanged));
			allFilterControls.Add(xUiC_ServerBrowserGamePrefSelectorCombo);
		}
		XUiC_ServerBrowserGamePrefString[] childrenByType2 = GetChildrenByType<XUiC_ServerBrowserGamePrefString>();
		foreach (XUiC_ServerBrowserGamePrefString xUiC_ServerBrowserGamePrefString in childrenByType2)
		{
			if (xUiC_ServerBrowserGamePrefString.GameInfoString == GameInfoString.Language)
			{
				languageFilter = xUiC_ServerBrowserGamePrefString;
			}
			xUiC_ServerBrowserGamePrefString.OnValueChanged = (Action<IServerBrowserFilterControl>)Delegate.Combine(xUiC_ServerBrowserGamePrefString.OnValueChanged, new Action<IServerBrowserFilterControl>(OnFilterValueChanged));
			allFilterControls.Add(xUiC_ServerBrowserGamePrefString);
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (IsDirty)
		{
			IsDirty = false;
			RefreshBindings();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (!(_bindingName == "crossplayTooltip"))
		{
			if (_bindingName == "results")
			{
				_value = currentResults.ToString();
				return true;
			}
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
		_value = PermissionsManager.GetPermissionDenyReason(EUserPerms.Crossplay) ?? Localization.Get("xuiOptionsGeneralCrossplayTooltip");
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CloseFiltersButton_OnPress(XUiController _sender, int _mouseButton)
	{
		closeFilters();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ResetFiltersButton_OnPress(XUiController _sender, int _mouseButton)
	{
		ResetFilters();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BackButton_OnPress(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
		base.xui.playerUI.windowManager.Open(XUiC_MainMenu.ID, _bModal: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StartSearchButton_OnPress(XUiController _sender, int _mouseButton)
	{
		closeFilters();
		List<IServerListInterface.ServerFilter> list = new List<IServerListInterface.ServerFilter>();
		foreach (IServerBrowserFilterControl allFilterControl in allFilterControls)
		{
			XUiC_ServersList.UiServerFilter filter = allFilterControl.GetFilter();
			if (filter.Type != IServerListInterface.ServerFilter.EServerFilterType.Any)
			{
				list.Add(filter);
			}
		}
		ServerListManager.Instance.StartSearch(list);
	}

	public void StartShortcutPressed()
	{
		if (btnStartSearch.Enabled)
		{
			StartSearchButton_OnPress(null, 0);
		}
		else
		{
			CloseFiltersButton_OnPress(null, 0);
		}
	}

	public void ResetFilters()
	{
		foreach (IServerBrowserFilterControl allFilterControl in allFilterControls)
		{
			allFilterControl.Reset();
		}
		if (regionFilter != null)
		{
			string stringValue = GamePrefs.GetString(EnumGamePrefs.Region) ?? "";
			regionFilter.SelectEntry(new XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue(stringValue, null));
		}
		if (languageFilter != null)
		{
			string value = GamePrefs.GetString(EnumGamePrefs.LanguageBrowser) ?? "";
			languageFilter.SetValue(value);
		}
		ApplyForcedSettings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ApplyForcedSettings()
	{
		bool flag = PermissionsManager.IsCrossplayAllowed();
		crossplayFilter.Enabled = flag;
		if (!flag)
		{
			crossplayFilter.SelectEntry(new XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue(0, null));
		}
		if (!LaunchPrefs.AllowJoinConfigModded.Value)
		{
			moddedConfigsFilter.SelectEntry(new XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue(0, null));
			moddedConfigsFilter.ViewComponent.UiTransform.gameObject.SetActive(value: false);
		}
		if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
		{
			requiresModsFilter.SelectEntry(new XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue(0, null));
			requiresModsFilter.ViewComponent.UiTransform.gameObject.SetActive(value: false);
			if (Submission.Enabled)
			{
				eacFilter.SelectEntry(new XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue(1, null));
				eacFilter.ViewComponent.UiTransform.gameObject.SetActive(value: false);
				ignoreSanctionsFilter.SelectEntry(new XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue(0, null));
				ignoreSanctionsFilter.ViewComponent.UiTransform.gameObject.SetActive(value: false);
			}
		}
	}

	public void closeFilters()
	{
		windowGroup.Controller.GetChildByType<XUiC_ServerBrowser>().ShowingFilters = false;
		base.xui.playerUI.CursorController.SetNavigationLockView(null);
	}

	public void SetServersList(XUiC_ServersList _serversList)
	{
		_serversList.OnFilterResultsChanged += ServersList_OnFilterResultsChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ServersList_OnFilterResultsChanged(int _count)
	{
		currentResults = _count;
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnFilterValueChanged(IServerBrowserFilterControl _sender)
	{
		this.OnFilterChanged?.Invoke(_sender);
		IsDirty = true;
	}

	public override void OnVisibilityChanged(bool _isVisible)
	{
		base.OnVisibilityChanged(_isVisible);
		if (_isVisible && base.IsOpen)
		{
			SelectInitialElement();
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (!openedBefore)
		{
			openedBefore = true;
			ResetFilters();
		}
		else
		{
			ApplyForcedSettings();
		}
		RefreshBindings();
	}

	public void SelectInitialElement()
	{
		if (ServerListManager.Instance.IsPrefilteredSearch)
		{
			GetChildById("btnStartSearch").SelectCursorElement(_withDelay: true);
		}
		else
		{
			GetChildById("btnResetFilters").SelectCursorElement(_withDelay: true);
		}
	}

	public XUiView GetInitialSelectedElement()
	{
		if (ServerListManager.Instance.IsPrefilteredSearch)
		{
			return GetChildById("btnStartSearch").ViewComponent;
		}
		return GetChildById("btnResetFilters").ViewComponent;
	}

	public override void OnClose()
	{
		base.OnClose();
		if (regionFilter != null)
		{
			XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue selection = regionFilter.GetSelection();
			if (selection.Type == XUiC_ServerBrowserGamePrefSelectorCombo.EOptionValueType.String)
			{
				GamePrefs.Set(EnumGamePrefs.Region, selection.StringValue);
			}
		}
		if (languageFilter != null)
		{
			string value = languageFilter.GetValue();
			GamePrefs.Set(EnumGamePrefs.LanguageBrowser, value);
		}
		GamePrefs.Set(EnumGamePrefs.IgnoreEOSSanctions, _value: false);
	}
}
