public class TeraMath
{
	public static int fastAbs(int i)
	{
		if (i < 0)
		{
			return -i;
		}
		return i;
	}

	public static float fastAbs(float d)
	{
		if (!(d >= 0f))
		{
			return 0f - d;
		}
		return d;
	}

	public static double fastAbs(double d)
	{
		if (!(d >= 0.0))
		{
			return 0.0 - d;
		}
		return d;
	}

	public static double fastFloor(double d)
	{
		int num = (int)d;
		return (d < 0.0 && d != (double)num) ? (num - 1) : num;
	}

	public static float fastFloor(float d)
	{
		int num = (int)d;
		return (d < 0f && d != (float)num) ? (num - 1) : num;
	}

	public static double clamp(double value)
	{
		if (value > 1.0)
		{
			return 1.0;
		}
		if (value < 0.0)
		{
			return 0.0;
		}
		return value;
	}

	public static float clamp(float value)
	{
		if (value > 1f)
		{
			return 1f;
		}
		if (value < 0f)
		{
			return 0f;
		}
		return value;
	}

	public static double clamp(double value, double min, double max)
	{
		if (value > max)
		{
			return max;
		}
		if (value < min)
		{
			return min;
		}
		return value;
	}

	public static float clamp(float value, float min, float max)
	{
		if (value > max)
		{
			return max;
		}
		if (value < min)
		{
			return min;
		}
		return value;
	}

	public static int clamp(int value, int min, int max)
	{
		if (value > max)
		{
			return max;
		}
		if (value < min)
		{
			return min;
		}
		return value;
	}

	public static double biLerp(double x, double y, double q11, double q12, double q21, double q22, double x1, double x2, double y1, double y2)
	{
		double q23 = lerp(x, x1, x2, q11, q21);
		double q24 = lerp(x, x1, x2, q12, q22);
		return lerp(y, y1, y2, q23, q24);
	}

	public static double lerp(double x, double x1, double x2, double q00, double q01)
	{
		return (x2 - x) / (x2 - x1) * q00 + (x - x1) / (x2 - x1) * q01;
	}

	public static float lerp(float x, float x1, float x2, float q00, float q01)
	{
		return (x2 - x) / (x2 - x1) * q00 + (x - x1) / (x2 - x1) * q01;
	}

	public static double lerp(double x1, double x2, double p)
	{
		return x1 * (1.0 - p) + x2 * p;
	}

	public static float lerpf(float x1, float x2, float p)
	{
		return x1 * (1f - p) + x2 * p;
	}

	public static int floorToInt(float val)
	{
		int num = (int)val;
		if (!(val < 0f) || val == (float)num)
		{
			return num;
		}
		return num - 1;
	}

	public static int ceilToInt(float val)
	{
		int num = (int)val;
		if (!(val >= 0f) || val == (float)num)
		{
			return num;
		}
		return num + 1;
	}
}
