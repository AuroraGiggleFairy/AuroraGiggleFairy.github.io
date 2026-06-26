using UnityEngine;

namespace JBooth.MicroSplat;

public class CurvatureMapGenerator
{
	public static float m_limit = 10000f;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int LEFT = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int RIGHT = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int BOTTOM = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int TOP = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float TIME = 0.2f;

	public static int m_iterations = 8;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float Horizontal(float dx, float dy, float dxx, float dyy, float dxy)
	{
		float num = -2f * (dy * dy * dxx + dx * dx * dyy - dx * dy * dxy);
		num /= dx * dx + dy * dy;
		if (float.IsInfinity(num) || float.IsNaN(num))
		{
			num = 0f;
		}
		if (num < 0f - m_limit)
		{
			num = 0f - m_limit;
		}
		if (num > m_limit)
		{
			num = m_limit;
		}
		num /= m_limit;
		return num * 0.5f + 0.5f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float Vertical(float dx, float dy, float dxx, float dyy, float dxy)
	{
		float num = -2f * (dx * dx * dxx + dy * dy * dyy + dx * dy * dxy);
		num /= dx * dx + dy * dy;
		if (float.IsInfinity(num) || float.IsNaN(num))
		{
			num = 0f;
		}
		if (num < 0f - m_limit)
		{
			num = 0f - m_limit;
		}
		if (num > m_limit)
		{
			num = m_limit;
		}
		num /= m_limit;
		return num * 0.5f + 0.5f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float Average(float dx, float dy, float dxx, float dyy, float dxy)
	{
		float num = Horizontal(dx, dy, dxx, dyy, dxy);
		float num2 = Vertical(dx, dy, dxx, dyy, dxy);
		return (num + num2) * 0.5f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void UpdateWaterMap(float[,] waterMap, float[,,] outFlow, int width, int height)
	{
		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < width; j++)
			{
				float num = outFlow[j, i, 0] + outFlow[j, i, 1] + outFlow[j, i, 2] + outFlow[j, i, 3];
				float num2 = 0f;
				num2 += ((j == 0) ? 0f : outFlow[j - 1, i, RIGHT]);
				num2 += ((j == width - 1) ? 0f : outFlow[j + 1, i, LEFT]);
				num2 += ((i == 0) ? 0f : outFlow[j, i - 1, TOP]);
				num2 += ((i == height - 1) ? 0f : outFlow[j, i + 1, BOTTOM]);
				float num3 = waterMap[j, i] + (num2 - num) * TIME;
				if (num3 < 0f)
				{
					num3 = 0f;
				}
				waterMap[j, i] = num3;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void CalculateVelocityField(float[,] velocityMap, float[,,] outFlow, int width, int height)
	{
		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < width; j++)
			{
				float num = ((j == 0) ? 0f : (outFlow[j - 1, i, RIGHT] - outFlow[j, i, LEFT]));
				float num2 = ((j == width - 1) ? 0f : (outFlow[j, i, RIGHT] - outFlow[j + 1, i, LEFT]));
				float num3 = ((i == height - 1) ? 0f : (outFlow[j, i + 1, BOTTOM] - outFlow[j, i, TOP]));
				float num4 = ((i == 0) ? 0f : (outFlow[j, i, BOTTOM] - outFlow[j, i - 1, TOP]));
				float num5 = (num + num2) * 0.5f;
				float num6 = (num4 + num3) * 0.5f;
				velocityMap[j, i] = Mathf.Sqrt(num5 * num5 + num6 * num6);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void NormalizeMap(float[,] map, int width, int height)
	{
		float num = float.PositiveInfinity;
		float num2 = float.NegativeInfinity;
		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < width; j++)
			{
				float num3 = map[j, i];
				if (num3 < num)
				{
					num = num3;
				}
				if (num3 > num2)
				{
					num2 = num3;
				}
			}
		}
		float num4 = num2 - num;
		for (int k = 0; k < height; k++)
		{
			for (int l = 0; l < width; l++)
			{
				float num5 = map[l, k];
				num5 = ((!(num4 < 1E-12f)) ? ((num5 - num) / num4) : 0f);
				map[l, k] = num5;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void FillWaterMap(float amount, float[,] waterMap, int width, int height)
	{
		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < width; j++)
			{
				waterMap[j, i] = amount;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ComputeOutflow(float[,] waterMap, float[,,] outFlow, float[,] heightMap, int width, int height)
	{
		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < width; j++)
			{
				int num = ((j != 0) ? (j - 1) : 0);
				int num2 = ((j == width - 1) ? (width - 1) : (j + 1));
				int num3 = ((i != 0) ? (i - 1) : 0);
				int num4 = ((i == height - 1) ? (height - 1) : (i + 1));
				float num5 = waterMap[j, i];
				float num6 = waterMap[num, i];
				float num7 = waterMap[num2, i];
				float num8 = waterMap[j, num3];
				float num9 = waterMap[j, num4];
				float num10 = heightMap[j, i];
				float num11 = heightMap[num, i];
				float num12 = heightMap[num2, i];
				float num13 = heightMap[j, num3];
				float num14 = heightMap[j, num4];
				float num15 = num5 + num10 - (num6 + num11);
				float num16 = num5 + num10 - (num7 + num12);
				float num17 = num5 + num10 - (num8 + num13);
				float num18 = num5 + num10 - (num9 + num14);
				float num19 = Mathf.Max(0f, outFlow[j, i, 0] + num15);
				float num20 = Mathf.Max(0f, outFlow[j, i, 1] + num16);
				float num21 = Mathf.Max(0f, outFlow[j, i, 2] + num17);
				float num22 = Mathf.Max(0f, outFlow[j, i, 3] + num18);
				float num23 = num19 + num20 + num21 + num22;
				if (num23 > 0f)
				{
					float num24 = num5 / (num23 * TIME);
					if (num24 > 1f)
					{
						num24 = 1f;
					}
					if (num24 < 0f)
					{
						num24 = 0f;
					}
					outFlow[j, i, 0] = num19 * num24;
					outFlow[j, i, 1] = num20 * num24;
					outFlow[j, i, 2] = num21 * num24;
					outFlow[j, i, 3] = num22 * num24;
				}
				else
				{
					outFlow[j, i, 0] = 0f;
					outFlow[j, i, 1] = 0f;
					outFlow[j, i, 2] = 0f;
					outFlow[j, i, 3] = 0f;
				}
			}
		}
	}

	public static void CreateMap(float[,] heights, Texture2D curveMap)
	{
		int width = curveMap.width;
		int height = curveMap.height;
		float num = 1f / ((float)width - 1f);
		float num2 = 1f / ((float)height - 1f);
		float[,] waterMap = new float[width, height];
		float[,,] outFlow = new float[width, height, 4];
		FillWaterMap(0.0001f, waterMap, width, height);
		for (int i = 0; i < m_iterations; i++)
		{
			ComputeOutflow(waterMap, outFlow, heights, width, height);
			UpdateWaterMap(waterMap, outFlow, width, height);
		}
		float[,] array = new float[width, height];
		CalculateVelocityField(array, outFlow, width, height);
		NormalizeMap(array, width, height);
		for (int j = 0; j < height; j++)
		{
			for (int num3 = width - 1; num3 >= 0; num3--)
			{
				int num4 = ((num3 == width - 1) ? num3 : (num3 + 1));
				int num5 = ((num3 == 0) ? num3 : (num3 - 1));
				int num6 = ((j == height - 1) ? j : (j + 1));
				int num7 = ((j == 0) ? j : (j - 1));
				float num8 = heights[num3, j];
				float num9 = heights[num5, j];
				float num10 = heights[num4, j];
				float num11 = heights[num3, num7];
				float num12 = heights[num3, num6];
				float num13 = heights[num5, num7];
				float num14 = heights[num5, num6];
				float num15 = heights[num4, num7];
				float num16 = heights[num4, num6];
				float dx = (num10 - num9) / (2f * num);
				float dy = (num12 - num11) / (2f * num2);
				float dxx = (num10 - 2f * num8 + num9) / (num * num);
				float dyy = (num12 - 2f * num8 + num11) / (num2 * num2);
				float dxy = (num16 - num15 - num14 + num13) / (4f * num * num2);
				float num17 = 0f;
				num17 = Average(dx, dy, dxx, dyy, dxy);
				float a = array[num3, j];
				curveMap.SetPixel(j, num3, new Color(0f, num17, 0f, a));
			}
		}
		curveMap.Apply();
	}
}
