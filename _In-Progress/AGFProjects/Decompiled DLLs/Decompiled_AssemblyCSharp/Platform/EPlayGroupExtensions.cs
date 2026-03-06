using System;
using System.Collections.Generic;

namespace Platform;

public static class EPlayGroupExtensions
{
	public static readonly EPlayGroup Current = GetCurrentPlayGroup();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<EPlayGroup, uint[]> s_playGroupToAllowedPlatformIds = new Dictionary<EPlayGroup, uint[]>
	{
		{
			EPlayGroup.Standalone,
			null
		},
		{
			EPlayGroup.XBS,
			null
		},
		{
			EPlayGroup.PS5,
			null
		}
	};

	public static bool IsCurrent(this EPlayGroup group)
	{
		return group == Current;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static EPlayGroup GetCurrentPlayGroup()
	{
		if (DeviceFlag.PS5.IsCurrent())
		{
			return EPlayGroup.PS5;
		}
		if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX).IsCurrent())
		{
			return EPlayGroup.XBS;
		}
		return EPlayGroup.Standalone;
	}

	public static EPlayGroup ToPlayGroup(this DeviceFlag device)
	{
		return device switch
		{
			DeviceFlag.StandaloneWindows => EPlayGroup.Standalone, 
			DeviceFlag.StandaloneOSX => EPlayGroup.Standalone, 
			DeviceFlag.StandaloneLinux => EPlayGroup.Standalone, 
			DeviceFlag.PS5 => EPlayGroup.PS5, 
			DeviceFlag.XBoxSeriesX => EPlayGroup.XBS, 
			DeviceFlag.XBoxSeriesS => EPlayGroup.XBS, 
			_ => throw new ArgumentOutOfRangeException("device", device, $"Missing play group mapping for {device}."), 
		};
	}

	public static EPlayGroup ToPlayGroup(this ClientInfo.EDeviceType deviceType)
	{
		return deviceType switch
		{
			ClientInfo.EDeviceType.Xbox => EPlayGroup.XBS, 
			ClientInfo.EDeviceType.PlayStation => EPlayGroup.PS5, 
			ClientInfo.EDeviceType.Linux => EPlayGroup.Standalone, 
			ClientInfo.EDeviceType.Mac => EPlayGroup.Standalone, 
			ClientInfo.EDeviceType.Windows => EPlayGroup.Standalone, 
			ClientInfo.EDeviceType.Unknown => EPlayGroup.Standalone, 
			_ => throw new ArgumentOutOfRangeException("deviceType", deviceType, $"Missing play group mapping for {deviceType}."), 
		};
	}

	public static uint[] GetCurrentlyAllowedPlatformIds()
	{
		if (PermissionsManager.IsCrossplayAllowed())
		{
			return null;
		}
		return s_playGroupToAllowedPlatformIds[Current];
	}
}
