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
	public bool openedBefore;

	[XuiBindParent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ServerBrowser parentBrowser;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_FullScreenCollider fullScreenCollider;

	[XuiBindComponent("btnStartSearch", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnStartSearch;

	[XuiBindComponent("btnResetFilters", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnResetFilters;

	[XuiBindComponent("btnBack", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnBack;

	[XuiBindComponent("btnCloseFilters", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnCloseFilters;

	[XuiXmlBinding("results")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public int ResultCount
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlBinding("crossplaytooltip")]
	public string CrossplayTooltip
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return PermissionsManager.GetPermissionDenyReason(EUserPerms.Crossplay) ?? Localization.Get("xuiOptionsGeneralCrossplayTooltip");
		}
	}

	public event Action<IServerBrowserFilterControl> OnFilterChanged;

	public override void Init()
	{
		base.Init();
		XUiC_ServerBrowserGamePrefSelectorCombo[] childrenByType = GetChildrenByType<XUiC_ServerBrowserGamePrefSelectorCombo>();
		foreach (XUiC_ServerBrowserGamePrefSelectorCombo xUiC_ServerBrowserGamePrefSelectorCombo in childrenByType)
		{
			switch (xUiC_ServerBrowserGamePrefSelectorCombo.GameInfoInt)
			{
			case GameInfoInt.AirDropFrequency:
				xUiC_ServerBrowserGamePrefSelectorCombo.ValuePreDisplayModifierFunc = [PublicizedFrom(EAccessModifier.Internal)] (int _n) => _n;
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
		base.OnVisiblity += OnVisibilityChanged;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		base.OnVisiblity -= OnVisibilityChanged;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		handleDirtyUpdateDefault();
		if (XUiUtils.HotkeysAllowedFor(base.ViewComponent))
		{
			if (xui.playerUI.playerInput.GUIActions.Apply.WasReleased)
			{
				ThreadManager.RunTaskAfterFrames(startShortcutPressed);
			}
			if (xui.playerUI.playerInput.GUIActions.Cancel.WasReleased)
			{
				ThreadManager.RunTaskAfterFrames(backShortcutPressed);
			}
		}
	}

	[XuiBindEvent("OnPress", "fullScreenCollider")]
	[XuiBindEvent("OnPress", "btnCloseFilters")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void CloseFiltersButton_OnPress(XUiController _sender, int _mouseButton)
	{
		CloseFilters();
	}

	[XuiBindEvent("OnPress", "btnResetFilters")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void ResetFiltersButton_OnPress(XUiController _sender, int _mouseButton)
	{
		resetFilters();
	}

	[XuiBindEvent("OnPress", "btnBack")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BackButton_OnPress(XUiController _sender, int _mouseButton)
	{
		parentBrowser.CloseBrowser();
	}

	[XuiBindEvent("OnPress", "btnStartSearch")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void StartSearchButton_OnPress(XUiController _sender, int _mouseButton)
	{
		CloseFilters();
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

	[PublicizedFrom(EAccessModifier.Private)]
	public void backShortcutPressed()
	{
		if (btnBack.ViewComponent.IsActiveInHierarchy)
		{
			BackButton_OnPress(null, 0);
		}
		else
		{
			CloseFiltersButton_OnPress(null, 0);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void startShortcutPressed()
	{
		if (btnStartSearch.ViewComponent.Enabled)
		{
			StartSearchButton_OnPress(null, 0);
		}
		else
		{
			CloseFiltersButton_OnPress(null, 0);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void resetFilters()
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
		applyForcedSettings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void applyForcedSettings()
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
			moddedConfigsFilter.ViewComponent.IsVisible = false;
		}
		if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
		{
			requiresModsFilter.SelectEntry(new XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue(0, null));
			requiresModsFilter.ViewComponent.IsVisible = false;
			if (Submission.Enabled)
			{
				eacFilter.SelectEntry(new XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue(1, null));
				eacFilter.ViewComponent.IsVisible = false;
				ignoreSanctionsFilter.SelectEntry(new XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue(0, null));
				ignoreSanctionsFilter.ViewComponent.IsVisible = false;
			}
		}
	}

	public void CloseFilters()
	{
		windowGroup.Controller.GetChildByType<XUiC_ServerBrowser>().ShowingFilters = false;
		xui.playerUI.CursorController.SetNavigationLockView(null);
	}

	public void SetServersList(XUiC_ServersList _serversList)
	{
		_serversList.OnFilterResultsChanged += ServersList_OnFilterResultsChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ServersList_OnFilterResultsChanged(int _count)
	{
		ResultCount = _count;
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnFilterValueChanged(IServerBrowserFilterControl _sender)
	{
		this.OnFilterChanged?.Invoke(_sender);
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnVisibilityChanged(XUiController _sender, bool _visibleSelf, bool _visibleInScene)
	{
		if (_visibleInScene)
		{
			selectInitialElement();
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (!openedBefore)
		{
			openedBefore = true;
			resetFilters();
		}
		else
		{
			applyForcedSettings();
		}
		RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void selectInitialElement()
	{
		GetInitialSelectedElement().Controller.SelectCursorElement(_withDelay: true);
	}

	public XUiView GetInitialSelectedElement()
	{
		return GetChildById(ServerListManager.Instance.IsPrefilteredSearch ? "btnStartSearch" : "btnResetFilters").ViewComponent;
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
