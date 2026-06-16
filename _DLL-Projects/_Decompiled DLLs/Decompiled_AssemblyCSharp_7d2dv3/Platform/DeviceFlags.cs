using System.Runtime.CompilerServices;

namespace Platform;

public static class DeviceFlags
{
	public const DeviceFlag Standalone = DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX;

	public const DeviceFlag XBoxSeries = DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX;

	public const DeviceFlag PS5 = DeviceFlag.PS5;

	public const DeviceFlag Console = DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

	public const DeviceFlag All = DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

	public const DeviceFlag None = DeviceFlag.None;

	public const DeviceFlag Current = DeviceFlag.StandaloneWindows;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsCurrent(this DeviceFlag flags)
	{
		return (flags & DeviceFlag.StandaloneWindows) != 0;
	}
}
