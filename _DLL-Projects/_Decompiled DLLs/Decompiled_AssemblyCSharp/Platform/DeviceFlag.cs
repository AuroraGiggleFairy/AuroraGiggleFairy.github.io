using System;

namespace Platform;

[Flags]
public enum DeviceFlag
{
	None = 0,
	StandaloneWindows = 1,
	StandaloneLinux = 2,
	StandaloneOSX = 4,
	XBoxSeriesS = 8,
	XBoxSeriesX = 0x10,
	PS5 = 0x20
}
