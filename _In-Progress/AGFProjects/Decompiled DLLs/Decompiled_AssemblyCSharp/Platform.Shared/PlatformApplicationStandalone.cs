using System;
using System.Linq;
using UnityEngine;

namespace Platform.Shared;

public class PlatformApplicationStandalone : IPlatformApplication
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const string prefResolutionWidth = "Screenmanager Resolution Width";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string prefResolutionHeight = "Screenmanager Resolution Height";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string prefFullscreen = "Screenmanager Fullscreen mode";

	[PublicizedFrom(EAccessModifier.Private)]
	public const int minResWidth = 640;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int minResHeight = 480;

	[PublicizedFrom(EAccessModifier.Private)]
	public Resolution[] lastResolutions;

	[PublicizedFrom(EAccessModifier.Private)]
	public Resolution[] supportedResolutions;

	public Resolution[] SupportedResolutions
	{
		get
		{
			bool flag = false;
			if (lastResolutions != null)
			{
				Resolution[] resolutions = Screen.resolutions;
				if (lastResolutions.Length == resolutions.Length)
				{
					for (int i = 0; i < resolutions.Length; i++)
					{
						Resolution resolution = lastResolutions[i];
						Resolution resolution2 = resolutions[i];
						if (!resolution.Equals(resolution2))
						{
							flag = true;
							lastResolutions = resolutions;
							break;
						}
					}
				}
				else
				{
					lastResolutions = resolutions;
					flag = true;
				}
			}
			else
			{
				lastResolutions = Screen.resolutions;
				flag = true;
			}
			if (flag)
			{
				supportedResolutions = lastResolutions.Where([PublicizedFrom(EAccessModifier.Internal)] (Resolution res) => res.width >= 640 && res.height >= 480).ToArray();
			}
			return supportedResolutions;
		}
	}

	public (int width, int height, FullScreenMode fullScreenMode) ScreenOptions
	{
		get
		{
			FullScreenMode item = (FullScreenMode)SdPlayerPrefs.GetInt("Screenmanager Fullscreen mode", 3);
			if (SdPlayerPrefs.HasKey("Screenmanager Resolution Width") && SdPlayerPrefs.HasKey("Screenmanager Resolution Height"))
			{
				int item2 = SdPlayerPrefs.GetInt("Screenmanager Resolution Width");
				int item3 = SdPlayerPrefs.GetInt("Screenmanager Resolution Height");
				return (width: item2, height: item3, fullScreenMode: item);
			}
			Resolution[] array = SupportedResolutions;
			if (array.Length > 1)
			{
				Resolution resolution = array[^2];
				return (width: resolution.width, height: resolution.height, fullScreenMode: item);
			}
			return (width: Screen.width, height: Screen.height, fullScreenMode: FullScreenMode.Windowed);
		}
	}

	public string temporaryCachePath => Application.temporaryCachePath;

	public void SetResolution(int width, int height, FullScreenMode fullscreen)
	{
		if (width < 640 || height < 480 || width <= height)
		{
			fullscreen = FullScreenMode.Windowed;
			SdPlayerPrefs.SetInt("UnitySelectMonitor", 0);
		}
		if (height > width)
		{
			height = width;
		}
		SdPlayerPrefs.SetInt("Screenmanager Resolution Width", width);
		SdPlayerPrefs.SetInt("Screenmanager Resolution Height", height);
		SdPlayerPrefs.SetInt("Screenmanager Fullscreen mode", (int)fullscreen);
		Screen.SetResolution(width, height, fullscreen);
	}

	public void RestartProcess(params string[] argv)
	{
		throw new NotImplementedException();
	}
}
