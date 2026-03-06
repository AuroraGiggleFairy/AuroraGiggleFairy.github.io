using System;
using UnityEngine;

public class MicroSplatPropData : ScriptableObject
{
	public enum PerTexVector2
	{
		SplatUVScale = 0,
		SplatUVOffset = 2
	}

	public enum PerTexColor
	{
		Tint = 4,
		SSSRTint = 72
	}

	public enum PerTexFloat
	{
		InterpolationContrast = 5,
		NormalStrength = 8,
		Smoothness = 9,
		AO = 10,
		Metallic = 11,
		Brightness = 12,
		Contrast = 13,
		Porosity = 14,
		Foam = 15,
		DetailNoiseStrength = 16,
		DistanceNoiseStrength = 17,
		DistanceResample = 18,
		DisplacementMip = 19,
		GeoTexStrength = 20,
		GeoTintStrength = 21,
		GeoNormalStrength = 22,
		GlobalSmoothMetalAOStength = 23,
		DisplacementStength = 24,
		DisplacementBias = 25,
		DisplacementOffset = 26,
		GlobalEmisStength = 27,
		NoiseNormal0Strength = 28,
		NoiseNormal1Strength = 29,
		NoiseNormal2Strength = 30,
		WindParticulateStrength = 31,
		SnowAmount = 32,
		GlitterAmount = 33,
		GeoHeightFilter = 34,
		GeoHeightFilterStrength = 35,
		TriplanarMode = 36,
		TriplanarContrast = 37,
		StochatsicEnabled = 38,
		Saturation = 39,
		TextureClusterContrast = 40,
		TextureClusterBoost = 41,
		HeightOffset = 42,
		HeightContrast = 43,
		AntiTileArrayNormalStrength = 56,
		AntiTileArrayDetailStrength = 57,
		AntiTileArrayDistanceStrength = 58,
		DisplaceShaping = 59,
		UVRotation = 64,
		TriplanarRotationX = 65,
		TriplanarRotationY = 66,
		FuzzyShadingCore = 68,
		FuzzyShadingEdge = 69,
		FuzzyShadingPower = 70,
		SSSThickness = 75
	}

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int sMaxTextures = 32;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int sMaxAttributes = 32;

	public Color[] values = new Color[1024];

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Texture2D tex;

