using UnityEngine;

namespace PartyGroupPlus
{
	public static class PartyGroupPlusPalette
	{
		public const int Count = 192;
		public const int HueCount = 16;
		public const int ShadeCount = 12;
		public const int BrightSteps = 4;

		const float MinBrightness = 0.60f;
		const float MaxBrightness = 0.95f;
		const float VividSat = 0.92f;
		const float MiddleSat = 0.66f;
		const float SoftSat = 0.40f;

		static readonly Color32[] Colors = Build();

		public static Color32 Get(int index)
		{
			if (index < 0)
			{
				index = 0;
			}

			return Colors[index % Count];
		}

		public static string ToBinding(int index)
		{
			Color32 c = Get(index);
			return c.r + "," + c.g + "," + c.b + ",255";
		}

		static Color32[] Build()
		{
			Color32[] colors = new Color32[Count];
			float[] sats = { VividSat, MiddleSat, SoftSat };
			for (int hue = 0; hue < HueCount; hue++)
			{
				float h = hue / (float)HueCount;
				for (int shade = 0; shade < ShadeCount; shade++)
				{
					int satBand = shade / BrightSteps;
					int bright = shade % BrightSteps;
					float s = sats[satBand];
					float v = Mathf.Lerp(MinBrightness, MaxBrightness, bright / (BrightSteps - 1f));
					Color c = Color.HSVToRGB(h, s, v);
					colors[hue * ShadeCount + shade] = c;
				}
			}

			return colors;
		}
	}
}
