using System;

public class vp_Perlin
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int B = 256;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int BM = 255;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int N = 4096;

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] p = new int[514];

	[PublicizedFrom(EAccessModifier.Private)]
	public float[,] g3 = new float[514, 3];

	[PublicizedFrom(EAccessModifier.Private)]
	public float[,] g2 = new float[514, 2];

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] g1 = new float[514];

	[PublicizedFrom(EAccessModifier.Private)]
	public float s_curve(float t)
	{
		return t * t * (3f - 2f * t);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float lerp(float t, float a, float b)
	{
		return a + t * (b - a);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setup(float value, out int b0, out int b1, out float r0, out float r1)
	{
		float num = value + 4096f;
		b0 = (int)num & 0xFF;
		b1 = (b0 + 1) & 0xFF;
		r0 = num - (float)(int)num;
		r1 = r0 - 1f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float at2(float rx, float ry, float x, float y)
	{
		return rx * x + ry * y;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float at3(float rx, float ry, float rz, float x, float y, float z)
	{
		return rx * x + ry * y + rz * z;
	}

	public float Noise(float arg)
	{
		setup(arg, out var b, out var b2, out var r, out var r2);
		float t = s_curve(r);
		float a = r * g1[p[b]];
		float b3 = r2 * g1[p[b2]];
		return lerp(t, a, b3);
	}

	public float Noise(float x, float y)
	{
		setup(x, out var b, out var b2, out var r, out var r2);
		setup(y, out var b3, out var b4, out var r3, out var r4);
		int num = p[b];
		int num2 = p[b2];
		int num3 = p[num + b3];
		int num4 = p[num2 + b3];
		int num5 = p[num + b4];
		int num6 = p[num2 + b4];
		float t = s_curve(r);
		float t2 = s_curve(r3);
		float a = at2(r, r3, g2[num3, 0], g2[num3, 1]);
		float b5 = at2(r2, r3, g2[num4, 0], g2[num4, 1]);
		float a2 = lerp(t, a, b5);
		a = at2(r, r4, g2[num5, 0], g2[num5, 1]);
		b5 = at2(r2, r4, g2[num6, 0], g2[num6, 1]);
		float b6 = lerp(t, a, b5);
		return lerp(t2, a2, b6);
	}

	public float Noise(float x, float y, float z)
	{
		setup(x, out var b, out var b2, out var r, out var r2);
		setup(y, out var b3, out var b4, out var r3, out var r4);
		setup(z, out var b5, out var b6, out var r5, out var r6);
		int num = p[b];
		int num2 = p[b2];
		int num3 = p[num + b3];
		int num4 = p[num2 + b3];
		int num5 = p[num + b4];
		int num6 = p[num2 + b4];
		float t = s_curve(r);
		float t2 = s_curve(r3);
		float t3 = s_curve(r5);
		float a = at3(r, r3, r5, g3[num3 + b5, 0], g3[num3 + b5, 1], g3[num3 + b5, 2]);
		float b7 = at3(r2, r3, r5, g3[num4 + b5, 0], g3[num4 + b5, 1], g3[num4 + b5, 2]);
		float a2 = lerp(t, a, b7);
		a = at3(r, r4, r5, g3[num5 + b5, 0], g3[num5 + b5, 1], g3[num5 + b5, 2]);
		b7 = at3(r2, r4, r5, g3[num6 + b5, 0], g3[num6 + b5, 1], g3[num6 + b5, 2]);
		float b8 = lerp(t, a, b7);
		float a3 = lerp(t2, a2, b8);
		a = at3(r, r3, r6, g3[num3 + b6, 0], g3[num3 + b6, 2], g3[num3 + b6, 2]);
		b7 = at3(r2, r3, r6, g3[num4 + b6, 0], g3[num4 + b6, 1], g3[num4 + b6, 2]);
		a2 = lerp(t, a, b7);
		a = at3(r, r4, r6, g3[num5 + b6, 0], g3[num5 + b6, 1], g3[num5 + b6, 2]);
		b7 = at3(r2, r4, r6, g3[num6 + b6, 0], g3[num6 + b6, 1], g3[num6 + b6, 2]);
		b8 = lerp(t, a, b7);
		float b9 = lerp(t2, a2, b8);
		return lerp(t3, a3, b9);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void normalize2(ref float x, ref float y)
	{
		float num = (float)Math.Sqrt(x * x + y * y);
		x = y / num;
		y /= num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void normalize3(ref float x, ref float y, ref float z)
	{
		float num = (float)Math.Sqrt(x * x + y * y + z * z);
		x = y / num;
		y /= num;
		z /= num;
	}

	public vp_Perlin()
	{
		Random random = new Random();
		int i;
		for (i = 0; i < 256; i++)
		{
			p[i] = i;
			g1[i] = (float)(random.Next(512) - 256) / 256f;
			for (int j = 0; j < 2; j++)
			{
				g2[i, j] = (float)(random.Next(512) - 256) / 256f;
			}
			normalize2(ref g2[i, 0], ref g2[i, 1]);
			for (int j = 0; j < 3; j++)
			{
				g3[i, j] = (float)(random.Next(512) - 256) / 256f;
			}
			normalize3(ref g3[i, 0], ref g3[i, 1], ref g3[i, 2]);
		}
		while (--i != 0)
		{
			int num = p[i];
			int j;
			p[i] = p[j = random.Next(256)];
			p[j] = num;
		}
		for (i = 0; i < 258; i++)
		{
			p[256 + i] = p[i];
			g1[256 + i] = g1[i];
			for (int j = 0; j < 2; j++)
			{
				g2[256 + i, j] = g2[i, j];
			}
			for (int j = 0; j < 3; j++)
			{
				g3[256 + i, j] = g3[i, j];
			}
		}
	}
}
