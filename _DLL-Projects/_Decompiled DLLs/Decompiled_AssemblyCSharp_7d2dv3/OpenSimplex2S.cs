using System.Runtime.CompilerServices;

public static class OpenSimplex2S
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class LatticeVertex4D
	{
		public readonly float dx;

		public readonly float dy;

		public readonly float dz;

		public readonly float dw;

		public readonly long xsvp;

		public readonly long ysvp;

		public readonly long zsvp;

		public readonly long wsvp;

		public LatticeVertex4D(int xsv, int ysv, int zsv, int wsv)
		{
			xsvp = xsv * 5910200641878280303L;
			ysvp = ysv * 6452764530575939509L;
			zsvp = zsv * 6614699811220273867L;
			wsvp = wsv * 6254464313819354443L;
			float num = (float)(xsv + ysv + zsv + wsv) * -0.1381966f;
			dx = (float)(-xsv) - num;
			dy = (float)(-ysv) - num;
			dz = (float)(-zsv) - num;
			dw = (float)(-wsv) - num;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const long PRIME_X = 5910200641878280303L;

	[PublicizedFrom(EAccessModifier.Private)]
	public const long PRIME_Y = 6452764530575939509L;

	[PublicizedFrom(EAccessModifier.Private)]
	public const long PRIME_Z = 6614699811220273867L;

	[PublicizedFrom(EAccessModifier.Private)]
	public const long PRIME_W = 6254464313819354443L;

	[PublicizedFrom(EAccessModifier.Private)]
	public const long HASH_MULTIPLIER = 6026932503003350773L;

	[PublicizedFrom(EAccessModifier.Private)]
	public const long SEED_FLIP_3D = -5968755714895566377L;

	[PublicizedFrom(EAccessModifier.Private)]
	public const double ROOT2OVER2 = 0.7071067811865476;

	[PublicizedFrom(EAccessModifier.Private)]
	public const double SKEW_2D = 0.366025403784439;

	[PublicizedFrom(EAccessModifier.Private)]
	public const double UNSKEW_2D = -0.21132486540518713;

	[PublicizedFrom(EAccessModifier.Private)]
	public const double ROOT3OVER3 = 0.577350269189626;

	[PublicizedFrom(EAccessModifier.Private)]
	public const double FALLBACK_ROTATE3 = 2.0 / 3.0;

	[PublicizedFrom(EAccessModifier.Private)]
	public const double ROTATE3_ORTHOGONALIZER = -0.21132486540518713;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float SKEW_4D = 0.309017f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float UNSKEW_4D = -0.1381966f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int N_GRADS_2D_EXPONENT = 7;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int N_GRADS_3D_EXPONENT = 8;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int N_GRADS_4D_EXPONENT = 9;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int N_GRADS_2D = 128;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int N_GRADS_3D = 256;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int N_GRADS_4D = 512;

	[PublicizedFrom(EAccessModifier.Private)]
	public const double NORMALIZER_2D = 0.05481866495625118;

	[PublicizedFrom(EAccessModifier.Private)]
	public const double NORMALIZER_3D = 0.2781926117527186;

	[PublicizedFrom(EAccessModifier.Private)]
	public const double NORMALIZER_4D = 0.11127401889945551;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float RSQUARED_2D = 2f / 3f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float RSQUARED_3D = 0.75f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float RSQUARED_4D = 0.8f;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly float[] GRADIENTS_2D;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly float[] GRADIENTS_3D;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly float[] GRADIENTS_4D;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly (short SecondaryIndexStart, short SecondaryIndexStop)[] LOOKUP_4D_A;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly LatticeVertex4D[] LOOKUP_4D_B;

	public static float Noise2(long seed, double x, double y)
	{
		double num = 0.366025403784439 * (x + y);
		double xs = x + num;
		double ys = y + num;
		return Noise2_UnskewedBase(seed, xs, ys);
	}

	public static float Noise2_ImproveX(long seed, double x, double y)
	{
		double num = x * 0.7071067811865476;
		double num2 = y * 1.2247448713915896;
		return Noise2_UnskewedBase(seed, num2 + num, num2 - num);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float Noise2_UnskewedBase(long seed, double xs, double ys)
	{
		int num = FastFloor(xs);
		int num2 = FastFloor(ys);
		float num3 = (float)(xs - (double)num);
		float num4 = (float)(ys - (double)num2);
		long num5 = num * 5910200641878280303L;
		long num6 = num2 * 6452764530575939509L;
		float num7 = (num3 + num4) * -0.21132487f;
		float num8 = num3 + num7;
		float num9 = num4 + num7;
		float num10 = 2f / 3f - num8 * num8 - num9 * num9;
		float num11 = num10 * num10 * (num10 * num10) * Grad(seed, num5, num6, num8, num9);
		float num12 = -3.1547005f * num7 + (-2f / 3f + num10);
		float dx = num8 - 0.57735026f;
		float dy = num9 - 0.57735026f;
		num11 += num12 * num12 * (num12 * num12) * Grad(seed, num5 + 5910200641878280303L, num6 + 6452764530575939509L, dx, dy);
		float num13 = num3 - num4;
		if ((double)num7 < -0.21132486540518713)
		{
			if (num3 + num13 > 1f)
			{
				float num14 = num8 - 1.3660254f;
				float num15 = num9 - 0.36602542f;
				float num16 = 2f / 3f - num14 * num14 - num15 * num15;
				if (num16 > 0f)
				{
					num11 += num16 * num16 * (num16 * num16) * Grad(seed, num5 + -6626342789952991010L, num6 + 6452764530575939509L, num14, num15);
				}
			}
			else
			{
				float num17 = num8 - -0.21132487f;
				float num18 = num9 - 0.7886751f;
				float num19 = 2f / 3f - num17 * num17 - num18 * num18;
				if (num19 > 0f)
				{
					num11 += num19 * num19 * (num19 * num19) * Grad(seed, num5, num6 + 6452764530575939509L, num17, num18);
				}
			}
			if (num4 - num13 > 1f)
			{
				float num20 = num8 - 0.36602542f;
				float num21 = num9 - 1.3660254f;
				float num22 = 2f / 3f - num20 * num20 - num21 * num21;
				if (num22 > 0f)
				{
					num11 += num22 * num22 * (num22 * num22) * Grad(seed, num5 + 5910200641878280303L, num6 + -5541215012557672598L, num20, num21);
				}
			}
			else
			{
				float num23 = num8 - 0.7886751f;
				float num24 = num9 - -0.21132487f;
				float num25 = 2f / 3f - num23 * num23 - num24 * num24;
				if (num25 > 0f)
				{
					num11 += num25 * num25 * (num25 * num25) * Grad(seed, num5 + 5910200641878280303L, num6, num23, num24);
				}
			}
		}
		else
		{
			if (num3 + num13 < 0f)
			{
				float num26 = num8 + 0.7886751f;
				float num27 = num9 + -0.21132487f;
				float num28 = 2f / 3f - num26 * num26 - num27 * num27;
				if (num28 > 0f)
				{
					num11 += num28 * num28 * (num28 * num28) * Grad(seed, num5 - 5910200641878280303L, num6, num26, num27);
				}
			}
			else
			{
				float num29 = num8 - 0.7886751f;
				float num30 = num9 - -0.21132487f;
				float num31 = 2f / 3f - num29 * num29 - num30 * num30;
				if (num31 > 0f)
				{
					num11 += num31 * num31 * (num31 * num31) * Grad(seed, num5 + 5910200641878280303L, num6, num29, num30);
				}
			}
			if (num4 < num13)
			{
				float num32 = num8 + -0.21132487f;
				float num33 = num9 + 0.7886751f;
				float num34 = 2f / 3f - num32 * num32 - num33 * num33;
				if (num34 > 0f)
				{
					num11 += num34 * num34 * (num34 * num34) * Grad(seed, num5, num6 - 6452764530575939509L, num32, num33);
				}
			}
			else
			{
				float num35 = num8 - -0.21132487f;
				float num36 = num9 - 0.7886751f;
				float num37 = 2f / 3f - num35 * num35 - num36 * num36;
				if (num37 > 0f)
				{
					num11 += num37 * num37 * (num37 * num37) * Grad(seed, num5, num6 + 6452764530575939509L, num35, num36);
				}
			}
		}
		return num11;
	}

	public static float Noise3_ImproveXY(long seed, double x, double y, double z)
	{
		double num = x + y;
		double num2 = num * -0.21132486540518713;
		double num3 = z * 0.577350269189626;
		double xr = x + num2 + num3;
		double yr = y + num2 + num3;
		double zr = num * -0.577350269189626 + num3;
		return Noise3_UnrotatedBase(seed, xr, yr, zr);
	}

	public static float Noise3_ImproveXZ(long seed, double x, double y, double z)
	{
		double num = x + z;
		double num2 = num * -0.211324865405187;
		double num3 = y * 0.577350269189626;
		double xr = x + num2 + num3;
		double zr = z + num2 + num3;
		double yr = num * -0.577350269189626 + num3;
		return Noise3_UnrotatedBase(seed, xr, yr, zr);
	}

	public static float Noise3_Fallback(long seed, double x, double y, double z)
	{
		double num = 2.0 / 3.0 * (x + y + z);
		double xr = num - x;
		double yr = num - y;
		double zr = num - z;
		return Noise3_UnrotatedBase(seed, xr, yr, zr);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float Noise3_UnrotatedBase(long seed, double xr, double yr, double zr)
	{
		int num = FastFloor(xr);
		int num2 = FastFloor(yr);
		int num3 = FastFloor(zr);
		float num4 = (float)(xr - (double)num);
		float num5 = (float)(yr - (double)num2);
		float num6 = (float)(zr - (double)num3);
		long num7 = num * 5910200641878280303L;
		long num8 = num2 * 6452764530575939509L;
		long num9 = num3 * 6614699811220273867L;
		long seed2 = seed ^ -5968755714895566377L;
		int num10 = (int)(-0.5f - num4);
		int num11 = (int)(-0.5f - num5);
		int num12 = (int)(-0.5f - num6);
		float num13 = num4 + (float)num10;
		float num14 = num5 + (float)num11;
		float num15 = num6 + (float)num12;
		float num16 = 0.75f - num13 * num13 - num14 * num14 - num15 * num15;
		float num17 = num16 * num16 * (num16 * num16) * Grad(seed, num7 + (num10 & 0x5205402B9270C86FL), num8 + (num11 & 0x598CD327003817B5L), num9 + (num12 & 0x5BCC226E9FA0BACBL), num13, num14, num15);
		float num18 = num4 - 0.5f;
		float num19 = num5 - 0.5f;
		float num20 = num6 - 0.5f;
		float num21 = 0.75f - num18 * num18 - num19 * num19 - num20 * num20;
		num17 += num21 * num21 * (num21 * num21) * Grad(seed2, num7 + 5910200641878280303L, num8 + 6452764530575939509L, num9 + 6614699811220273867L, num18, num19, num20);
		float num22 = (float)((num10 | 1) << 1) * num18;
		float num23 = (float)((num11 | 1) << 1) * num19;
		float num24 = (float)((num12 | 1) << 1) * num20;
		float num25 = (float)(-2 - (num10 << 2)) * num18 - 1f;
		float num26 = (float)(-2 - (num11 << 2)) * num19 - 1f;
		float num27 = (float)(-2 - (num12 << 2)) * num20 - 1f;
		bool flag = false;
		float num28 = num22 + num16;
		if (num28 > 0f)
		{
			float dx = num13 - (float)(num10 | 1);
			float dy = num14;
			float dz = num15;
			num17 += num28 * num28 * (num28 * num28) * Grad(seed, num7 + (~num10 & 0x5205402B9270C86FL), num8 + (num11 & 0x598CD327003817B5L), num9 + (num12 & 0x5BCC226E9FA0BACBL), dx, dy, dz);
		}
		else
		{
			float num29 = num23 + num24 + num16;
			if (num29 > 0f)
			{
				float dx2 = num13;
				float dy2 = num14 - (float)(num11 | 1);
				float dz2 = num15 - (float)(num12 | 1);
				num17 += num29 * num29 * (num29 * num29) * Grad(seed, num7 + (num10 & 0x5205402B9270C86FL), num8 + (~num11 & 0x598CD327003817B5L), num9 + (~num12 & 0x5BCC226E9FA0BACBL), dx2, dy2, dz2);
			}
			float num30 = num25 + num21;
			if (num30 > 0f)
			{
				float dx3 = (float)(num10 | 1) + num18;
				float dy3 = num19;
				float dz3 = num20;
				num17 += num30 * num30 * (num30 * num30) * Grad(seed2, num7 + (num10 & -6626342789952991010L), num8 + 6452764530575939509L, num9 + 6614699811220273867L, dx3, dy3, dz3);
				flag = true;
			}
		}
		bool flag2 = false;
		float num31 = num23 + num16;
		if (num31 > 0f)
		{
			float dx4 = num13;
			float dy4 = num14 - (float)(num11 | 1);
			float dz4 = num15;
			num17 += num31 * num31 * (num31 * num31) * Grad(seed, num7 + (num10 & 0x5205402B9270C86FL), num8 + (~num11 & 0x598CD327003817B5L), num9 + (num12 & 0x5BCC226E9FA0BACBL), dx4, dy4, dz4);
		}
		else
		{
			float num32 = num22 + num24 + num16;
			if (num32 > 0f)
			{
				float dx5 = num13 - (float)(num10 | 1);
				float dy5 = num14;
				float dz5 = num15 - (float)(num12 | 1);
				num17 += num32 * num32 * (num32 * num32) * Grad(seed, num7 + (~num10 & 0x5205402B9270C86FL), num8 + (num11 & 0x598CD327003817B5L), num9 + (~num12 & 0x5BCC226E9FA0BACBL), dx5, dy5, dz5);
			}
			float num33 = num26 + num21;
			if (num33 > 0f)
			{
				float dx6 = num18;
				float dy6 = (float)(num11 | 1) + num19;
				float dz6 = num20;
				num17 += num33 * num33 * (num33 * num33) * Grad(seed2, num7 + 5910200641878280303L, num8 + (num11 & -5541215012557672598L), num9 + 6614699811220273867L, dx6, dy6, dz6);
				flag2 = true;
			}
		}
		bool flag3 = false;
		float num34 = num24 + num16;
		if (num34 > 0f)
		{
			float dx7 = num13;
			float dy7 = num14;
			float dz7 = num15 - (float)(num12 | 1);
			num17 += num34 * num34 * (num34 * num34) * Grad(seed, num7 + (num10 & 0x5205402B9270C86FL), num8 + (num11 & 0x598CD327003817B5L), num9 + (~num12 & 0x5BCC226E9FA0BACBL), dx7, dy7, dz7);
		}
		else
		{
			float num35 = num22 + num23 + num16;
			if (num35 > 0f)
			{
				float dx8 = num13 - (float)(num10 | 1);
				float dy8 = num14 - (float)(num11 | 1);
				float dz8 = num15;
				num17 += num35 * num35 * (num35 * num35) * Grad(seed, num7 + (~num10 & 0x5205402B9270C86FL), num8 + (~num11 & 0x598CD327003817B5L), num9 + (num12 & 0x5BCC226E9FA0BACBL), dx8, dy8, dz8);
			}
			float num36 = num27 + num21;
			if (num36 > 0f)
			{
				float dx9 = num18;
				float dy9 = num19;
				float dz9 = (float)(num12 | 1) + num20;
				num17 += num36 * num36 * (num36 * num36) * Grad(seed2, num7 + 5910200641878280303L, num8 + 6452764530575939509L, num9 + (num12 & -5217344451269003882L), dx9, dy9, dz9);
				flag3 = true;
			}
		}
		if (!flag)
		{
			float num37 = num26 + num27 + num21;
			if (num37 > 0f)
			{
				float dx10 = num18;
				float dy10 = (float)(num11 | 1) + num19;
				float dz10 = (float)(num12 | 1) + num20;
				num17 += num37 * num37 * (num37 * num37) * Grad(seed2, num7 + 5910200641878280303L, num8 + (num11 & -5541215012557672598L), num9 + (num12 & -5217344451269003882L), dx10, dy10, dz10);
			}
		}
		if (!flag2)
		{
			float num38 = num25 + num27 + num21;
			if (num38 > 0f)
			{
				float dx11 = (float)(num10 | 1) + num18;
				float dy11 = num19;
				float dz11 = (float)(num12 | 1) + num20;
				num17 += num38 * num38 * (num38 * num38) * Grad(seed2, num7 + (num10 & -6626342789952991010L), num8 + 6452764530575939509L, num9 + (num12 & -5217344451269003882L), dx11, dy11, dz11);
			}
		}
		if (!flag3)
		{
			float num39 = num25 + num26 + num21;
			if (num39 > 0f)
			{
				float dx12 = (float)(num10 | 1) + num18;
				float dy12 = (float)(num11 | 1) + num19;
				float dz12 = num20;
				num17 += num39 * num39 * (num39 * num39) * Grad(seed2, num7 + (num10 & -6626342789952991010L), num8 + (num11 & -5541215012557672598L), num9 + 6614699811220273867L, dx12, dy12, dz12);
			}
		}
		return num17;
	}

	public static float Noise4_ImproveXYZ_ImproveXY(long seed, double x, double y, double z, double w)
	{
		double num = x + y;
		double num2 = num * -0.211324865405187;
		double num3 = z * 0.2886751345948129;
		double num4 = w * 1.118033988749894;
		double xs = x + (num3 + num4 + num2);
		double ys = y + (num3 + num4 + num2);
		double zs = num * -0.577350269189626 + (num3 + num4);
		double ws = z * -0.866025403784439 + num4;
		return Noise4_UnskewedBase(seed, xs, ys, zs, ws);
	}

	public static float Noise4_ImproveXYZ_ImproveXZ(long seed, double x, double y, double z, double w)
	{
		double num = x + z;
		double num2 = num * -0.211324865405187;
		double num3 = y * 0.2886751345948129;
		double num4 = w * 1.118033988749894;
		double xs = x + (num3 + num4 + num2);
		double zs = z + (num3 + num4 + num2);
		double ys = num * -0.577350269189626 + (num3 + num4);
		double ws = y * -0.866025403784439 + num4;
		return Noise4_UnskewedBase(seed, xs, ys, zs, ws);
	}

	public static float Noise4_ImproveXYZ(long seed, double x, double y, double z, double w)
	{
		double num = x + y + z;
		double num2 = w * 1.118033988749894;
		double num3 = num * (-1.0 / 6.0) + num2;
		double xs = x + num3;
		double ys = y + num3;
		double zs = z + num3;
		double ws = -0.5 * num + num2;
		return Noise4_UnskewedBase(seed, xs, ys, zs, ws);
	}

	public static float Noise4_Fallback(long seed, double x, double y, double z, double w)
	{
		double num = 0.30901700258255005 * (x + y + z + w);
		double xs = x + num;
		double ys = y + num;
		double zs = z + num;
		double ws = w + num;
		return Noise4_UnskewedBase(seed, xs, ys, zs, ws);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float Noise4_UnskewedBase(long seed, double xs, double ys, double zs, double ws)
	{
		int num = FastFloor(xs);
		int num2 = FastFloor(ys);
		int num3 = FastFloor(zs);
		int num4 = FastFloor(ws);
		float num5 = (float)(xs - (double)num);
		float num6 = (float)(ys - (double)num2);
		float num7 = (float)(zs - (double)num3);
		float num8 = (float)(ws - (double)num4);
		float num9 = (num5 + num6 + num7 + num8) * -0.1381966f;
		float num10 = num5 + num9;
		float num11 = num6 + num9;
		float num12 = num7 + num9;
		float num13 = num8 + num9;
		long num14 = num * 5910200641878280303L;
		long num15 = num2 * 6452764530575939509L;
		long num16 = num3 * 6614699811220273867L;
		long num17 = num4 * 6254464313819354443L;
		int num18 = (FastFloor(xs * 4.0) & 3) | ((FastFloor(ys * 4.0) & 3) << 2) | ((FastFloor(zs * 4.0) & 3) << 4) | ((FastFloor(ws * 4.0) & 3) << 6);
		float num19 = 0f;
		(short SecondaryIndexStart, short SecondaryIndexStop) tuple = LOOKUP_4D_A[num18];
		int item = tuple.SecondaryIndexStart;
		short item2 = tuple.SecondaryIndexStop;
		int num20 = item;
		int num21 = item2;
		for (int i = num20; i < num21; i++)
		{
			LatticeVertex4D latticeVertex4D = LOOKUP_4D_B[i];
			float num22 = num10 + latticeVertex4D.dx;
			float num23 = num11 + latticeVertex4D.dy;
			float num24 = num12 + latticeVertex4D.dz;
			float num25 = num13 + latticeVertex4D.dw;
			float num26 = num22 * num22 + num23 * num23 + (num24 * num24 + num25 * num25);
			if (num26 < 0.8f)
			{
				num26 -= 0.8f;
				num26 *= num26;
				num19 += num26 * num26 * Grad(seed, num14 + latticeVertex4D.xsvp, num15 + latticeVertex4D.ysvp, num16 + latticeVertex4D.zsvp, num17 + latticeVertex4D.wsvp, num22, num23, num24, num25);
			}
		}
		return num19;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[PublicizedFrom(EAccessModifier.Private)]
	public static float Grad(long seed, long xsvp, long ysvp, float dx, float dy)
	{
		long num = (seed ^ xsvp ^ ysvp) * 6026932503003350773L;
		int num2 = (int)(num ^ (num >> 58)) & 0xFE;
		return GRADIENTS_2D[num2 | 0] * dx + GRADIENTS_2D[num2 | 1] * dy;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[PublicizedFrom(EAccessModifier.Private)]
	public static float Grad(long seed, long xrvp, long yrvp, long zrvp, float dx, float dy, float dz)
	{
		long num = (seed ^ xrvp ^ (yrvp ^ zrvp)) * 6026932503003350773L;
		int num2 = (int)(num ^ (num >> 58)) & 0x3FC;
		return GRADIENTS_3D[num2 | 0] * dx + GRADIENTS_3D[num2 | 1] * dy + GRADIENTS_3D[num2 | 2] * dz;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[PublicizedFrom(EAccessModifier.Private)]
	public static float Grad(long seed, long xsvp, long ysvp, long zsvp, long wsvp, float dx, float dy, float dz, float dw)
	{
		long num = (seed ^ (xsvp ^ ysvp) ^ (zsvp ^ wsvp)) * 6026932503003350773L;
		int num2 = (int)(num ^ (num >> 57)) & 0x7FC;
		return GRADIENTS_4D[num2 | 0] * dx + GRADIENTS_4D[num2 | 1] * dy + (GRADIENTS_4D[num2 | 2] * dz + GRADIENTS_4D[num2 | 3] * dw);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[PublicizedFrom(EAccessModifier.Private)]
	public static int FastFloor(double x)
	{
		int num = (int)x;
		if (!(x < (double)num))
		{
			return num;
		}
		return num - 1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	static OpenSimplex2S()
	{
		GRADIENTS_2D = new float[256];
		float[] array = new float[48]
		{
			0.38268343f, 0.9238795f, 0.9238795f, 0.38268343f, 0.9238795f, -0.38268343f, 0.38268343f, -0.9238795f, -0.38268343f, -0.9238795f,
			-0.9238795f, -0.38268343f, -0.9238795f, 0.38268343f, -0.38268343f, 0.9238795f, 0.13052619f, 0.9914449f, 0.6087614f, 0.7933533f,
			0.7933533f, 0.6087614f, 0.9914449f, 0.13052619f, 0.9914449f, -0.13052619f, 0.7933533f, -0.6087614f, 0.6087614f, -0.7933533f,
			0.13052619f, -0.9914449f, -0.13052619f, -0.9914449f, -0.6087614f, -0.7933533f, -0.7933533f, -0.6087614f, -0.9914449f, -0.13052619f,
			-0.9914449f, 0.13052619f, -0.7933533f, 0.6087614f, -0.6087614f, 0.7933533f, -0.13052619f, 0.9914449f
		};
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = (float)((double)array[i] / 0.05481866495625118);
		}
		int num = 0;
		int num2 = 0;
		while (num < GRADIENTS_2D.Length)
		{
			if (num2 == array.Length)
			{
				num2 = 0;
			}
			GRADIENTS_2D[num] = array[num2];
			num++;
			num2++;
		}
		GRADIENTS_3D = new float[1024];
		float[] array2 = new float[192]
		{
			2.2247448f, 2.2247448f, -1f, 0f, 2.2247448f, 2.2247448f, 1f, 0f, 3.0862665f, 1.1721513f,
			0f, 0f, 1.1721513f, 3.0862665f, 0f, 0f, -2.2247448f, 2.2247448f, -1f, 0f,
			-2.2247448f, 2.2247448f, 1f, 0f, -1.1721513f, 3.0862665f, 0f, 0f, -3.0862665f, 1.1721513f,
			0f, 0f, -1f, -2.2247448f, -2.2247448f, 0f, 1f, -2.2247448f, -2.2247448f, 0f,
			0f, -3.0862665f, -1.1721513f, 0f, 0f, -1.1721513f, -3.0862665f, 0f, -1f, -2.2247448f,
			2.2247448f, 0f, 1f, -2.2247448f, 2.2247448f, 0f, 0f, -1.1721513f, 3.0862665f, 0f,
			0f, -3.0862665f, 1.1721513f, 0f, -2.2247448f, -2.2247448f, -1f, 0f, -2.2247448f, -2.2247448f,
			1f, 0f, -3.0862665f, -1.1721513f, 0f, 0f, -1.1721513f, -3.0862665f, 0f, 0f,
			-2.2247448f, -1f, -2.2247448f, 0f, -2.2247448f, 1f, -2.2247448f, 0f, -1.1721513f, 0f,
			-3.0862665f, 0f, -3.0862665f, 0f, -1.1721513f, 0f, -2.2247448f, -1f, 2.2247448f, 0f,
			-2.2247448f, 1f, 2.2247448f, 0f, -3.0862665f, 0f, 1.1721513f, 0f, -1.1721513f, 0f,
			3.0862665f, 0f, -1f, 2.2247448f, -2.2247448f, 0f, 1f, 2.2247448f, -2.2247448f, 0f,
			0f, 1.1721513f, -3.0862665f, 0f, 0f, 3.0862665f, -1.1721513f, 0f, -1f, 2.2247448f,
			2.2247448f, 0f, 1f, 2.2247448f, 2.2247448f, 0f, 0f, 3.0862665f, 1.1721513f, 0f,
			0f, 1.1721513f, 3.0862665f, 0f, 2.2247448f, -2.2247448f, -1f, 0f, 2.2247448f, -2.2247448f,
			1f, 0f, 1.1721513f, -3.0862665f, 0f, 0f, 3.0862665f, -1.1721513f, 0f, 0f,
			2.2247448f, -1f, -2.2247448f, 0f, 2.2247448f, 1f, -2.2247448f, 0f, 3.0862665f, 0f,
			-1.1721513f, 0f, 1.1721513f, 0f, -3.0862665f, 0f, 2.2247448f, -1f, 2.2247448f, 0f,
			2.2247448f, 1f, 2.2247448f, 0f, 1.1721513f, 0f, 3.0862665f, 0f, 3.0862665f, 0f,
			1.1721513f, 0f
		};
		for (int j = 0; j < array2.Length; j++)
		{
			array2[j] = (float)((double)array2[j] / 0.2781926117527186);
		}
		int num3 = 0;
		int num4 = 0;
		while (num3 < GRADIENTS_3D.Length)
		{
			if (num4 == array2.Length)
			{
				num4 = 0;
			}
			GRADIENTS_3D[num3] = array2[num4];
			num3++;
			num4++;
		}
		GRADIENTS_4D = new float[2048];
		float[] array3 = new float[640]
		{
			-0.6740059f, -0.32398477f, -0.32398477f, 0.5794685f, -0.7504884f, -0.40046722f, 0.15296486f, 0.502986f, -0.7504884f, 0.15296486f,
			-0.40046722f, 0.502986f, -0.8828162f, 0.08164729f, 0.08164729f, 0.4553054f, -0.4553054f, -0.08164729f, -0.08164729f, 0.8828162f,
			-0.502986f, -0.15296486f, 0.40046722f, 0.7504884f, -0.502986f, 0.40046722f, -0.15296486f, 0.7504884f, -0.5794685f, 0.32398477f,
			0.32398477f, 0.6740059f, -0.6740059f, -0.32398477f, 0.5794685f, -0.32398477f, -0.7504884f, -0.40046722f, 0.502986f, 0.15296486f,
			-0.7504884f, 0.15296486f, 0.502986f, -0.40046722f, -0.8828162f, 0.08164729f, 0.4553054f, 0.08164729f, -0.4553054f, -0.08164729f,
			0.8828162f, -0.08164729f, -0.502986f, -0.15296486f, 0.7504884f, 0.40046722f, -0.502986f, 0.40046722f, 0.7504884f, -0.15296486f,
			-0.5794685f, 0.32398477f, 0.6740059f, 0.32398477f, -0.6740059f, 0.5794685f, -0.32398477f, -0.32398477f, -0.7504884f, 0.502986f,
			-0.40046722f, 0.15296486f, -0.7504884f, 0.502986f, 0.15296486f, -0.40046722f, -0.8828162f, 0.4553054f, 0.08164729f, 0.08164729f,
			-0.4553054f, 0.8828162f, -0.08164729f, -0.08164729f, -0.502986f, 0.7504884f, -0.15296486f, 0.40046722f, -0.502986f, 0.7504884f,
			0.40046722f, -0.15296486f, -0.5794685f, 0.6740059f, 0.32398477f, 0.32398477f, 0.5794685f, -0.6740059f, -0.32398477f, -0.32398477f,
			0.502986f, -0.7504884f, -0.40046722f, 0.15296486f, 0.502986f, -0.7504884f, 0.15296486f, -0.40046722f, 0.4553054f, -0.8828162f,
			0.08164729f, 0.08164729f, 0.8828162f, -0.4553054f, -0.08164729f, -0.08164729f, 0.7504884f, -0.502986f, -0.15296486f, 0.40046722f,
			0.7504884f, -0.502986f, 0.40046722f, -0.15296486f, 0.6740059f, -0.5794685f, 0.32398477f, 0.32398477f, -0.753341f, -0.3796829f,
			-0.3796829f, -0.3796829f, -0.78216845f, -0.43214726f, -0.43214726f, 0.121284805f, -0.78216845f, -0.43214726f, 0.121284805f, -0.43214726f,
			-0.78216845f, 0.121284805f, -0.43214726f, -0.43214726f, -0.85865086f, -0.5086297f, 0.04480237f, 0.04480237f, -0.85865086f, 0.04480237f,
			-0.5086297f, 0.04480237f, -0.85865086f, 0.04480237f, 0.04480237f, -0.5086297f, -0.9982829f, -0.033819415f, -0.033819415f, -0.033819415f,
			-0.3796829f, -0.753341f, -0.3796829f, -0.3796829f, -0.43214726f, -0.78216845f, -0.43214726f, 0.121284805f, -0.43214726f, -0.78216845f,
			0.121284805f, -0.43214726f, 0.121284805f, -0.78216845f, -0.43214726f, -0.43214726f, -0.5086297f, -0.85865086f, 0.04480237f, 0.04480237f,
			0.04480237f, -0.85865086f, -0.5086297f, 0.04480237f, 0.04480237f, -0.85865086f, 0.04480237f, -0.5086297f, -0.033819415f, -0.9982829f,
			-0.033819415f, -0.033819415f, -0.3796829f, -0.3796829f, -0.753341f, -0.3796829f, -0.43214726f, -0.43214726f, -0.78216845f, 0.121284805f,
			-0.43214726f, 0.121284805f, -0.78216845f, -0.43214726f, 0.121284805f, -0.43214726f, -0.78216845f, -0.43214726f, -0.5086297f, 0.04480237f,
			-0.85865086f, 0.04480237f, 0.04480237f, -0.5086297f, -0.85865086f, 0.04480237f, 0.04480237f, 0.04480237f, -0.85865086f, -0.5086297f,
			-0.033819415f, -0.033819415f, -0.9982829f, -0.033819415f, -0.3796829f, -0.3796829f, -0.3796829f, -0.753341f, -0.43214726f, -0.43214726f,
			0.121284805f, -0.78216845f, -0.43214726f, 0.121284805f, -0.43214726f, -0.78216845f, 0.121284805f, -0.43214726f, -0.43214726f, -0.78216845f,
			-0.5086297f, 0.04480237f, 0.04480237f, -0.85865086f, 0.04480237f, -0.5086297f, 0.04480237f, -0.85865086f, 0.04480237f, 0.04480237f,
			-0.5086297f, -0.85865086f, -0.033819415f, -0.033819415f, -0.033819415f, -0.9982829f, -0.32398477f, -0.6740059f, -0.32398477f, 0.5794685f,
			-0.40046722f, -0.7504884f, 0.15296486f, 0.502986f, 0.15296486f, -0.7504884f, -0.40046722f, 0.502986f, 0.08164729f, -0.8828162f,
			0.08164729f, 0.4553054f, -0.08164729f, -0.4553054f, -0.08164729f, 0.8828162f, -0.15296486f, -0.502986f, 0.40046722f, 0.7504884f,
			0.40046722f, -0.502986f, -0.15296486f, 0.7504884f, 0.32398477f, -0.5794685f, 0.32398477f, 0.6740059f, -0.32398477f, -0.32398477f,
			-0.6740059f, 0.5794685f, -0.40046722f, 0.15296486f, -0.7504884f, 0.502986f, 0.15296486f, -0.40046722f, -0.7504884f, 0.502986f,
			0.08164729f, 0.08164729f, -0.8828162f, 0.4553054f, -0.08164729f, -0.08164729f, -0.4553054f, 0.8828162f, -0.15296486f, 0.40046722f,
			-0.502986f, 0.7504884f, 0.40046722f, -0.15296486f, -0.502986f, 0.7504884f, 0.32398477f, 0.32398477f, -0.5794685f, 0.6740059f,
			-0.32398477f, -0.6740059f, 0.5794685f, -0.32398477f, -0.40046722f, -0.7504884f, 0.502986f, 0.15296486f, 0.15296486f, -0.7504884f,
			0.502986f, -0.40046722f, 0.08164729f, -0.8828162f, 0.4553054f, 0.08164729f, -0.08164729f, -0.4553054f, 0.8828162f, -0.08164729f,
			-0.15296486f, -0.502986f, 0.7504884f, 0.40046722f, 0.40046722f, -0.502986f, 0.7504884f, -0.15296486f, 0.32398477f, -0.5794685f,
			0.6740059f, 0.32398477f, -0.32398477f, -0.32398477f, 0.5794685f, -0.6740059f, -0.40046722f, 0.15296486f, 0.502986f, -0.7504884f,
			0.15296486f, -0.40046722f, 0.502986f, -0.7504884f, 0.08164729f, 0.08164729f, 0.4553054f, -0.8828162f, -0.08164729f, -0.08164729f,
			0.8828162f, -0.4553054f, -0.15296486f, 0.40046722f, 0.7504884f, -0.502986f, 0.40046722f, -0.15296486f, 0.7504884f, -0.502986f,
			0.32398477f, 0.32398477f, 0.6740059f, -0.5794685f, -0.32398477f, 0.5794685f, -0.6740059f, -0.32398477f, -0.40046722f, 0.502986f,
			-0.7504884f, 0.15296486f, 0.15296486f, 0.502986f, -0.7504884f, -0.40046722f, 0.08164729f, 0.4553054f, -0.8828162f, 0.08164729f,
			-0.08164729f, 0.8828162f, -0.4553054f, -0.08164729f, -0.15296486f, 0.7504884f, -0.502986f, 0.40046722f, 0.40046722f, 0.7504884f,
			-0.502986f, -0.15296486f, 0.32398477f, 0.6740059f, -0.5794685f, 0.32398477f, -0.32398477f, 0.5794685f, -0.32398477f, -0.6740059f,
			-0.40046722f, 0.502986f, 0.15296486f, -0.7504884f, 0.15296486f, 0.502986f, -0.40046722f, -0.7504884f, 0.08164729f, 0.4553054f,
			0.08164729f, -0.8828162f, -0.08164729f, 0.8828162f, -0.08164729f, -0.4553054f, -0.15296486f, 0.7504884f, 0.40046722f, -0.502986f,
			0.40046722f, 0.7504884f, -0.15296486f, -0.502986f, 0.32398477f, 0.6740059f, 0.32398477f, -0.5794685f, 0.5794685f, -0.32398477f,
			-0.6740059f, -0.32398477f, 0.502986f, -0.40046722f, -0.7504884f, 0.15296486f, 0.502986f, 0.15296486f, -0.7504884f, -0.40046722f,
			0.4553054f, 0.08164729f, -0.8828162f, 0.08164729f, 0.8828162f, -0.08164729f, -0.4553054f, -0.08164729f, 0.7504884f, -0.15296486f,
			-0.502986f, 0.40046722f, 0.7504884f, 0.40046722f, -0.502986f, -0.15296486f, 0.6740059f, 0.32398477f, -0.5794685f, 0.32398477f,
			0.5794685f, -0.32398477f, -0.32398477f, -0.6740059f, 0.502986f, -0.40046722f, 0.15296486f, -0.7504884f, 0.502986f, 0.15296486f,
			-0.40046722f, -0.7504884f, 0.4553054f, 0.08164729f, 0.08164729f, -0.8828162f, 0.8828162f, -0.08164729f, -0.08164729f, -0.4553054f,
			0.7504884f, -0.15296486f, 0.40046722f, -0.502986f, 0.7504884f, 0.40046722f, -0.15296486f, -0.502986f, 0.6740059f, 0.32398477f,
			0.32398477f, -0.5794685f, 0.033819415f, 0.033819415f, 0.033819415f, 0.9982829f, -0.04480237f, -0.04480237f, 0.5086297f, 0.85865086f,
			-0.04480237f, 0.5086297f, -0.04480237f, 0.85865086f, -0.121284805f, 0.43214726f, 0.43214726f, 0.78216845f, 0.5086297f, -0.04480237f,
			-0.04480237f, 0.85865086f, 0.43214726f, -0.121284805f, 0.43214726f, 0.78216845f, 0.43214726f, 0.43214726f, -0.121284805f, 0.78216845f,
			0.3796829f, 0.3796829f, 0.3796829f, 0.753341f, 0.033819415f, 0.033819415f, 0.9982829f, 0.033819415f, -0.04480237f, 0.04480237f,
			0.85865086f, 0.5086297f, -0.04480237f, 0.5086297f, 0.85865086f, -0.04480237f, -0.121284805f, 0.43214726f, 0.78216845f, 0.43214726f,
			0.5086297f, -0.04480237f, 0.85865086f, -0.04480237f, 0.43214726f, -0.121284805f, 0.78216845f, 0.43214726f, 0.43214726f, 0.43214726f,
			0.78216845f, -0.121284805f, 0.3796829f, 0.3796829f, 0.753341f, 0.3796829f, 0.033819415f, 0.9982829f, 0.033819415f, 0.033819415f,
			-0.04480237f, 0.85865086f, -0.04480237f, 0.5086297f, -0.04480237f, 0.85865086f, 0.5086297f, -0.04480237f, -0.121284805f, 0.78216845f,
			0.43214726f, 0.43214726f, 0.5086297f, 0.85865086f, -0.04480237f, -0.04480237f, 0.43214726f, 0.78216845f, -0.121284805f, 0.43214726f,
			0.43214726f, 0.78216845f, 0.43214726f, -0.121284805f, 0.3796829f, 0.753341f, 0.3796829f, 0.3796829f, 0.9982829f, 0.033819415f,
			0.033819415f, 0.033819415f, 0.85865086f, -0.04480237f, -0.04480237f, 0.5086297f, 0.85865086f, -0.04480237f, 0.5086297f, -0.04480237f,
			0.78216845f, -0.121284805f, 0.43214726f, 0.43214726f, 0.85865086f, 0.5086297f, -0.04480237f, -0.04480237f, 0.78216845f, 0.43214726f,
			-0.121284805f, 0.43214726f, 0.78216845f, 0.43214726f, 0.43214726f, -0.121284805f, 0.753341f, 0.3796829f, 0.3796829f, 0.3796829f
		};
		for (int k = 0; k < array3.Length; k++)
		{
			array3[k] = (float)((double)array3[k] / 0.11127401889945551);
		}
		int num5 = 0;
		int num6 = 0;
		while (num5 < GRADIENTS_4D.Length)
		{
			if (num6 == array3.Length)
			{
				num6 = 0;
			}
			GRADIENTS_4D[num5] = array3[num6];
			num5++;
			num6++;
		}
		int[][] array4 = new int[256][]
		{
			new int[20]
			{
				21, 69, 81, 84, 85, 86, 89, 90, 101, 102,
				105, 106, 149, 150, 153, 154, 165, 166, 169, 170
			},
			new int[15]
			{
				21, 69, 81, 85, 86, 89, 90, 101, 102, 106,
				149, 150, 154, 166, 170
			},
			new int[16]
			{
				1, 5, 17, 21, 65, 69, 81, 85, 86, 90,
				102, 106, 150, 154, 166, 170
			},
			new int[17]
			{
				1, 21, 22, 69, 70, 81, 82, 85, 86, 90,
				102, 106, 150, 154, 166, 170, 171
			},
			new int[15]
			{
				21, 69, 84, 85, 86, 89, 90, 101, 105, 106,
				149, 153, 154, 169, 170
			},
			new int[16]
			{
				5, 21, 69, 85, 86, 89, 90, 101, 102, 105,
				106, 149, 150, 153, 154, 170
			},
			new int[12]
			{
				5, 21, 69, 85, 86, 89, 90, 102, 106, 150,
				154, 170
			},
			new int[15]
			{
				5, 21, 22, 69, 70, 85, 86, 89, 90, 102,
				106, 150, 154, 170, 171
			},
			new int[16]
			{
				4, 5, 20, 21, 68, 69, 84, 85, 89, 90,
				105, 106, 153, 154, 169, 170
			},
			new int[12]
			{
				5, 21, 69, 85, 86, 89, 90, 105, 106, 153,
				154, 170
			},
			new int[10] { 5, 21, 69, 85, 86, 89, 90, 106, 154, 170 },
			new int[14]
			{
				5, 21, 22, 69, 70, 85, 86, 89, 90, 91,
				106, 154, 170, 171
			},
			new int[17]
			{
				4, 21, 25, 69, 73, 84, 85, 88, 89, 90,
				105, 106, 153, 154, 169, 170, 174
			},
			new int[15]
			{
				5, 21, 25, 69, 73, 85, 86, 89, 90, 105,
				106, 153, 154, 170, 174
			},
			new int[14]
			{
				5, 21, 25, 69, 73, 85, 86, 89, 90, 94,
				106, 154, 170, 174
			},
			new int[17]
			{
				5, 21, 26, 69, 74, 85, 86, 89, 90, 91,
				94, 106, 154, 170, 171, 174, 175
			},
			new int[15]
			{
				21, 81, 84, 85, 86, 89, 101, 102, 105, 106,
				149, 165, 166, 169, 170
			},
			new int[16]
			{
				17, 21, 81, 85, 86, 89, 90, 101, 102, 105,
				106, 149, 150, 165, 166, 170
			},
			new int[12]
			{
				17, 21, 81, 85, 86, 90, 101, 102, 106, 150,
				166, 170
			},
			new int[15]
			{
				17, 21, 22, 81, 82, 85, 86, 90, 101, 102,
				106, 150, 166, 170, 171
			},
			new int[16]
			{
				20, 21, 84, 85, 86, 89, 90, 101, 102, 105,
				106, 149, 153, 165, 169, 170
			},
			new int[14]
			{
				21, 85, 86, 89, 90, 101, 102, 105, 106, 149,
				154, 166, 169, 170
			},
			new int[14]
			{
				21, 85, 86, 89, 90, 101, 102, 105, 106, 150,
				154, 166, 170, 171
			},
			new int[13]
			{
				21, 22, 85, 86, 90, 102, 106, 107, 150, 154,
				166, 170, 171
			},
			new int[12]
			{
				20, 21, 84, 85, 89, 90, 101, 105, 106, 153,
				169, 170
			},
			new int[14]
			{
				21, 85, 86, 89, 90, 101, 102, 105, 106, 153,
				154, 169, 170, 174
			},
			new int[11]
			{
				21, 85, 86, 89, 90, 101, 102, 105, 106, 154,
				170
			},
			new int[12]
			{
				21, 22, 85, 86, 89, 90, 102, 106, 107, 154,
				170, 171
			},
			new int[15]
			{
				20, 21, 25, 84, 85, 88, 89, 90, 101, 105,
				106, 153, 169, 170, 174
			},
			new int[13]
			{
				21, 25, 85, 89, 90, 105, 106, 110, 153, 154,
				169, 170, 174
			},
			new int[12]
			{
				21, 25, 85, 86, 89, 90, 105, 106, 110, 154,
				170, 174
			},
			new int[14]
			{
				21, 26, 85, 86, 89, 90, 106, 107, 110, 154,
				170, 171, 174, 175
			},
			new int[16]
			{
				16, 17, 20, 21, 80, 81, 84, 85, 101, 102,
				105, 106, 165, 166, 169, 170
			},
			new int[12]
			{
				17, 21, 81, 85, 86, 101, 102, 105, 106, 165,
				166, 170
			},
			new int[10] { 17, 21, 81, 85, 86, 101, 102, 106, 166, 170 },
			new int[14]
			{
				17, 21, 22, 81, 82, 85, 86, 101, 102, 103,
				106, 166, 170, 171
			},
			new int[12]
			{
				20, 21, 84, 85, 89, 101, 102, 105, 106, 165,
				169, 170
			},
			new int[14]
			{
				21, 85, 86, 89, 90, 101, 102, 105, 106, 165,
				166, 169, 170, 186
			},
			new int[11]
			{
				21, 85, 86, 89, 90, 101, 102, 105, 106, 166,
				170
			},
			new int[12]
			{
				21, 22, 85, 86, 90, 101, 102, 106, 107, 166,
				170, 171
			},
			new int[10] { 20, 21, 84, 85, 89, 101, 105, 106, 169, 170 },
			new int[11]
			{
				21, 85, 86, 89, 90, 101, 102, 105, 106, 169,
				170
			},
			new int[10] { 21, 85, 86, 89, 90, 101, 102, 105, 106, 170 },
			new int[13]
			{
				21, 22, 85, 86, 89, 90, 101, 102, 105, 106,
				107, 170, 171
			},
			new int[14]
			{
				20, 21, 25, 84, 85, 88, 89, 101, 105, 106,
				109, 169, 170, 174
			},
			new int[12]
			{
				21, 25, 85, 89, 90, 101, 105, 106, 110, 169,
				170, 174
			},
			new int[13]
			{
				21, 25, 85, 86, 89, 90, 101, 102, 105, 106,
				110, 170, 174
			},
			new int[15]
			{
				21, 85, 86, 89, 90, 102, 105, 106, 107, 110,
				154, 170, 171, 174, 175
			},
			new int[17]
			{
				16, 21, 37, 81, 84, 85, 97, 100, 101, 102,
				105, 106, 165, 166, 169, 170, 186
			},
			new int[15]
			{
				17, 21, 37, 81, 85, 86, 97, 101, 102, 105,
				106, 165, 166, 170, 186
			},
			new int[14]
			{
				17, 21, 37, 81, 85, 86, 97, 101, 102, 106,
				118, 166, 170, 186
			},
			new int[17]
			{
				17, 21, 38, 81, 85, 86, 98, 101, 102, 103,
				106, 118, 166, 170, 171, 186, 187
			},
			new int[15]
			{
				20, 21, 37, 84, 85, 89, 100, 101, 102, 105,
				106, 165, 169, 170, 186
			},
			new int[13]
			{
				21, 37, 85, 101, 102, 105, 106, 122, 165, 166,
				169, 170, 186
			},
			new int[12]
			{
				21, 37, 85, 86, 101, 102, 105, 106, 122, 166,
				170, 186
			},
			new int[14]
			{
				21, 38, 85, 86, 101, 102, 106, 107, 122, 166,
				170, 171, 186, 187
			},
			new int[14]
			{
				20, 21, 37, 84, 85, 89, 100, 101, 105, 106,
				121, 169, 170, 186
			},
			new int[12]
			{
				21, 37, 85, 89, 101, 102, 105, 106, 122, 169,
				170, 186
			},
			new int[13]
			{
				21, 37, 85, 86, 89, 90, 101, 102, 105, 106,
				122, 170, 186
			},
			new int[15]
			{
				21, 85, 86, 90, 101, 102, 105, 106, 107, 122,
				166, 170, 171, 186, 187
			},
			new int[17]
			{
				20, 21, 41, 84, 85, 89, 101, 104, 105, 106,
				109, 121, 169, 170, 174, 186, 190
			},
			new int[14]
			{
				21, 41, 85, 89, 101, 105, 106, 110, 122, 169,
				170, 174, 186, 190
			},
			new int[15]
			{
				21, 85, 89, 90, 101, 102, 105, 106, 110, 122,
				169, 170, 174, 186, 190
			},
			new int[17]
			{
				21, 85, 86, 89, 90, 101, 102, 105, 106, 107,
				110, 122, 170, 171, 174, 186, 191
			},
			new int[15]
			{
				69, 81, 84, 85, 86, 89, 101, 149, 150, 153,
				154, 165, 166, 169, 170
			},
			new int[16]
			{
				65, 69, 81, 85, 86, 89, 90, 101, 102, 149,
				150, 153, 154, 165, 166, 170
			},
			new int[12]
			{
				65, 69, 81, 85, 86, 90, 102, 149, 150, 154,
				166, 170
			},
			new int[15]
			{
				65, 69, 70, 81, 82, 85, 86, 90, 102, 149,
				150, 154, 166, 170, 171
			},
			new int[16]
			{
				68, 69, 84, 85, 86, 89, 90, 101, 105, 149,
				150, 153, 154, 165, 169, 170
			},
			new int[14]
			{
				69, 85, 86, 89, 90, 101, 106, 149, 150, 153,
				154, 166, 169, 170
			},
			new int[14]
			{
				69, 85, 86, 89, 90, 102, 106, 149, 150, 153,
				154, 166, 170, 171
			},
			new int[13]
			{
				69, 70, 85, 86, 90, 102, 106, 150, 154, 155,
				166, 170, 171
			},
			new int[12]
			{
				68, 69, 84, 85, 89, 90, 105, 149, 153, 154,
				169, 170
			},
			new int[14]
			{
				69, 85, 86, 89, 90, 105, 106, 149, 150, 153,
				154, 169, 170, 174
			},
			new int[11]
			{
				69, 85, 86, 89, 90, 106, 149, 150, 153, 154,
				170
			},
			new int[12]
			{
				69, 70, 85, 86, 89, 90, 106, 150, 154, 155,
				170, 171
			},
			new int[15]
			{
				68, 69, 73, 84, 85, 88, 89, 90, 105, 149,
				153, 154, 169, 170, 174
			},
			new int[13]
			{
				69, 73, 85, 89, 90, 105, 106, 153, 154, 158,
				169, 170, 174
			},
			new int[12]
			{
				69, 73, 85, 86, 89, 90, 106, 153, 154, 158,
				170, 174
			},
			new int[14]
			{
				69, 74, 85, 86, 89, 90, 106, 154, 155, 158,
				170, 171, 174, 175
			},
			new int[16]
			{
				80, 81, 84, 85, 86, 89, 101, 102, 105, 149,
				150, 153, 165, 166, 169, 170
			},
			new int[14]
			{
				81, 85, 86, 89, 101, 102, 106, 149, 150, 154,
				165, 166, 169, 170
			},
			new int[14]
			{
				81, 85, 86, 90, 101, 102, 106, 149, 150, 154,
				165, 166, 170, 171
			},
			new int[13]
			{
				81, 82, 85, 86, 90, 102, 106, 150, 154, 166,
				167, 170, 171
			},
			new int[14]
			{
				84, 85, 86, 89, 101, 105, 106, 149, 153, 154,
				165, 166, 169, 170
			},
			new int[16]
			{
				85, 86, 89, 90, 101, 102, 105, 106, 149, 150,
				153, 154, 165, 166, 169, 170
			},
			new int[16]
			{
				21, 69, 81, 85, 86, 89, 90, 101, 102, 106,
				149, 150, 154, 166, 170, 171
			},
			new int[10] { 85, 86, 90, 102, 106, 150, 154, 166, 170, 171 },
			new int[14]
			{
				84, 85, 89, 90, 101, 105, 106, 149, 153, 154,
				165, 169, 170, 174
			},
			new int[16]
			{
				21, 69, 84, 85, 86, 89, 90, 101, 105, 106,
				149, 153, 154, 169, 170, 174
			},
			new int[19]
			{
				21, 69, 85, 86, 89, 90, 101, 102, 105, 106,
				149, 150, 153, 154, 166, 169, 170, 171, 174
			},
			new int[11]
			{
				85, 86, 89, 90, 102, 106, 150, 154, 166, 170,
				171
			},
			new int[13]
			{
				84, 85, 88, 89, 90, 105, 106, 153, 154, 169,
				170, 173, 174
			},
			new int[10] { 85, 89, 90, 105, 106, 153, 154, 169, 170, 174 },
			new int[11]
			{
				85, 86, 89, 90, 105, 106, 153, 154, 169, 170,
				174
			},
			new int[10] { 85, 86, 89, 90, 106, 154, 170, 171, 174, 175 },
			new int[12]
			{
				80, 81, 84, 85, 101, 102, 105, 149, 165, 166,
				169, 170
			},
			new int[14]
			{
				81, 85, 86, 101, 102, 105, 106, 149, 150, 165,
				166, 169, 170, 186
			},
			new int[11]
			{
				81, 85, 86, 101, 102, 106, 149, 150, 165, 166,
				170
			},
			new int[12]
			{
				81, 82, 85, 86, 101, 102, 106, 150, 166, 167,
				170, 171
			},
			new int[14]
			{
				84, 85, 89, 101, 102, 105, 106, 149, 153, 165,
				166, 169, 170, 186
			},
			new int[16]
			{
				21, 81, 84, 85, 86, 89, 101, 102, 105, 106,
				149, 165, 166, 169, 170, 186
			},
			new int[19]
			{
				21, 81, 85, 86, 89, 90, 101, 102, 105, 106,
				149, 150, 154, 165, 166, 169, 170, 171, 186
			},
			new int[11]
			{
				85, 86, 90, 101, 102, 106, 150, 154, 166, 170,
				171
			},
			new int[11]
			{
				84, 85, 89, 101, 105, 106, 149, 153, 165, 169,
				170
			},
			new int[19]
			{
				21, 84, 85, 86, 89, 90, 101, 102, 105, 106,
				149, 153, 154, 165, 166, 169, 170, 174, 186
			},
			new int[13]
			{
				21, 85, 86, 89, 90, 101, 102, 105, 106, 154,
				166, 169, 170
			},
			new int[14]
			{
				21, 85, 86, 89, 90, 101, 102, 105, 106, 150,
				154, 166, 170, 171
			},
			new int[12]
			{
				84, 85, 88, 89, 101, 105, 106, 153, 169, 170,
				173, 174
			},
			new int[11]
			{
				85, 89, 90, 101, 105, 106, 153, 154, 169, 170,
				174
			},
			new int[14]
			{
				21, 85, 86, 89, 90, 101, 102, 105, 106, 153,
				154, 169, 170, 174
			},
			new int[13]
			{
				21, 85, 86, 89, 90, 102, 105, 106, 154, 170,
				171, 174, 175
			},
			new int[15]
			{
				80, 81, 84, 85, 97, 100, 101, 102, 105, 149,
				165, 166, 169, 170, 186
			},
			new int[13]
			{
				81, 85, 97, 101, 102, 105, 106, 165, 166, 169,
				170, 182, 186
			},
			new int[12]
			{
				81, 85, 86, 97, 101, 102, 106, 165, 166, 170,
				182, 186
			},
			new int[14]
			{
				81, 85, 86, 98, 101, 102, 106, 166, 167, 170,
				171, 182, 186, 187
			},
			new int[13]
			{
				84, 85, 100, 101, 102, 105, 106, 165, 166, 169,
				170, 185, 186
			},
			new int[10] { 85, 101, 102, 105, 106, 165, 166, 169, 170, 186 },
			new int[11]
			{
				85, 86, 101, 102, 105, 106, 165, 166, 169, 170,
				186
			},
			new int[10] { 85, 86, 101, 102, 106, 166, 170, 171, 186, 187 },
			new int[12]
			{
				84, 85, 89, 100, 101, 105, 106, 165, 169, 170,
				185, 186
			},
			new int[11]
			{
				85, 89, 101, 102, 105, 106, 165, 166, 169, 170,
				186
			},
			new int[14]
			{
				21, 85, 86, 89, 90, 101, 102, 105, 106, 165,
				166, 169, 170, 186
			},
			new int[13]
			{
				21, 85, 86, 90, 101, 102, 105, 106, 166, 170,
				171, 186, 187
			},
			new int[14]
			{
				84, 85, 89, 101, 104, 105, 106, 169, 170, 173,
				174, 185, 186, 190
			},
			new int[10] { 85, 89, 101, 105, 106, 169, 170, 174, 186, 190 },
			new int[13]
			{
				21, 85, 89, 90, 101, 102, 105, 106, 169, 170,
				174, 186, 190
			},
			new int[13]
			{
				85, 86, 89, 90, 101, 102, 105, 106, 170, 171,
				174, 186, 191
			},
			new int[16]
			{
				64, 65, 68, 69, 80, 81, 84, 85, 149, 150,
				153, 154, 165, 166, 169, 170
			},
			new int[12]
			{
				65, 69, 81, 85, 86, 149, 150, 153, 154, 165,
				166, 170
			},
			new int[10] { 65, 69, 81, 85, 86, 149, 150, 154, 166, 170 },
			new int[14]
			{
				65, 69, 70, 81, 82, 85, 86, 149, 150, 151,
				154, 166, 170, 171
			},
			new int[12]
			{
				68, 69, 84, 85, 89, 149, 150, 153, 154, 165,
				169, 170
			},
			new int[14]
			{
				69, 85, 86, 89, 90, 149, 150, 153, 154, 165,
				166, 169, 170, 234
			},
			new int[11]
			{
				69, 85, 86, 89, 90, 149, 150, 153, 154, 166,
				170
			},
			new int[12]
			{
				69, 70, 85, 86, 90, 149, 150, 154, 155, 166,
				170, 171
			},
			new int[10] { 68, 69, 84, 85, 89, 149, 153, 154, 169, 170 },
			new int[11]
			{
				69, 85, 86, 89, 90, 149, 150, 153, 154, 169,
				170
			},
			new int[10] { 69, 85, 86, 89, 90, 149, 150, 153, 154, 170 },
			new int[13]
			{
				69, 70, 85, 86, 89, 90, 149, 150, 153, 154,
				155, 170, 171
			},
			new int[14]
			{
				68, 69, 73, 84, 85, 88, 89, 149, 153, 154,
				157, 169, 170, 174
			},
			new int[12]
			{
				69, 73, 85, 89, 90, 149, 153, 154, 158, 169,
				170, 174
			},
			new int[13]
			{
				69, 73, 85, 86, 89, 90, 149, 150, 153, 154,
				158, 170, 174
			},
			new int[15]
			{
				69, 85, 86, 89, 90, 106, 150, 153, 154, 155,
				158, 170, 171, 174, 175
			},
			new int[12]
			{
				80, 81, 84, 85, 101, 149, 150, 153, 165, 166,
				169, 170
			},
			new int[14]
			{
				81, 85, 86, 101, 102, 149, 150, 153, 154, 165,
				166, 169, 170, 234
			},
			new int[11]
			{
				81, 85, 86, 101, 102, 149, 150, 154, 165, 166,
				170
			},
			new int[12]
			{
				81, 82, 85, 86, 102, 149, 150, 154, 166, 167,
				170, 171
			},
			new int[14]
			{
				84, 85, 89, 101, 105, 149, 150, 153, 154, 165,
				166, 169, 170, 234
			},
			new int[16]
			{
				69, 81, 84, 85, 86, 89, 101, 149, 150, 153,
				154, 165, 166, 169, 170, 234
			},
			new int[19]
			{
				69, 81, 85, 86, 89, 90, 101, 102, 106, 149,
				150, 153, 154, 165, 166, 169, 170, 171, 234
			},
			new int[11]
			{
				85, 86, 90, 102, 106, 149, 150, 154, 166, 170,
				171
			},
			new int[11]
			{
				84, 85, 89, 101, 105, 149, 153, 154, 165, 169,
				170
			},
			new int[19]
			{
				69, 84, 85, 86, 89, 90, 101, 105, 106, 149,
				150, 153, 154, 165, 166, 169, 170, 174, 234
			},
			new int[13]
			{
				69, 85, 86, 89, 90, 106, 149, 150, 153, 154,
				166, 169, 170
			},
			new int[14]
			{
				69, 85, 86, 89, 90, 102, 106, 149, 150, 153,
				154, 166, 170, 171
			},
			new int[12]
			{
				84, 85, 88, 89, 105, 149, 153, 154, 169, 170,
				173, 174
			},
			new int[11]
			{
				85, 89, 90, 105, 106, 149, 153, 154, 169, 170,
				174
			},
			new int[14]
			{
				69, 85, 86, 89, 90, 105, 106, 149, 150, 153,
				154, 169, 170, 174
			},
			new int[13]
			{
				69, 85, 86, 89, 90, 106, 150, 153, 154, 170,
				171, 174, 175
			},
			new int[10] { 80, 81, 84, 85, 101, 149, 165, 166, 169, 170 },
			new int[11]
			{
				81, 85, 86, 101, 102, 149, 150, 165, 166, 169,
				170
			},
			new int[10] { 81, 85, 86, 101, 102, 149, 150, 165, 166, 170 },
			new int[13]
			{
				81, 82, 85, 86, 101, 102, 149, 150, 165, 166,
				167, 170, 171
			},
			new int[11]
			{
				84, 85, 89, 101, 105, 149, 153, 165, 166, 169,
				170
			},
			new int[19]
			{
				81, 84, 85, 86, 89, 101, 102, 105, 106, 149,
				150, 153, 154, 165, 166, 169, 170, 186, 234
			},
			new int[13]
			{
				81, 85, 86, 101, 102, 106, 149, 150, 154, 165,
				166, 169, 170
			},
			new int[14]
			{
				81, 85, 86, 90, 101, 102, 106, 149, 150, 154,
				165, 166, 170, 171
			},
			new int[10] { 84, 85, 89, 101, 105, 149, 153, 165, 169, 170 },
			new int[13]
			{
				84, 85, 89, 101, 105, 106, 149, 153, 154, 165,
				166, 169, 170
			},
			new int[16]
			{
				85, 86, 89, 90, 101, 102, 105, 106, 149, 150,
				153, 154, 165, 166, 169, 170
			},
			new int[14]
			{
				85, 86, 89, 90, 101, 102, 106, 149, 150, 154,
				166, 169, 170, 171
			},
			new int[13]
			{
				84, 85, 88, 89, 101, 105, 149, 153, 165, 169,
				170, 173, 174
			},
			new int[14]
			{
				84, 85, 89, 90, 101, 105, 106, 149, 153, 154,
				165, 169, 170, 174
			},
			new int[14]
			{
				85, 86, 89, 90, 101, 105, 106, 149, 153, 154,
				166, 169, 170, 174
			},
			new int[16]
			{
				85, 86, 89, 90, 102, 105, 106, 150, 153, 154,
				166, 169, 170, 171, 174, 175
			},
			new int[14]
			{
				80, 81, 84, 85, 97, 100, 101, 149, 165, 166,
				169, 170, 181, 186
			},
			new int[12]
			{
				81, 85, 97, 101, 102, 149, 165, 166, 169, 170,
				182, 186
			},
			new int[13]
			{
				81, 85, 86, 97, 101, 102, 149, 150, 165, 166,
				170, 182, 186
			},
			new int[15]
			{
				81, 85, 86, 101, 102, 106, 150, 165, 166, 167,
				170, 171, 182, 186, 187
			},
			new int[12]
			{
				84, 85, 100, 101, 105, 149, 165, 166, 169, 170,
				185, 186
			},
			new int[11]
			{
				85, 101, 102, 105, 106, 149, 165, 166, 169, 170,
				186
			},
			new int[14]
			{
				81, 85, 86, 101, 102, 105, 106, 149, 150, 165,
				166, 169, 170, 186
			},
			new int[13]
			{
				81, 85, 86, 101, 102, 106, 150, 165, 166, 170,
				171, 186, 187
			},
			new int[13]
			{
				84, 85, 89, 100, 101, 105, 149, 153, 165, 169,
				170, 185, 186
			},
			new int[14]
			{
				84, 85, 89, 101, 102, 105, 106, 149, 153, 165,
				166, 169, 170, 186
			},
			new int[14]
			{
				85, 86, 89, 101, 102, 105, 106, 149, 154, 165,
				166, 169, 170, 186
			},
			new int[16]
			{
				85, 86, 90, 101, 102, 105, 106, 150, 154, 165,
				166, 169, 170, 171, 186, 187
			},
			new int[15]
			{
				84, 85, 89, 101, 105, 106, 153, 165, 169, 170,
				173, 174, 185, 186, 190
			},
			new int[13]
			{
				84, 85, 89, 101, 105, 106, 153, 165, 169, 170,
				174, 186, 190
			},
			new int[16]
			{
				85, 89, 90, 101, 102, 105, 106, 153, 154, 165,
				166, 169, 170, 174, 186, 190
			},
			new int[15]
			{
				85, 86, 89, 90, 101, 102, 105, 106, 154, 166,
				169, 170, 171, 174, 186
			},
			new int[17]
			{
				64, 69, 81, 84, 85, 133, 145, 148, 149, 150,
				153, 154, 165, 166, 169, 170, 234
			},
			new int[15]
			{
				65, 69, 81, 85, 86, 133, 145, 149, 150, 153,
				154, 165, 166, 170, 234
			},
			new int[14]
			{
				65, 69, 81, 85, 86, 133, 145, 149, 150, 154,
				166, 170, 214, 234
			},
			new int[17]
			{
				65, 69, 81, 85, 86, 134, 146, 149, 150, 151,
				154, 166, 170, 171, 214, 234, 235
			},
			new int[15]
			{
				68, 69, 84, 85, 89, 133, 148, 149, 150, 153,
				154, 165, 169, 170, 234
			},
			new int[13]
			{
				69, 85, 133, 149, 150, 153, 154, 165, 166, 169,
				170, 218, 234
			},
			new int[12]
			{
				69, 85, 86, 133, 149, 150, 153, 154, 166, 170,
				218, 234
			},
			new int[14]
			{
				69, 85, 86, 134, 149, 150, 154, 155, 166, 170,
				171, 218, 234, 235
			},
			new int[14]
			{
				68, 69, 84, 85, 89, 133, 148, 149, 153, 154,
				169, 170, 217, 234
			},
			new int[12]
			{
				69, 85, 89, 133, 149, 150, 153, 154, 169, 170,
				218, 234
			},
			new int[13]
			{
				69, 85, 86, 89, 90, 133, 149, 150, 153, 154,
				170, 218, 234
			},
			new int[15]
			{
				69, 85, 86, 90, 149, 150, 153, 154, 155, 166,
				170, 171, 218, 234, 235
			},
			new int[17]
			{
				68, 69, 84, 85, 89, 137, 149, 152, 153, 154,
				157, 169, 170, 174, 217, 234, 238
			},
			new int[14]
			{
				69, 85, 89, 137, 149, 153, 154, 158, 169, 170,
				174, 218, 234, 238
			},
			new int[15]
			{
				69, 85, 89, 90, 149, 150, 153, 154, 158, 169,
				170, 174, 218, 234, 238
			},
			new int[17]
			{
				69, 85, 86, 89, 90, 149, 150, 153, 154, 155,
				158, 170, 171, 174, 218, 234, 239
			},
			new int[15]
			{
				80, 81, 84, 85, 101, 145, 148, 149, 150, 153,
				165, 166, 169, 170, 234
			},
			new int[13]
			{
				81, 85, 145, 149, 150, 153, 154, 165, 166, 169,
				170, 230, 234
			},
			new int[12]
			{
				81, 85, 86, 145, 149, 150, 154, 165, 166, 170,
				230, 234
			},
			new int[14]
			{
				81, 85, 86, 146, 149, 150, 154, 166, 167, 170,
				171, 230, 234, 235
			},
			new int[13]
			{
				84, 85, 148, 149, 150, 153, 154, 165, 166, 169,
				170, 233, 234
			},
			new int[10] { 85, 149, 150, 153, 154, 165, 166, 169, 170, 234 },
			new int[11]
			{
				85, 86, 149, 150, 153, 154, 165, 166, 169, 170,
				234
			},
			new int[10] { 85, 86, 149, 150, 154, 166, 170, 171, 234, 235 },
			new int[12]
			{
				84, 85, 89, 148, 149, 153, 154, 165, 169, 170,
				233, 234
			},
			new int[11]
			{
				85, 89, 149, 150, 153, 154, 165, 166, 169, 170,
				234
			},
			new int[14]
			{
				69, 85, 86, 89, 90, 149, 150, 153, 154, 165,
				166, 169, 170, 234
			},
			new int[13]
			{
				69, 85, 86, 90, 149, 150, 153, 154, 166, 170,
				171, 234, 235
			},
			new int[14]
			{
				84, 85, 89, 149, 152, 153, 154, 169, 170, 173,
				174, 233, 234, 238
			},
			new int[10] { 85, 89, 149, 153, 154, 169, 170, 174, 234, 238 },
			new int[13]
			{
				69, 85, 89, 90, 149, 150, 153, 154, 169, 170,
				174, 234, 238
			},
			new int[13]
			{
				85, 86, 89, 90, 149, 150, 153, 154, 170, 171,
				174, 234, 239
			},
			new int[14]
			{
				80, 81, 84, 85, 101, 145, 148, 149, 165, 166,
				169, 170, 229, 234
			},
			new int[12]
			{
				81, 85, 101, 145, 149, 150, 165, 166, 169, 170,
				230, 234
			},
			new int[13]
			{
				81, 85, 86, 101, 102, 145, 149, 150, 165, 166,
				170, 230, 234
			},
			new int[15]
			{
				81, 85, 86, 102, 149, 150, 154, 165, 166, 167,
				170, 171, 230, 234, 235
			},
			new int[12]
			{
				84, 85, 101, 148, 149, 153, 165, 166, 169, 170,
				233, 234
			},
			new int[11]
			{
				85, 101, 149, 150, 153, 154, 165, 166, 169, 170,
				234
			},
			new int[14]
			{
				81, 85, 86, 101, 102, 149, 150, 153, 154, 165,
				166, 169, 170, 234
			},
			new int[13]
			{
				81, 85, 86, 102, 149, 150, 154, 165, 166, 170,
				171, 234, 235
			},
			new int[13]
			{
				84, 85, 89, 101, 105, 148, 149, 153, 165, 169,
				170, 233, 234
			},
			new int[14]
			{
				84, 85, 89, 101, 105, 149, 150, 153, 154, 165,
				166, 169, 170, 234
			},
			new int[14]
			{
				85, 86, 89, 101, 106, 149, 150, 153, 154, 165,
				166, 169, 170, 234
			},
			new int[16]
			{
				85, 86, 90, 102, 106, 149, 150, 153, 154, 165,
				166, 169, 170, 171, 234, 235
			},
			new int[15]
			{
				84, 85, 89, 105, 149, 153, 154, 165, 169, 170,
				173, 174, 233, 234, 238
			},
			new int[13]
			{
				84, 85, 89, 105, 149, 153, 154, 165, 169, 170,
				174, 234, 238
			},
			new int[16]
			{
				85, 89, 90, 105, 106, 149, 150, 153, 154, 165,
				166, 169, 170, 174, 234, 238
			},
			new int[15]
			{
				85, 86, 89, 90, 106, 149, 150, 153, 154, 166,
				169, 170, 171, 174, 234
			},
			new int[17]
			{
				80, 81, 84, 85, 101, 149, 161, 164, 165, 166,
				169, 170, 181, 186, 229, 234, 250
			},
			new int[14]
			{
				81, 85, 101, 149, 161, 165, 166, 169, 170, 182,
				186, 230, 234, 250
			},
			new int[15]
			{
				81, 85, 101, 102, 149, 150, 165, 166, 169, 170,
				182, 186, 230, 234, 250
			},
			new int[17]
			{
				81, 85, 86, 101, 102, 149, 150, 165, 166, 167,
				170, 171, 182, 186, 230, 234, 251
			},
			new int[14]
			{
				84, 85, 101, 149, 164, 165, 166, 169, 170, 185,
				186, 233, 234, 250
			},
			new int[10] { 85, 101, 149, 165, 166, 169, 170, 186, 234, 250 },
			new int[13]
			{
				81, 85, 101, 102, 149, 150, 165, 166, 169, 170,
				186, 234, 250
			},
			new int[13]
			{
				85, 86, 101, 102, 149, 150, 165, 166, 170, 171,
				186, 234, 251
			},
			new int[15]
			{
				84, 85, 101, 105, 149, 153, 165, 166, 169, 170,
				185, 186, 233, 234, 250
			},
			new int[13]
			{
				84, 85, 101, 105, 149, 153, 165, 166, 169, 170,
				186, 234, 250
			},
			new int[16]
			{
				85, 101, 102, 105, 106, 149, 150, 153, 154, 165,
				166, 169, 170, 186, 234, 250
			},
			new int[15]
			{
				85, 86, 101, 102, 106, 149, 150, 154, 165, 166,
				169, 170, 171, 186, 234
			},
			new int[17]
			{
				84, 85, 89, 101, 105, 149, 153, 165, 169, 170,
				173, 174, 185, 186, 233, 234, 254
			},
			new int[13]
			{
				85, 89, 101, 105, 149, 153, 165, 169, 170, 174,
				186, 234, 254
			},
			new int[15]
			{
				85, 89, 101, 105, 106, 149, 153, 154, 165, 166,
				169, 170, 174, 186, 234
			},
			new int[20]
			{
				85, 86, 89, 90, 101, 102, 105, 106, 149, 150,
				153, 154, 165, 166, 169, 170, 171, 174, 186, 234
			}
		};
		LatticeVertex4D[] array5 = new LatticeVertex4D[256];
		for (int l = 0; l < 256; l++)
		{
			int xsv = (l & 3) - 1;
			int ysv = ((l >> 2) & 3) - 1;
			int zsv = ((l >> 4) & 3) - 1;
			int wsv = ((l >> 6) & 3) - 1;
			array5[l] = new LatticeVertex4D(xsv, ysv, zsv, wsv);
		}
		int num7 = 0;
		for (int m = 0; m < 256; m++)
		{
			num7 += array4[m].Length;
		}
		LOOKUP_4D_A = new(short, short)[256];
		LOOKUP_4D_B = new LatticeVertex4D[num7];
		int n = 0;
		int num8 = 0;
		for (; n < 256; n++)
		{
			LOOKUP_4D_A[n] = (SecondaryIndexStart: (short)num8, SecondaryIndexStop: (short)(num8 + array4[n].Length));
			for (int num9 = 0; num9 < array4[n].Length; num9++)
			{
				LOOKUP_4D_B[num8++] = array5[array4[n][num9]];
			}
		}
	}
}
