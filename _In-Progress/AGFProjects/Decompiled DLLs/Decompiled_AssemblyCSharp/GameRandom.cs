using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GameRandom : IMemoryPoolableObject
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int MBIG = int.MaxValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int MSEED = 161803398;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int MZ = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	public int inext;

	[PublicizedFrom(EAccessModifier.Private)]
	public int inextp;

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] SeedArray = new int[56];

	public double RandomDouble => NextDouble();

	public float RandomFloat => (float)NextDouble();

	public int RandomInt => Next();

	public Vector2 RandomInsideUnitCircle
	{
		get
		{
			float f = (float)NextDouble() * (MathF.PI * 2f);
			return new Vector2(Mathf.Cos(f), Mathf.Sin(f)) * (float)Math.Sqrt(NextDouble());
		}
	}

	public Vector2 RandomOnUnitCircle
	{
		get
		{
			float f = (float)NextDouble() * (MathF.PI * 2f);
			return new Vector2(Mathf.Cos(f), Mathf.Sin(f));
		}
	}

	public Vector3 RandomOnUnitCircleXZ
	{
		get
		{
			float f = (float)NextDouble() * (MathF.PI * 2f);
			return new Vector3(Mathf.Cos(f), 0f, Mathf.Sin(f));
		}
	}

	public Vector3 RandomInsideUnitSphere => new Vector3((float)(NextDouble() - 0.5), (float)(NextDouble() - 0.5), (float)(NextDouble() - 0.5)).normalized * (float)Math.Sqrt(NextDouble());

	public Vector3 RandomOnUnitSphere => new Vector3((float)(NextDouble() - 0.5), (float)(NextDouble() - 0.5), (float)(NextDouble() - 0.5)).normalized;

	public float RandomGaussian
	{
		get
		{
			float num;
			float num3;
			do
			{
				num = 2f * RandomRange(0f, 1f) - 1f;
				float num2 = 2f * RandomRange(0f, 1f) - 1f;
				num3 = num * num + num2 * num2;
			}
			while (num3 >= 1f || num3 == 0f);
			num3 = Mathf.Sqrt(-2f * Mathf.Log(num3) / num3);
			return num3 * num;
		}
	}

	public void SetSeed(int _seed)
	{
		InternalSetSeed(_seed);
	}

	public void SetLock()
	{
	}

	public void Cleanup()
	{
	}

	public void Reset()
	{
	}

	public float RandomRange(float _maxExclusive)
	{
		return (float)(NextDouble() * (double)_maxExclusive);
	}

	public float RandomRange(float _min, float _maxExclusive)
	{
		return (float)(NextDouble() * (double)(_maxExclusive - _min) + (double)_min);
	}

	public int RandomRange(int _maxExclusive)
	{
		return Next(_maxExclusive);
	}

	public int RandomRange(int _min, int _maxExclusive)
	{
		return Next(_maxExclusive - _min) + _min;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void log(string _format, params object[] _values)
	{
		Log.Warning($"{Time.time.ToCultureInvariantString()} GameRandom " + _format, _values);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InternalSetSeed(int Seed)
	{
		int num = ((Seed == int.MinValue) ? int.MaxValue : Math.Abs(Seed));
		int num2 = 161803398 - num;
		SeedArray[55] = num2;
		int num3 = 1;
		for (int i = 1; i < 55; i++)
		{
			int num4 = 21 * i % 55;
			SeedArray[num4] = num3;
			num3 = num2 - num3;
			if (num3 < 0)
			{
				num3 += int.MaxValue;
			}
			num2 = SeedArray[num4];
		}
		for (int j = 1; j < 5; j++)
		{
			for (int k = 1; k < 56; k++)
			{
				SeedArray[k] -= SeedArray[1 + (k + 30) % 55];
				if (SeedArray[k] < 0)
				{
					SeedArray[k] += int.MaxValue;
				}
			}
		}
		inext = 0;
		inextp = 21;
		Seed = 1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[PublicizedFrom(EAccessModifier.Private)]
	public double Sample()
	{
		return (double)InternalSample() * 4.656612875245797E-10;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int InternalSample()
	{
		int num = inext;
		int num2 = inextp;
		if (++num >= 56)
		{
			num = 1;
		}
		if (++num2 >= 56)
		{
			num2 = 1;
		}
		int num3 = SeedArray[num] - SeedArray[num2];
		if (num3 == int.MaxValue)
		{
			num3--;
		}
		if (num3 < 0)
		{
			num3 += int.MaxValue;
		}
		SeedArray[num] = num3;
		inext = num;
		inextp = num2;
		return num3;
	}

	public int PeekSample()
	{
		int num = inext;
		int num2 = inextp;
		if (++num >= 56)
		{
			num = 1;
		}
		if (++num2 >= 56)
		{
			num2 = 1;
		}
		int num3 = SeedArray[num] - SeedArray[num2];
		if (num3 == int.MaxValue)
		{
			num3--;
		}
		if (num3 < 0)
		{
			num3 += int.MaxValue;
		}
		return num3;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[PublicizedFrom(EAccessModifier.Private)]
	public int Next()
	{
		return InternalSample();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public double GetSampleForLargeRange()
	{
		int num = InternalSample();
		if (InternalSample() % 2 == 0)
		{
			num = -num;
		}
		return ((double)num + 2147483646.0) / 4294967293.0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int Next(int minValue, int maxValue)
	{
		if (minValue > maxValue)
		{
			throw new ArgumentOutOfRangeException("minValue", "Argument_MinMaxValue");
		}
		long num = (long)maxValue - (long)minValue;
		if (num <= int.MaxValue)
		{
			return (int)(Sample() * (double)num) + minValue;
		}
		return (int)((long)(GetSampleForLargeRange() * (double)num) + minValue);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int Next(int maxValue)
	{
		if (maxValue < 0)
		{
			throw new ArgumentOutOfRangeException("maxValue", "ArgumentOutOfRange_MustBePositive");
		}
		return (int)(Sample() * (double)maxValue);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[PublicizedFrom(EAccessModifier.Private)]
	public double NextDouble()
	{
		return Sample();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void NextBytes(byte[] buffer)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		for (int i = 0; i < buffer.Length; i++)
		{
			buffer[i] = (byte)(InternalSample() % 256);
		}
	}
}
