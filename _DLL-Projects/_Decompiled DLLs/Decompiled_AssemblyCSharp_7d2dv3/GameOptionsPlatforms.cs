using Platform;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public static class GameOptionsPlatforms
{
	public enum GfxPreset
	{
		Lowest = 0,
		Low = 1,
		Medium = 2,
		High = 3,
		Ultra = 4,
		Custom = 5,
		ConsolePerformance = 6,
		LEGACY_ConsolePerformanceFSR = 7,
		ConsoleQuality = 8,
		LEGACY_ConsoleQualityFSR = 9,
		Simplified = 100
	}

	public static class UpscalerMode
	{
		public const int Off = 0;

		public const int FSR2 = 1;

		public const int FSR3 = 2;

		public const int DynamicResolution = 3;

		public const int Scale = 4;

		public const int DLSS = 5;

		public static string ToString(int upscalerSettingValue)
		{
			return upscalerSettingValue switch
			{
				0 => "Off", 
				1 => "FSR2", 
				2 => "FSR3", 
				3 => "Dynamic Resolution", 
				4 => "Scale", 
				5 => "DLSS", 
				_ => "Unknown", 
			};
		}
	}

	public static int DefaultUpscalerMode
	{
		get
		{
			if (!FSR3.FSR3Supported())
			{
				return 4;
			}
			return 2;
		}
	}

	public static float GetStreamingMipmapBudget()
	{
		return (float)SystemInfo.graphicsMemorySize * 0.9f;
	}

	public static string GetItemIconFilterString()
	{
		string result = "mip0";
		if ((float)SystemInfo.graphicsMemorySize <= 3200f || SystemInfo.systemMemorySize < 6800)
		{
			result = "mip1";
		}
		return result;
	}

	public static int CalcTextureQualityMin()
	{
		int systemMemorySize = SystemInfo.systemMemorySize;
		if (SystemInfo.graphicsMemorySize < 2400 || systemMemorySize < 4900)
		{
			return 1;
		}
		return 0;
	}

	public static GfxPreset CalcDefaultGfxPreset()
	{
		if ((DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX).IsCurrent())
		{
			GfxPreset result = GfxPreset.Medium;
			float num = SystemInfo.systemMemorySize;
			float num2 = SystemInfo.graphicsMemorySize;
			if (!SystemInfo.operatingSystem.Contains(" Steam ") && (num2 < 2400f || num < 4800f))
			{
				result = GfxPreset.Low;
			}
			if (num2 > 7500f && num > 5200f)
			{
				string text = SystemInfo.graphicsDeviceVendor.ToLower();
				if (text.Contains("nvidia"))
				{
					if (!FindGfxName(" 1070, 305"))
					{
						result = GfxPreset.High;
						if (FindGfxName(" 208, 307, 308, 309, 407, 408, 409, 507, 508, 509"))
						{
							result = GfxPreset.Ultra;
						}
					}
				}
				else if ((text == "amd" || text == "ati") && !FindGfxName("RX 570,RX 580,RX 590,RX 5500,RX 65,RX 66"))
				{
					result = GfxPreset.High;
					if (FindGfxName(" 680, 690, 695, 770, 780, 790, 907, 908, 909"))
					{
						result = GfxPreset.Ultra;
					}
				}
			}
			return result;
		}
		if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
		{
			return GfxPreset.ConsolePerformance;
		}
		return GfxPreset.Medium;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool FindGfxName(string names)
	{
		string text = SystemInfo.graphicsDeviceName.ToLower();
		if (text.Contains("laptop"))
		{
			return false;
		}
		string[] array = names.Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			if (text.Contains(array[i]))
			{
				return true;
			}
		}
		return false;
	}

	public static int ApplyTextureFilterLimit(int filter)
	{
		if ((float)SystemInfo.graphicsMemorySize < 3200f)
		{
			filter = 0;
		}
		return filter;
	}
}
