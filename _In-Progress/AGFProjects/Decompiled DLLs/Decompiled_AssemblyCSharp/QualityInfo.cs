using UnityEngine;

public class QualityInfo
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static Color[] qualityColors = new Color[7];

	[PublicizedFrom(EAccessModifier.Private)]
	public static string[] hexColors = new string[7];

	public static void Cleanup()
	{
		qualityColors = new Color[7];
		hexColors = new string[7];
	}

	public static void Add(int _key, string _hexColor)
	{
		qualityColors[_key] = HexToRGB(_hexColor);
		hexColors[_key] = _hexColor;
	}

	public static Color GetQualityColor(int _quality)
	{
		return GetTierColor(_quality);
	}

	public static Color GetTierColor(int _tier)
	{
		if (_tier > qualityColors.Length - 1)
		{
			_tier = qualityColors.Length - 1;
		}
		return qualityColors[_tier];
	}

	public static string GetQualityColorHex(int _quality)
	{
		if (_quality > qualityColors.Length - 1)
		{
			_quality = qualityColors.Length - 1;
		}
		return hexColors[_quality];
	}

	public static string GetQualityLevelName(int _quality, bool _useQualityColor = false)
	{
		if (_quality == 0)
		{
			return Localization.Get("lblQualityBroken");
		}
		string text = "";
		_quality /= 1;
		switch (_quality)
		{
		case 0:
			text = Localization.Get("lblQualityDamaged");
			break;
		case 1:
			text = Localization.Get("lblQualityPoor");
			break;
		case 2:
			text = Localization.Get("lblQualityAverage");
			break;
		case 3:
			text = Localization.Get("lblQualityGreat");
			break;
		case 4:
			text = Localization.Get("lblQualityFlawless");
			break;
		case 5:
			text = Localization.Get("lblQualityLegendary");
			break;
		case 6:
			text = Localization.Get("lblQualityLegendary");
			break;
		}
		if (_useQualityColor)
		{
			text = $"[{GetQualityColorHex(_quality)}]{text}[-]";
		}
		return text;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int HexToInt(char hexChar)
	{
		return hexChar switch
		{
			'0' => 0, 
			'1' => 1, 
			'2' => 2, 
			'3' => 3, 
			'4' => 4, 
			'5' => 5, 
			'6' => 6, 
			'7' => 7, 
			'8' => 8, 
			'9' => 9, 
			'A' => 10, 
			'B' => 11, 
			'C' => 12, 
			'D' => 13, 
			'E' => 14, 
			'F' => 15, 
			_ => -1, 
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Color HexToRGB(string color)
	{
		color.Replace("#", "");
		float r = ((float)HexToInt(color[1]) + (float)HexToInt(color[0]) * 16f) / 255f;
		float g = ((float)HexToInt(color[3]) + (float)HexToInt(color[2]) * 16f) / 255f;
		float b = ((float)HexToInt(color[5]) + (float)HexToInt(color[4]) * 16f) / 255f;
		return new Color
		{
			r = r,
			g = g,
			b = b,
			a = 1f
		};
	}
}
