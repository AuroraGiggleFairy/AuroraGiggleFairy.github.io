using System;
using System.IO;
using Newtonsoft.Json;

namespace SortingNearbyPlus
{
	internal static class SettingsManager
	{
		public const int RangeMin = 0;
		public const int RangeMax = 128;
		public const int VerticalMin = -1;

		public static ModSettings Settings { get; private set; } = new ModSettings();
		public static string Filename { get; private set; }

		public static int HorizontalRange => Settings.HorizontalRange;
		public static int VerticalRange => Settings.VerticalRange;
		public static bool LandClaimClamp => Settings.LandClaimClamp;

		public static void Load()
		{
			string modPath = GetModPath();
			if (string.IsNullOrEmpty(modPath))
			{
				Settings = new ModSettings();
				return;
			}

			Filename = Path.Combine(modPath, "SortConfiguration.json");

			try
			{
				if (File.Exists(Filename))
				{
					Settings = JsonConvert.DeserializeObject<ModSettings>(File.ReadAllText(Filename)) ?? new ModSettings();
				}
				else
				{
					Settings = new ModSettings();
					Save();
				}

				Settings.HorizontalRange = Clamp(Settings.HorizontalRange, RangeMin, RangeMax);
				Settings.VerticalRange = Settings.VerticalRange == -1 ? -1 : Clamp(Settings.VerticalRange, RangeMin, RangeMax);
			}
			catch (Exception ex)
			{
				Log.Warn("Failed to load settings: " + ex.Message);
				Settings = new ModSettings();
			}
		}

		public static void Save()
		{
			string modPath = GetModPath();
			if (string.IsNullOrEmpty(modPath))
			{
				return;
			}

			Filename = Path.Combine(modPath, "SortConfiguration.json");
			File.WriteAllText(Filename, JsonConvert.SerializeObject(Settings, Formatting.Indented));
		}

		public static int SetHorizontalRange(int value)
		{
			Settings.HorizontalRange = Clamp(value, RangeMin, RangeMax);
			Save();
			return Settings.HorizontalRange;
		}

		public static int SetVerticalRange(int value)
		{
			Settings.VerticalRange = value == -1 ? -1 : Clamp(value, RangeMin, RangeMax);
			Save();
			return Settings.VerticalRange;
		}

		public static bool SetLandClaimClamp(bool value)
		{
			Settings.LandClaimClamp = value;
			Save();
			return Settings.LandClaimClamp;
		}

		public static string AsString()
		{
			return "[SortingNearbyPlus] settings"
				+ "\n  range H=" + HorizontalRange + " V=" + VerticalRange
				+ "\n  landclaim=" + (LandClaimClamp ? "on" : "off")
				+ "\n  file=" + Filename;
		}

		private static string GetModPath()
		{
			return ModManager.GetMod("AGF-NoEAC-SortingNearbyPlus")?.Path;
		}

		private static int Clamp(int value, int min, int max)
		{
			if (value < min)
			{
				return min;
			}

			return value > max ? max : value;
		}
	}
}