	[HideInInspector]
	public AnimationCurve geoCurve = AnimationCurve.Linear(0f, 0f, 0f, 0f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Texture2D geoTex;

	[HideInInspector]
	public AnimationCurve geoSlopeFilter = AnimationCurve.Linear(0f, 0.2f, 0.4f, 1f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Texture2D geoSlopeTex;

	[HideInInspector]
	public AnimationCurve globalSlopeFilter = AnimationCurve.Linear(0f, 0.2f, 0.4f, 1f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Texture2D globalSlopeTex;

	[PublicizedFrom(EAccessModifier.Private)]
	public void RevisionData()
	{
		if (values.Length == 256)
		{
			Color[] array = new Color[1024];
			for (int i = 0; i < 16; i++)
			{
				for (int j = 0; j < 16; j++)
				{
					array[j * 32 + i] = values[j * 32 + i];
				}
			}
			values = array;
		}
		else
		{
			if (values.Length != 512)
			{
				return;
			}
			Color[] array2 = new Color[1024];
			for (int k = 0; k < 32; k++)
			{
				for (int l = 0; l < 16; l++)
				{
					array2[l * 32 + k] = values[l * 32 + k];
				}
			}
			values = array2;
		}
	}

	public Color GetValue(int x, int y)
	{
		RevisionData();
		return values[y * 32 + x];
	}

	public void SetValue(int x, int y, Color c)
	{
		RevisionData();
		values[y * 32 + x] = c;
	}

	public void SetValue(int x, int y, int channel, float value)
	{
		RevisionData();
		int num = y * 32 + x;
		Color color = values[num];
		color[channel] = value;
		values[num] = color;
	}

	public void SetValue(int x, int y, int channel, Vector2 value)
	{
		RevisionData();
		int num = y * 32 + x;
		Color color = values[num];
		if (channel == 0)
		{
			color.r = value.x;
			color.g = value.y;
		}
		else
		{
			color.b = value.x;
			color.a = value.y;
		}
		values[num] = color;
	}

	public void SetValue(int textureIndex, PerTexFloat channel, float value)
	{
		float num = (float)channel / 4f;
		int num2 = (int)num;
		int channel2 = Mathf.RoundToInt((num - (float)num2) * 4f);
		SetValue(textureIndex, num2, channel2, value);
	}

	public void SetValue(int textureIndex, PerTexColor channel, Color value)
	{
		int y = (int)((float)channel / 4f);
		SetValue(textureIndex, y, value);
	}

	public void SetValue(int textureIndex, PerTexVector2 channel, Vector2 value)
	{
		float num = (float)channel / 4f;
		int num2 = (int)num;
		int channel2 = Mathf.RoundToInt((num - (float)num2) * 4f);
		SetValue(textureIndex, num2, channel2, value);
	}

	public Texture2D GetTexture()
	{
		RevisionData();
		if (tex == null)
		{
			if (SystemInfo.SupportsTextureFormat(TextureFormat.RGBAFloat))
			{
				tex = new Texture2D(32, 32, TextureFormat.RGBAFloat, mipChain: false, linear: true);
			}
			else if (SystemInfo.SupportsTextureFormat(TextureFormat.RGBAHalf))
			{
				tex = new Texture2D(32, 32, TextureFormat.RGBAHalf, mipChain: false, linear: true);
			}
			else
			{
				Debug.LogError("Could not create RGBAFloat or RGBAHalf format textures, per texture properties will be clamped to 0-1 range, which will break things");
				tex = new Texture2D(32, 32, TextureFormat.RGBA32, mipChain: false, linear: true);
			}
			tex.hideFlags = HideFlags.HideAndDontSave;
			tex.wrapMode = TextureWrapMode.Clamp;
			tex.filterMode = FilterMode.Point;
		}
		tex.SetPixels(values);
		tex.Apply();
		return tex;
	}

	public Texture2D GetGeoCurve()
	{
		if (geoTex == null)
		{
			geoTex = new Texture2D(256, 1, TextureFormat.RHalf, mipChain: false, linear: true);
			geoTex.hideFlags = HideFlags.HideAndDontSave;
		}
		for (int i = 0; i < 256; i++)
		{
			float num = geoCurve.Evaluate((float)i / 255f);
			geoTex.SetPixel(i, 0, new Color(num, num, num, num));
		}
		geoTex.Apply();
		return geoTex;
	}

	public Texture2D GetGeoSlopeFilter()
	{
		if (geoSlopeTex == null)
		{
			geoSlopeTex = new Texture2D(256, 1, TextureFormat.Alpha8, mipChain: false, linear: true);
			geoSlopeTex.hideFlags = HideFlags.HideAndDontSave;
		}
		for (int i = 0; i < 256; i++)
		{
			float num = geoSlopeFilter.Evaluate((float)i / 255f);
			geoSlopeTex.SetPixel(i, 0, new Color(num, num, num, num));
		}
		geoSlopeTex.Apply();
		return geoSlopeTex;
	}

	public Texture2D GetGlobalSlopeFilter()
	{
		if (globalSlopeTex == null)
		{
			globalSlopeTex = new Texture2D(256, 1, TextureFormat.Alpha8, mipChain: false, linear: true);
			globalSlopeTex.hideFlags = HideFlags.HideAndDontSave;
		}
		for (int i = 0; i < 256; i++)
		{
			float num = globalSlopeFilter.Evaluate((float)i / 255f);
			globalSlopeTex.SetPixel(i, 0, new Color(num, num, num, num));
		}
		globalSlopeTex.Apply();
		return globalSlopeTex;
	}
}
