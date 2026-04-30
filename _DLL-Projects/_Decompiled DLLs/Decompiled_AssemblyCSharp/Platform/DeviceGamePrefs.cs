using System.IO;

namespace Platform;

public static class DeviceGamePrefs
{
	public static void Apply()
	{
		if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
		{
			int num = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxQualityPreset);
			if (num != 6 && num != 8)
			{
				Log.Out($"[DeviceGamePrefs] Quality preset \"{num}\" is unsupported on this platform; defaulting to ConsolePerformance.");
				GamePrefs.Set(EnumGamePrefs.OptionsGfxQualityPreset, 6);
			}
			int num2 = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxUpscalerMode);
			if (num2 != 2 && num2 != 4)
			{
				Log.Out($"[DeviceGamePrefs] Upscaler mode \"{num2}\" is unsupported on this platform; defaulting to \"{GameOptionsPlatforms.DefaultUpscalerMode}\".");
				GamePrefs.Set(EnumGamePrefs.OptionsGfxUpscalerMode, GameOptionsPlatforms.DefaultUpscalerMode);
			}
			GameOptionsManager.SetGraphicsQuality();
		}
		ApplyConfigFilePrefs();
	}

	public static string ConfigFilename(string _deviceName)
	{
		return "gameprefs_" + _deviceName;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ApplyConfigFilePrefs()
	{
		string text = ConfigFilename(DeviceFlag.StandaloneWindows.GetDeviceName());
		string text2 = Path.Combine(GameIO.GetApplicationPath(), text + ".xml");
		if (File.Exists(text2))
		{
			Log.Out("[DeviceGamePrefs] Applying game prefs from {0}", text2);
			DynamicProperties dynamicProperties = new DynamicProperties();
			if (dynamicProperties.Load(GameIO.GetApplicationPath(), text))
			{
				ApplyGamePrefs(dynamicProperties);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ApplyGamePrefs(DynamicProperties properties)
	{
		foreach (string key in properties.Values.Dict.Keys)
		{
			if (EnumUtils.TryParse<EnumGamePrefs>(key, out var _result, _ignoreCase: true))
			{
				object obj = GamePrefs.Parse(_result, properties.Values[key]);
				if (obj != null)
				{
					GamePrefs.SetObject(_result, obj);
					Log.Out("[DeviceGamePrefs] {0}={1}", key, GamePrefs.GetObject(_result));
				}
				else
				{
					Log.Error("[DeviceGamePrefs] Invalid value for GamePref: {0}", key);
				}
			}
			else
			{
				Log.Error("[DeviceGamePrefs] Unknown GamePref: {0}", key);
			}
		}
	}
}
