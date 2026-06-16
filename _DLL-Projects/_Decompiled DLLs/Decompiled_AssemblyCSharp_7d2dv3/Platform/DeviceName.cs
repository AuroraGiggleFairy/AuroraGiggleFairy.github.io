namespace Platform;

public static class DeviceName
{
	public const string StandaloneWindows = "Windows";

	public const string StandaloneLinux = "Linux";

	public const string StandaloneOSX = "OSX";

	public const string PS5 = "PS5";

	public const string XBoxSeriesS = "XBoxSeriesS";

	public const string XBoxSeriesX = "XBoxSeriesX";

	public static string GetDeviceName(this DeviceFlag _deviceId)
	{
		switch (_deviceId)
		{
		case DeviceFlag.StandaloneWindows:
			return "Windows";
		case DeviceFlag.StandaloneLinux:
			return "Linux";
		case DeviceFlag.StandaloneOSX:
			return "OSX";
		case DeviceFlag.XBoxSeriesS:
			return "XBoxSeriesS";
		case DeviceFlag.XBoxSeriesX:
			return "XBoxSeriesX";
		case DeviceFlag.PS5:
			return "PS5";
		default:
			Log.Warning($"Device name for flag '{_deviceId}' is unknown");
			return string.Empty;
		}
	}
}
