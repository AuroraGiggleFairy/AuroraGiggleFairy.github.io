namespace Platform;

public static class DeviceFlags
{
	public const DeviceFlag Standalone = DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX;

	public const DeviceFlag XBoxSeries = DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX;

	public const DeviceFlag PS5 = DeviceFlag.PS5;

	public const DeviceFlag Console = DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

	public const DeviceFlag All = DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

	public const DeviceFlag None = DeviceFlag.None;

	[PublicizedFrom(EAccessModifier.Private)]
	public static DeviceFlag? m_current;

	public static DeviceFlag Current
	{
		get
		{
			DeviceFlag valueOrDefault = m_current.GetValueOrDefault();
			if (!m_current.HasValue)
			{
				valueOrDefault = GetCurrentDeviceFlag();
				m_current = valueOrDefault;
				return valueOrDefault;
			}
			return valueOrDefault;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static DeviceFlag GetCurrentDeviceFlag()
	{
		return DeviceFlag.StandaloneWindows;
	}

	public static bool IsCurrent(this DeviceFlag flags)
	{
		return flags.HasFlag(Current);
	}
}
