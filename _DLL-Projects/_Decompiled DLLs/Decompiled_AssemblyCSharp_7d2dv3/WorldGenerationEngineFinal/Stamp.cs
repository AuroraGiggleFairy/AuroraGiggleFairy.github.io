using System;
using System.Collections.Generic;
using UnityEngine;

namespace WorldGenerationEngineFinal;

public class Stamp
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly WorldBuilder worldBuilder;

	public TranslationData transform;

	public float alpha;

	public bool additive;

	public float scale;

	public Color32 customColor;

	public float alphaCutoff;

	public Rect Area;

	public bool isWater;

	public string Name = "";

	public RawStamp stamp;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cOneByoneScale = 1.4f;

	public int imageHeight => stamp.height;

	public int imageWidth => stamp.width;

	public Stamp(WorldBuilder _worldBuilder, RawStamp _stamp, TranslationData _transData, Color32 _customColor = default(Color32), float _alphaCutoff = 0.1f, bool _isWater = false, string stampName = "")
	{
		worldBuilder = _worldBuilder;
		stamp = _stamp;
		transform = _transData;
		scale = transform.scale;
		customColor = _customColor;
		alphaCutoff = _alphaCutoff;
		isWater = _isWater;
		Name = stampName;
		alpha = 1f;
		additive = false;
		int rotation = transform.rotation;
		int num = (int)((float)_stamp.width * scale * 1.4f);
		int num2 = (int)((float)_stamp.height * scale * 1.4f);
		int x = transform.x - num / 2;
		int x2 = transform.x + num / 2;
		int y = transform.y - num2 / 2;
		int y2 = transform.y + num2 / 2;
		int x3 = transform.x;
		int y3 = transform.y;
		Vector2i rotatedPoint = getRotatedPoint(x, y, x3, y3, rotation);
		Vector2i rotatedPoint2 = getRotatedPoint(x2, y, x3, y3, rotation);
		Vector2i rotatedPoint3 = getRotatedPoint(x, y2, x3, y3, rotation);
		Vector2i rotatedPoint4 = getRotatedPoint(x2, y2, x3, y3, rotation);
		Vector2 vector = new Vector2(Mathf.Min(Mathf.Min(rotatedPoint.x, rotatedPoint2.x), Mathf.Min(rotatedPoint3.x, rotatedPoint4.x)), Mathf.Min(Mathf.Min(rotatedPoint.y, rotatedPoint2.y), Mathf.Min(rotatedPoint3.y, rotatedPoint4.y)));
		Vector2 vector2 = new Vector2(Mathf.Max(Mathf.Max(rotatedPoint.x, rotatedPoint2.x), Mathf.Max(rotatedPoint3.x, rotatedPoint4.x)), Mathf.Max(Mathf.Max(rotatedPoint.y, rotatedPoint2.y), Mathf.Max(rotatedPoint3.y, rotatedPoint4.y)));
		Area = new Rect(vector, vector2 - vector);
		if (isWater)
		{
			if (worldBuilder.waterRects == null)
			{
				worldBuilder.waterRects = new List<Rect>();
			}
			worldBuilder.waterRects.Add(Area);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Color[] rotateColorArray(Color[] src, float angle, int width, int height)
	{
		Color[] array = new Color[width * height];
		double num = Math.Sin(Math.PI / 180.0 * (double)angle);
		double num2 = Math.Cos(Math.PI / 180.0 * (double)angle);
		int num3 = width / 2;
		int num4 = height / 2;
		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < width; j++)
			{
				float num5 = (float)(num2 * (double)(j - num3) + num * (double)(i - num4) + (double)num3);
				float num6 = (float)((0.0 - num) * (double)(j - num3) + num2 * (double)(i - num4) + (double)num4);
				int num7 = (int)num5;
				int num8 = (int)num6;
				num5 -= (float)num7;
				num6 -= (float)num8;
				if (num7 >= 0 && num7 < width && num8 >= 0 && num8 < height)
				{
					Color color = src[num8 * width + num7];
					Color rightVal = color;
					Color upVal = color;
					Color upRightVal = color;
					if (num7 + 1 < width)
					{
						rightVal = src[num8 * width + num7 + 1];
					}
					if (num8 + 1 < height)
					{
						upVal = src[(num8 + 1) * width + num7];
					}
					if (num7 + 1 < width && num8 + 1 < height)
					{
						upRightVal = src[(num8 + 1) * width + num7 + 1];
					}
					array[i * width + j] = QuadLerpColor(color, rightVal, upRightVal, upVal, num5, num6);
				}
			}
		}
		return array;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Color QuadLerpColor(Color selfVal, Color rightVal, Color upRightVal, Color upVal, float horizontalPerc, float verticalPerc)
	{
		return Color.Lerp(Color.Lerp(selfVal, rightVal, horizontalPerc), Color.Lerp(upVal, upRightVal, horizontalPerc), verticalPerc);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i getRotatedPoint(int x, int y, int cx, int cy, int angle)
	{
		double num = Math.Cos(angle);
		double num2 = Math.Sin(angle);
		return new Vector2i(Mathf.RoundToInt((float)((double)(x - cx) * num - (double)(y - cy) * num2 + (double)cx)), Mathf.RoundToInt((float)((double)(x - cx) * num2 + (double)(y - cy) * num + (double)cy)));
	}
}
