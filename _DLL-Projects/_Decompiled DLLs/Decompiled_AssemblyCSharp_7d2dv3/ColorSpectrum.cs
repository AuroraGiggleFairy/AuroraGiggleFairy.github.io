using Unity.Collections;
using UnityEngine;

public class ColorSpectrum
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Color[] values;

	public static ColorSpectrum FromTexture(string _filename)
	{
		Texture2D texture2D = DataLoader.LoadAsset<Texture2D>(_filename);
		if (texture2D == null)
		{
			return null;
		}
		ColorSpectrum result = new ColorSpectrum(_filename, texture2D);
		DataLoader.UnloadAsset(_filename, texture2D);
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsSupportedRawTextureFormat(TextureFormat _format)
	{
		if ((uint)(_format - 3) <= 2u || _format == TextureFormat.BGRA32)
		{
			return true;
		}
		return false;
	}

	public ColorSpectrum(string name, Texture2D _tex)
	{
		int width = _tex.width;
		values = new Color[width];
		if (IsSupportedRawTextureFormat(_tex.format))
		{
			switch (_tex.format)
			{
			case TextureFormat.ARGB32:
			{
				NativeArray<TextureUtils.ColorARGB32> rawTextureData4 = _tex.GetRawTextureData<TextureUtils.ColorARGB32>();
				for (int l = 0; l < width; l++)
				{
					values[l] = TextureUtils.GetLinearColor(rawTextureData4[l]);
				}
				break;
			}
			case TextureFormat.RGBA32:
			{
				NativeArray<Color32> rawTextureData2 = _tex.GetRawTextureData<Color32>();
				for (int j = 0; j < width; j++)
				{
					values[j] = TextureUtils.GetLinearColor(rawTextureData2[j]);
				}
				break;
			}
			case TextureFormat.BGRA32:
			{
				NativeArray<TextureUtils.ColorBGRA32> rawTextureData3 = _tex.GetRawTextureData<TextureUtils.ColorBGRA32>();
				for (int k = 0; k < width; k++)
				{
					values[k] = TextureUtils.GetLinearColor(rawTextureData3[k]);
				}
				break;
			}
			case TextureFormat.RGB24:
			{
				NativeArray<TextureUtils.ColorRGB24> rawTextureData = _tex.GetRawTextureData<TextureUtils.ColorRGB24>();
				for (int i = 0; i < width; i++)
				{
					values[i] = TextureUtils.GetLinearColor(rawTextureData[i]);
				}
				break;
			}
			}
		}
		else
		{
			Log.Warning("Color Spectrum texture " + name + " is not in a format supported for non-allocating GetRawTextureData access. Falling back to GetPixels.");
			Color[] pixels = _tex.GetPixels();
			for (int m = 0; m < width; m++)
			{
				values[m] = pixels[m].linear;
			}
		}
	}

	public Color GetValue(float _v)
	{
		int num = values.Length;
		float num2 = (float)num * _v;
		int num3 = (int)num2;
		Color a = values[num3];
		Color b = values[(num3 + 1) % num];
		return Color.LerpUnclamped(a, b, num2 - (float)num3);
	}

	public static bool Exists(string _filename)
	{
		return Resources.Load(_filename) != null;
	}
}
