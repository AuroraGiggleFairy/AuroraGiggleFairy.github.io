using InControl;
using Platform.Shared;
using Steamworks;

namespace Platform.Steam;

public class Utils : IUtils
{
	[PublicizedFrom(EAccessModifier.Private)]
	public IPlatform owner;

	public void Init(IPlatform _owner)
	{
		owner = _owner;
	}

	public bool OpenBrowser(string _url)
	{
		if (global::Utils.IsValidWebUrl(ref _url))
		{
			SteamFriends.ActivateGameOverlayToWebPage(_url);
		}
		return true;
	}

	public string GetPlatformLanguage()
	{
		if (GameManager.IsDedicatedServer)
		{
			return "english";
		}
		if (owner.Api.ClientApiStatus != EApiStatus.Ok)
		{
			Log.Warning("[Steam] Unable to get platform language, Steam not initialized");
			return "english";
		}
		string text = SteamUtils.GetSteamUILanguage().ToLower();
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
		return "english";
	}

	public string GetAppLanguage()
	{
		if (GameManager.IsDedicatedServer)
		{
			return "english";
		}
		if (owner.Api.ClientApiStatus != EApiStatus.Ok)
		{
			Log.Warning("[Steam] Unable to get app language, Steam not initialized");
			return "english";
		}
		string text = SteamApps.GetCurrentGameLanguage().ToLower();
		if (text == "latam")
		{
			text = "spanish";
		}
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
		return "english";
	}

	public string GetCountry()
	{
		if (GameManager.IsDedicatedServer)
		{
			return "??";
		}
		if (owner.Api.ClientApiStatus == EApiStatus.Ok)
		{
			return SteamUtils.GetIPCountry();
		}
		Log.Warning("[Steam] Unable to get country, Steam not initialized");
		return "??";
	}

	public void ClearTempFiles()
	{
		Platform.Shared.Utils.TryDeleteTempCacheContents();
	}

	public string GetTempFileName(string prefix = "", string suffix = "")
	{
		return Platform.Shared.Utils.GetRandomTempCacheFileName(prefix, suffix);
	}

	public void ControllerDisconnected(InputDevice inputDevice)
	{
	}

	public string GetCrossplayPlayerIcon(EPlayGroup _playGroup, bool _fetchGenericIcons, EPlatformIdentifier _nativePlatform = EPlatformIdentifier.None)
	{
		switch (_playGroup)
		{
		case EPlayGroup.Standalone:
			if (_fetchGenericIcons)
			{
				return "ui_platform_pc";
			}
			break;
		case EPlayGroup.XBS:
			if (_fetchGenericIcons)
			{
				return "ui_platform_console";
			}
			break;
		case EPlayGroup.PS5:
			if (_fetchGenericIcons)
			{
				return "ui_platform_console";
			}
			break;
		}
		return string.Empty;
	}
}
