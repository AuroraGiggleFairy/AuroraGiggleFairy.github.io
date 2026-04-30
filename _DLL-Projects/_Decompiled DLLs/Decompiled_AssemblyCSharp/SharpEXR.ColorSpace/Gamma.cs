using System;

namespace SharpEXR.ColorSpace;

public static class Gamma
{
	public static float Expand(float nonlinear)
	{
		return (float)Math.Pow(nonlinear, 2.2);
	}

	public static float Compress(float linear)
	{
		return (float)Math.Pow(linear, 0.45454545454545453);
	}

	public static void Expand(ref tVec3 pColor)
	{
		pColor.X = Expand(pColor.X);
		pColor.Y = Expand(pColor.Y);
		pColor.Z = Expand(pColor.Z);
	}

	public static void Compress(ref tVec3 pColor)
	{
		pColor.X = Compress(pColor.X);
		pColor.Y = Compress(pColor.Y);
		pColor.Z = Compress(pColor.Z);
	}

	public static void Expand(ref float r, ref float g, ref float b)
	{
		r = Expand(r);
		g = Expand(g);
		b = Expand(b);
	}

	public static void Compress(ref float r, ref float g, ref float b)
	{
		r = Compress(r);
		g = Compress(g);
		b = Compress(b);
	}

	public static tVec3 Expand(float r, float g, float b)
	{
		tVec3 pColor = new tVec3(r, g, b);
		Expand(ref pColor);
		return pColor;
	}

	public static tVec3 Compress(float r, float g, float b)
	{
		tVec3 pColor = new tVec3(r, g, b);
		Compress(ref pColor);
		return pColor;
	}

	public static float Expand_sRGB(float nonlinear)
	{
		if (!(nonlinear <= 0.04045f))
		{
			return (float)Math.Pow((nonlinear + 0.055f) / 1.055f, 2.4000000953674316);
		}
		return nonlinear / 12.92f;
	}

	public static float Compress_sRGB(float linear)
	{
		if (!(linear <= 0.0031308f))
		{
			return 1.055f * (float)Math.Pow(linear, 0.4166666567325592) - 0.055f;
		}
		return 12.92f * linear;
	}

	public static void Expand_sRGB(ref tVec3 pColor)
	{
		pColor.X = Expand_sRGB(pColor.X);
		pColor.Y = Expand_sRGB(pColor.Y);
		pColor.Z = Expand_sRGB(pColor.Z);
	}

	public static void Compress_sRGB(ref tVec3 pColor)
	{
		pColor.X = Compress_sRGB(pColor.X);
		pColor.Y = Compress_sRGB(pColor.Y);
		pColor.Z = Compress_sRGB(pColor.Z);
	}

	public static void Expand_sRGB(ref float r, ref float g, ref float b)
	{
		r = Expand_sRGB(r);
		g = Expand_sRGB(g);
		b = Expand_sRGB(b);
	}

	public static void Compress_sRGB(ref float r, ref float g, ref float b)
	{
		r = Compress_sRGB(r);
		g = Compress_sRGB(g);
		b = Compress_sRGB(b);
	}

	public static tVec3 Expand_sRGB(float r, float g, float b)
	{
		tVec3 pColor = new tVec3(r, g, b);
		Expand_sRGB(ref pColor);
		return pColor;
	}

	public static tVec3 Compress_sRGB(float r, float g, float b)
	{
		tVec3 pColor = new tVec3(r, g, b);
		Compress_sRGB(ref pColor);
		return pColor;
	}
}
