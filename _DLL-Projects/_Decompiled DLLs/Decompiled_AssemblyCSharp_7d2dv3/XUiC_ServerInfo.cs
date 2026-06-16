using System.Collections.Generic;
using SandboxOptions;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ServerInfo : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum DisplayMode
	{
		Dedicated,
		Peer
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<XUiC_ServerBrowserGamePrefInfo> infoFields = new List<XUiC_ServerBrowserGamePrefInfo>();

	[XuiBindComponent("ServerWebsiteURL", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiController lblServerUrl;

	[PublicizedFrom(EAccessModifier.Private)]
	public DisplayMode displayMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public string unfilteredUrlText;

	[PublicizedFrom(EAccessModifier.Private)]
	public string serverUrl;

	[PublicizedFrom(EAccessModifier.Private)]
	public string serverDescription;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool serverUrlHovered;

	[PublicizedFrom(EAccessModifier.Private)]
	public string serverSandboxCode;

	[XuiXmlBinding("serverurl")]
	public string ServerUrl
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return serverUrl ?? "";
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			serverUrl = value;
			IsDirty = true;
		}
	}

	[XuiXmlBinding("serverurlhovered")]
	public bool ServerUrlHovered
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return serverUrlHovered;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			serverUrlHovered = value;
			IsDirty = true;
		}
	}

	[XuiXmlBinding("serverdescription")]
	public string ServerDescription
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return serverDescription ?? "";
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			serverDescription = value;
			IsDirty = true;
		}
	}

	[XuiXmlBinding("serversandboxcode")]
	public string ServerSandboxCode
	{
		get
		{
			return serverSandboxCode ?? "";
		}
		set
		{
			serverSandboxCode = value;
			IsDirty = true;
		}
	}

	[XuiXmlBinding("sandboxpreset")]
	public string SandboxPreset
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			SandboxOptionPreset presetByCode = SandboxOptionManager.Current.GetPresetByCode(ServerSandboxCode);
			if (presetByCode == null)
			{
				return Localization.Get("sandboxPresetGroupUser");
			}
			if (presetByCode != null && !presetByCode.IsUserPreset && !presetByCode.IsCustomPreset && !presetByCode.IsModded)
			{
				return presetByCode.DisplayName;
			}
			return Localization.Get("sandboxPresetGroupUser");
		}
	}

	[XuiXmlBinding("showip")]
	public bool ShowIp
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return displayMode == DisplayMode.Dedicated;
		}
	}

	public override void Init()
	{
		base.Init();
		XUiC_ServerBrowserGamePrefInfo[] childrenByType = GetChildrenByType<XUiC_ServerBrowserGamePrefInfo>();
		foreach (XUiC_ServerBrowserGamePrefInfo xUiC_ServerBrowserGamePrefInfo in childrenByType)
		{
			infoFields.Add(xUiC_ServerBrowserGamePrefInfo);
			if (xUiC_ServerBrowserGamePrefInfo.ValueType == GamePrefs.EnumType.Int)
			{
				switch (xUiC_ServerBrowserGamePrefInfo.GameInfoInt)
				{
				case GameInfoInt.CurrentServerTime:
					xUiC_ServerBrowserGamePrefInfo.CustomIntValueFormatter = [PublicizedFrom(EAccessModifier.Internal)] (GameServerInfo _, int _worldTime) => ValueDisplayFormatters.WorldTime((ulong)_worldTime, Localization.Get("xuiDayTimeLong"));
					break;
				case GameInfoInt.AirDropFrequency:
					xUiC_ServerBrowserGamePrefInfo.CustomIntValueFormatter = [PublicizedFrom(EAccessModifier.Internal)] (GameServerInfo _, int _value) => string.Format(Localization.Get("goAirDropValue" + ((_value == 1) ? "" : "s")), _value);
					break;
				}
			}
			else if (xUiC_ServerBrowserGamePrefInfo.ValueType == GamePrefs.EnumType.String && xUiC_ServerBrowserGamePrefInfo.GameInfoString == GameInfoString.ServerVersion)
			{
				xUiC_ServerBrowserGamePrefInfo.CustomStringValueFormatter = [PublicizedFrom(EAccessModifier.Internal)] (GameServerInfo _info, string _s) => (_info.Version.Build != 0) ? ((_info.IsCompatibleVersion ? "" : "[ff0000]") + _info.Version.LongString) : ("[ff0000]" + _s);
			}
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		handleDirtyUpdateDefault();
	}

	public void InitializeForListFilter(XUiC_ServersList.EnumServerLists _mode)
	{
		if (_mode == XUiC_ServersList.EnumServerLists.Peer || _mode == XUiC_ServersList.EnumServerLists.Friends || _mode == XUiC_ServersList.EnumServerLists.History)
		{
			setDisplayMode(DisplayMode.Peer);
		}
		else
		{
			setDisplayMode(DisplayMode.Dedicated);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setDisplayMode(DisplayMode _mode)
	{
		if (displayMode != _mode)
		{
			displayMode = _mode;
			IsDirty = true;
		}
	}

	public void SetServerInfo(GameServerInfo _gameInfo)
	{
		foreach (XUiC_ServerBrowserGamePrefInfo infoField in infoFields)
		{
			infoField.SetCurrentValue(_gameInfo);
		}
		ServerDescription = "";
		ServerUrl = "";
		unfilteredUrlText = "";
		string value = ((_gameInfo == null) ? "" : GeneratedTextManager.GetDisplayTextImmediately(_gameInfo.ServerDisplayName, _checkBlockState: false, GeneratedTextManager.TextFilteringMode.Filter, GeneratedTextManager.BbCodeSupportMode.NotSupported));
		GameStats.Set(EnumGameStats.SandboxPreset, value);
		ServerSandboxCode = ((_gameInfo == null) ? "" : _gameInfo.GetValue(GameInfoString.SandboxCode));
		if (_gameInfo == null)
		{
			return;
		}
		AuthoredText serverDisplayName = _gameInfo.ServerDescription;
		if (_gameInfo.IsDedicated || _gameInfo.IsLAN)
		{
			setDisplayMode(DisplayMode.Dedicated);
		}
		else
		{
			setDisplayMode(DisplayMode.Peer);
			if (string.IsNullOrEmpty(serverDisplayName.Text))
			{
				serverDisplayName = _gameInfo.ServerDisplayName;
			}
		}
		unfilteredUrlText = _gameInfo.ServerURL.Text;
		GeneratedTextManager.GetDisplayText(serverDisplayName, [PublicizedFrom(EAccessModifier.Private)] (string _desc) =>
		{
			ServerDescription = _desc;
		}, _runCallbackIfReadyNow: true, _checkBlockState: false, GeneratedTextManager.TextFilteringMode.FilterWithSafeString, GeneratedTextManager.BbCodeSupportMode.Supported);
		GeneratedTextManager.GetDisplayText(_gameInfo.ServerURL, [PublicizedFrom(EAccessModifier.Private)] (string _url) =>
		{
			ServerUrl = _url;
		}, _runCallbackIfReadyNow: true, _checkBlockState: false, GeneratedTextManager.TextFilteringMode.FilterWithSafeString);
	}

	[XuiBindEvent("OnHover", "lblServerUrl")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void UrlController_OnHover(XUiController _sender, bool _isOver)
	{
		ServerUrlHovered = _isOver;
	}

	[XuiBindEvent("OnPress", "lblServerUrl")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void UrlController_OnPress(XUiController _sender, int _mouseButton)
	{
		XUiC_MessageBoxWindowGroup.ShowUrlConfirmationDialog(xui, unfilteredUrlText, _modal: false, null, null, null, ServerUrl);
	}
}
