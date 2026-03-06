using System.Collections.Generic;
using UnityEngine;
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

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblServerDescription;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblServerUrl;

	[PublicizedFrom(EAccessModifier.Private)]
	public string unfilteredUrlText;

	[PublicizedFrom(EAccessModifier.Private)]
	public DisplayMode displayMode;

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
					xUiC_ServerBrowserGamePrefInfo.CustomIntValueFormatter = [PublicizedFrom(EAccessModifier.Internal)] (GameServerInfo _info, int _worldTime) => ValueDisplayFormatters.WorldTime((ulong)_worldTime, Localization.Get("xuiDayTimeLong"));
					break;
				case GameInfoInt.AirDropFrequency:
					xUiC_ServerBrowserGamePrefInfo.CustomIntValueFormatter = [PublicizedFrom(EAccessModifier.Internal)] (GameServerInfo _info, int _value) =>
					{
						string text;
						if (_value % 24 == 0)
						{
							text = "goAirDropValue";
							_value /= 24;
						}
						else
						{
							text = "goAirDropValueHour";
						}
						return string.Format(Localization.Get(text + ((_value == 1) ? "" : "s")), _value);
					};
					break;
				}
			}
			else if (xUiC_ServerBrowserGamePrefInfo.ValueType == GamePrefs.EnumType.String && xUiC_ServerBrowserGamePrefInfo.GameInfoString == GameInfoString.ServerVersion)
			{
				xUiC_ServerBrowserGamePrefInfo.CustomStringValueFormatter = [PublicizedFrom(EAccessModifier.Internal)] (GameServerInfo _info, string _s) => (_info.Version.Build != 0) ? ((_info.IsCompatibleVersion ? "" : "[ff0000]") + _info.Version.LongString) : ("[ff0000]" + _s);
			}
		}
		XUiController childById = GetChildById("ServerDescription");
		lblServerDescription = (XUiV_Label)childById.ViewComponent;
		XUiController childById2 = GetChildById("ServerWebsiteURL");
		lblServerUrl = (XUiV_Label)childById2.ViewComponent;
		childById2.OnHover += UrlController_OnHover;
		childById2.OnPress += UrlController_OnPress;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		lblServerUrl.IsNavigatable = !string.IsNullOrEmpty(lblServerUrl.Text);
	}

	public void InitializeForListFilter(XUiC_ServersList.EnumServerLists mode)
	{
		if (mode == XUiC_ServersList.EnumServerLists.Peer || mode == XUiC_ServersList.EnumServerLists.Friends || mode == XUiC_ServersList.EnumServerLists.History)
		{
			SetDisplayMode(DisplayMode.Peer);
		}
		else
		{
			SetDisplayMode(DisplayMode.Dedicated);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetDisplayMode(DisplayMode _mode)
	{
		if (displayMode != _mode)
		{
			displayMode = _mode;
			RefreshBindings();
		}
	}

	public void SetServerInfo(GameServerInfo _gameInfo)
	{
		foreach (XUiC_ServerBrowserGamePrefInfo infoField in infoFields)
		{
			infoField.SetCurrentValue(_gameInfo);
		}
		if (_gameInfo == null)
		{
			lblServerDescription.Text = "";
			lblServerUrl.Text = "";
			unfilteredUrlText = "";
			lblServerUrl.IsNavigatable = false;
			return;
		}
		AuthoredText authoredText = _gameInfo.ServerDescription;
		if (_gameInfo.IsDedicated || _gameInfo.IsLAN)
		{
			SetDisplayMode(DisplayMode.Dedicated);
		}
		else
		{
			SetDisplayMode(DisplayMode.Peer);
			if (string.IsNullOrEmpty(authoredText.Text))
			{
				authoredText = _gameInfo.ServerDisplayName;
			}
		}
		unfilteredUrlText = _gameInfo.ServerURL.Text;
		GeneratedTextManager.GetDisplayText(authoredText, [PublicizedFrom(EAccessModifier.Private)] (string desc) =>
		{
			lblServerDescription.Text = desc;
		}, _runCallbackIfReadyNow: true, _checkBlockState: false, GeneratedTextManager.TextFilteringMode.FilterWithSafeString, GeneratedTextManager.BbCodeSupportMode.Supported);
		GeneratedTextManager.GetDisplayText(_gameInfo.ServerURL, [PublicizedFrom(EAccessModifier.Private)] (string url) =>
		{
			lblServerUrl.Text = url;
		}, _runCallbackIfReadyNow: true, _checkBlockState: false, GeneratedTextManager.TextFilteringMode.FilterWithSafeString);
		lblServerUrl.IsNavigatable = !string.IsNullOrEmpty(_gameInfo.ServerURL.Text);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UrlController_OnHover(XUiController _sender, bool _isOver)
	{
		lblServerUrl.Color = (_isOver ? ((Color)new Color32(250, byte.MaxValue, 163, byte.MaxValue)) : Color.white);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UrlController_OnPress(XUiController _sender, int _mouseButton)
	{
		XUiC_MessageBoxWindowGroup.ShowUrlConfirmationDialog(base.xui, unfilteredUrlText, _modal: false, null, null, null, lblServerUrl.Text);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (_bindingName == "showip")
		{
			switch (displayMode)
			{
			case DisplayMode.Dedicated:
				_value = true.ToString();
				break;
			case DisplayMode.Peer:
				_value = false.ToString();
				break;
			}
			return true;
		}
		return base.GetBindingValueInternal(ref _value, _bindingName);
	}
}
